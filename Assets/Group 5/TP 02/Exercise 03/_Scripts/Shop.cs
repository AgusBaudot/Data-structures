using Sortings;
using System.Collections.Generic;
using ShopContext;
using TMPro;
using UnityEngine;
using static Comparers;

public class Shop : MonoBehaviour
{
    [SerializeField] private Transform shopContainer;
    [SerializeField] private Transform playerContainer;
    [SerializeField] private ShopItemUI itemPrefab;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private ItemListSO itemDataBase;

    private Dictionary<int, int> _shopInventory = new();
    private Dictionary<int, int> _playerInventory = new();
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

    private void InitializeShop()
    {
        itemDataBase.Init();

        foreach (var item in itemDataBase.AllItems)
        {
            //Set initial amount of each item randomly
            int amount = Random.Range(1, 5);
            _shopInventory[item.ID] = amount;
            
            //Shop panel entry
            ShopItemUI shopUI = Instantiate(itemPrefab, shopContainer);
            shopUI.Setup(item, shop: this, ItemContext.Shop);
            
            //Player panel entry (start empty)
            ShopItemUI playerUI = Instantiate(itemPrefab, playerContainer);
            playerUI.Setup(item, shop: this, ItemContext.Player);
        }

        UpdateMoney();
        UpdateUI();

        //foreach (var kvp in itemDataBase.AllItems)
        //{
        //    _shopItemSOs.Add(kvp.Key, kvp.Value);

        //    GameObject uiObj = Instantiate(ItemSOPrefab, ItemSOContainer);
        //    ShopItemSOUI ui = uiObj.GetComponent<ShopItemSOUI>();
        //    ui.Setup(kvp.Value, this);
        //}

    }

    // private void CreateItemUI(ItemSO item)
    // {
    //     ShopItemUI ui = Instantiate(itemPrefab, itemContainer);
    //     ui.Setup(item, this);
    // }

    public void Buy(ItemSO item)
    {
        if (!_shopInventory.ContainsKey(item.ID) || _money < item.Price)
            return;

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
        foreach (Transform child in shopContainer)
        {
            UpdateAmount(child);
        }

        foreach (Transform child in playerContainer)
        {
            UpdateAmount(child);
        }

        // SimpleList<ItemSO> sortedItems = new();
        //
        // foreach (int id in _shopInventory.Keys)
        // {
        //     ItemSO item = itemDataBase.GetItemByID(id);
        //     if (item != null)
        //         sortedItems.Add(item);
        // }
        //
        // sortedItems.Sort(_comparers[_currentSortOption]);
        //
        // for (int i = 0; i < sortedItems.Count; i++)
        // {
        //     ItemSO itemSO =  sortedItems[i];
        //     ShopItemUI ui = FindUIForItemSO(itemSO);
        //     
        //     if (ui != null)
        //         ui.transform.SetSiblingIndex(i);
        //     int shopStock = _shopInventory.TryGetValue(itemSO.ID, out var s) ? s : 0;
        //     int playerStock = _playerInventory.TryGetValue(itemSO.ID, out var p) ? p : 0;
        //     ui.Refresh(shopStock, playerStock);
        // }
    }

    private void UpdateAmount(Transform child)
    {
        ShopItemUI ui = child.GetComponent<ShopItemUI>();
        if (ui != null)
        {
            int shopStock = _shopInventory.TryGetValue(ui.ItemID, out var s) ? s : 0;
            int playerStock = _playerInventory.TryGetValue(ui.ItemID, out var p) ? p : 0;
            ui.Refresh(shopStock, playerStock);
        }
    }


    private ShopItemUI FindUIForItemSO(ItemSO itemSO, ItemContext context)
    {
        Transform container = context == ItemContext.Shop ? shopContainer : playerContainer;
        foreach (Transform child in container)
        {
            ShopItemUI ui = child.GetComponent<ShopItemUI>();
            if (ui != null && ui.ItemID == itemSO.ID)
                return ui;
        }
    
        return null;
    }

    private void SortUI(IComparer<ItemSO> comparer, ItemContext context)
    {
        SimpleList<ItemSO> items = new();
        //Select source according to context
        Dictionary<int, int> source = context == ItemContext.Shop
            ? _shopInventory
            : _playerInventory;
        
        //Populate list
        foreach (int id in source.Keys)
        {
            ItemSO item = itemDataBase.GetItemByID(id);
            if (item != null)
                items.Add(item);
        }
        
        //Custom sort: first by "owned/sellable" in this context, then by selected comparer
        items.Sort((a, b) =>
        {
            int aAmount = GetContextAmount(a.ID, context);
            int bAmount = GetContextAmount(b.ID, context);

            bool aHasStock = aAmount > 0;
            bool bHasStock = bAmount > 0;

            //Put items with 0 stock at the bottom
            if (aHasStock != bHasStock)
                return aHasStock ? -1 : 1;

            //Otherwise sort by chosen comparer
            return comparer.Compare(a, b);
        });
        
        //Reorder UI
        for (int i = 0; i < items.Count; i++)
        {
            ShopItemUI ui = FindUIForItemSO(items[i], context);
            if (ui != null)
                ui.transform.SetSiblingIndex(i);
        }
    }

    private int GetContextAmount (int itemID, ItemContext context)
    {
        return context switch
        {
            ItemContext.Shop => _shopInventory.TryGetValue(itemID, out var s) ? s : 0,
            ItemContext.Player => _playerInventory.TryGetValue(itemID, out var p) ? p : 0,
            _ => 0
        };
    }

    public void OnComparerChanged(int index, ItemContext context)
    {
        if (_currentSortOption == (SortOption)index) return;
        _currentSortOption = (SortOption)index;
        SortUI(_comparers[_currentSortOption], context);
        UpdateUI();
    }
}