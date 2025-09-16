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
        foreach (var item in AllItems)
        {
            if (!_itemLookUp.ContainsKey(item.ID))
                _itemLookUp.Add(item.ID, item);
            else
                Debug.LogWarning($"Duplicate item ID {item.ID} found!");
        }
    }

    public ItemSO GetItemByID(int id)
    {
        if (_itemLookUp == null) Init();
        return _itemLookUp.TryGetValue(id, out var item) ? item : null;
    }
}