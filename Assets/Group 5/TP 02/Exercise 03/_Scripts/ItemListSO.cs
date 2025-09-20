using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemListSO", menuName = "ScriptableObject/Shop/Item list")]
public class ItemListSO : ScriptableObject
{
    public List<ItemSO> AllItems;

    private Dictionary<int, ItemSO> _itemLookUp;

    public void Init()
    {
        _itemLookUp = new Dictionary<int, ItemSO>();

        // CAUTION: doing it this way won't override their assets (since it is done at runtime), so if we want their ID's to be permanent and persist to disk, change to editor persistence.
        int nextID = 1;
        for (int i = 0; i < AllItems.Count; i++)
        {
            var item = AllItems[i];
            if (item == null) continue;

            //Pick first free new ID.
            if (item.ID <= 0 || _itemLookUp.ContainsKey(item.ID))
            {
                while (_itemLookUp.ContainsKey(nextID)) nextID++;
                item.ID = nextID;
                nextID++;
            }

            _itemLookUp[item.ID] = item;
        }
    }

    public ItemSO GetItemByID(int id)
    {
        if (_itemLookUp == null) Init();
        return _itemLookUp.TryGetValue(id, out var item) ? item : null;
    }
}