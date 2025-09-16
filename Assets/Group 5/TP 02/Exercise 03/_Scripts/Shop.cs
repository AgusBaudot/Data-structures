using Sortings;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Comparers;

public class Shop : MonoBehaviour
{
    [SerializeField] private Transform itemContainer;
    [SerializeField] private ShopItemUI itemPrefab;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private ItemListSO itemDataBase;

    private Dictionary<int, int> _shopInventory = new();
    private Dictionary<int, int> _playerInventory = new();
    private Dictionary<int, ShopItemUI> _itemUIMap = new();
    private int _money = 200;
    private SortOption _currentSortOption = SortOption.ID;

    private readonly Dictionary<SortOption, IComparer<ItemSO>> _comparers = new()
    {
        { SortOption.ID, ItemSOComparers.ID },
        { SortOption.Name, ItemSOComparers.Name },
        { SortOption.Price, ItemSOComparers.Price },
        { SortOption.Type, ItemSOComparers.Type },
        { SortOption.Rarity, ItemSOComparers.Rarity },
    };

    private void Start()
    {
        InitializeShop();
    }

    public SimpleList<ItemSO> GetItemSOsSorted(IComparer<ItemSO> comparer)
    {
        SimpleList<ItemSO> ItemSOs = new SimpleList<ItemSO>();
        ItemSOs.Sort(comparer);
        return ItemSOs;
    }

    private void InitializeShop()
    {
        itemDataBase.Init();

        foreach (var item in itemDataBase.AllItems)
        {
            int amount = Random.Range(1, 5);
            _shopInventory[item.ID] = amount;
            CreateItemUI(item, amount);
        }

        UpdateMoney();

        //foreach (var kvp in itemDataBase.AllItems)
        //{
        //    _shopItemSOs.Add(kvp.Key, kvp.Value);

        //    GameObject uiObj = Instantiate(ItemSOPrefab, ItemSOContainer);
        //    ShopItemSOUI ui = uiObj.GetComponent<ShopItemSOUI>();
        //    ui.Setup(kvp.Value, this);
        //}

        //UpdateUI();
    }

    private void CreateItemUI(ItemSO item, int amount)
    {
        ShopItemUI ui = Instantiate(itemPrefab, itemContainer);
        ui.Setup(item, this);
    }

    public bool Buy(ItemSO item)
    {
        if (!_shopInventory.ContainsKey(item.ID) || _money < item.Price)
            return false;

        _shopInventory[item.ID]--;

        if (_shopInventory[item.ID] <= 0) 
            _shopInventory.Remove(item.ID);

        //_playerInventory[item.ID] = _playerInventory.ContainsKey(item.ID) ? _playerInventory[item.ID] + 1 : 1;

        if (_playerInventory.ContainsKey(item.ID)) 
            _playerInventory[item.ID]++;
        else 
            _playerInventory[item.ID] = 1;

        _money -= item.Price;
        UpdateMoney();
        UpdateUI();
        return true;
        //if (!_shopItemSOs.ContainsValue(ItemSO.ID) || _money < ItemSO.Price) return false;

        //_shopItemSOs.Remove(ItemSO.ID);
        //_playerItemSOs.Add(ItemSO.ID, ItemSO.ID);
        //_money -= ItemSO.Price;
        //UpdateMoney();
        //UpdateUI();
        //return true;
    }

    public void Sell(ItemSO item)
    {
        if (!_playerInventory.ContainsKey(item.ID)) return;

        _playerInventory[item.ID]--;
        if (_playerInventory[item.ID] <= 0) _playerInventory.Remove(item.ID);

        if (_shopInventory.ContainsKey(item.ID)) _shopInventory[item.ID]++;
        else _shopInventory[item.ID] = 1;

        _money += item.Price;
        UpdateMoney();
        UpdateUI();
        //_playerItemSOs.Remove(ItemSO.ID);
        //_shopItemSOs.Add(ItemSO.ID, ItemSO.ID);
        //_money += ItemSO.Price;
        //UpdateMoney();
        //UpdateUI();
    }

    private void UpdateMoney() => moneyText.text = _money.ToString();

    private void UpdateUI()
    {
        SimpleList<ItemSO> sortedItems = new();
        
        foreach (int id in _shopInventory.Keys)
        {
            ItemSO item = itemDataBase.GetItemByID(id);
            if (item != null)
                sortedItems.Add(item);
        }

        sortedItems.Sort(_comparers[_currentSortOption]);

        for (int i = 0; i < sortedItems.Count; i++)
        {
            ItemSO itemSO = sortedItems[i];

            // Find the child UI representing this ItemSO.
            ShopItemUI ui = FindUIForItemSO(itemSO);
            if (ui != null)
                ui.transform.SetSiblingIndex(i);
        }
    }


    private ShopItemUI FindUIForItemSO(ItemSO itemSO)
    {
        if (_itemUIMap.TryGetValue(itemSO.ID, out var ui))
            return ui;

        foreach (Transform child in itemContainer)
        {
            ShopItemUI foundUI = child.GetComponent<ShopItemUI>();
            if (foundUI != null && foundUI.ItemID == itemSO.ID)
            {
                _itemUIMap[itemSO.ID] = foundUI;
                return foundUI;
            }
        }

        return null;
    }

    public void OnComparerChanged(int index)
    {
        if (_currentSortOption == (SortOption)index) return;
        _currentSortOption = (SortOption)index;
        UpdateUI();
    }
}