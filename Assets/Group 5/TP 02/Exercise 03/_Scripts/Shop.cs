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
    private int _money = 500;
    private SortOption _currentPlayerSort = SortOption.ID;
    private SortOption _currentShopSort = SortOption.ID;

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
    }

    public void Buy(ItemSO item)
    {
        int shopBefore = GetContextAmount(item.ID, ItemContext.Shop);
        int playerBefore = GetContextAmount(item.ID, ItemContext.Player);
        
        if (shopBefore == 0 || _money < item.Price) return;

        _shopInventory[item.ID] = shopBefore - 1;
        
        if (_shopInventory[item.ID] <= 0)
            _shopInventory.Remove(item.ID);

        if (_playerInventory.ContainsKey(item.ID))
            _playerInventory[item.ID]++;
        else 
            _playerInventory[item.ID] = 1;

        _money -= item.Price;
        UpdateMoney();

        int shopAfter = GetContextAmount(item.ID, ItemContext.Shop);
        int playerAfter = GetContextAmount(item.ID, ItemContext.Player);
        
        MaybeReorderAfterChange(shopBefore, shopAfter, playerBefore, playerAfter);
        
        UpdateUI();
    }

    public void Sell(ItemSO item)
    {
        if (!_playerInventory.ContainsKey(item.ID)) return;

        int shopBefore = GetContextAmount(item.ID, ItemContext.Shop);
        int playerBefore = GetContextAmount(item.ID, ItemContext.Player);

        _playerInventory[item.ID] = playerBefore - 1;
        if (_playerInventory[item.ID] <= 0) _playerInventory.Remove(item.ID);

        if (_shopInventory.ContainsKey(item.ID))
            _shopInventory[item.ID]++;
        else 
            _shopInventory[item.ID] = 1;

        _money += item.Price;
        UpdateMoney();

        int shopAfter = GetContextAmount(item.ID, ItemContext.Shop);
        int playerAfter = GetContextAmount(item.ID, ItemContext.Player);
        
        MaybeReorderAfterChange(shopBefore, shopAfter, playerBefore, playerAfter);
        
        UpdateUI();
    }

    private void UpdateMoney() => moneyText.text = _money.ToString();

    private void UpdateUI()
    {
        UpdateUIContext(ItemContext.Shop);
        UpdateUIContext(ItemContext.Player);
    }

    private void UpdateUIContext(ItemContext context)
    {
        Transform container = context == ItemContext.Player ? playerContainer : shopContainer;
        
        foreach (Transform child in container)
            UpdateAmount(child);
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

        comparer ??= Comparer<ItemSO>.Default;
        
        //Populate list
        foreach (var item in itemDataBase.AllItems)
        {
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
    
    private void MaybeReorderAfterChange(int shopBefore, int shopAfter, int playerBefore, int playerAfter)
    {
        // Shop context transitions
        if (shopBefore == 0 && shopAfter > 0)
        {
            // item appeared in shop (was zero, now non-zero) -> bring it above empty items
            SortUI(_comparers[_currentShopSort], ItemContext.Shop);
        }
        else if (shopBefore > 0 && shopAfter == 0)
        {
            // item went to zero in shop -> push it to bottom
            SortUI(_comparers[_currentShopSort], ItemContext.Shop);
        }

        // Player context transitions
        if (playerBefore == 0 && playerAfter > 0)
        {
            // player gained a previously zero item -> bring it above empty player items
            SortUI(_comparers[_currentPlayerSort], ItemContext.Player);
        }
        else if (playerBefore > 0 && playerAfter == 0)
        {
            // player lost their last of this item -> push it to bottom
            SortUI(_comparers[_currentPlayerSort], ItemContext.Player);
        }
    }

    public void OnShopComparerChanged(int index)
    {
        if ((int)_currentShopSort == index) return;
        _currentShopSort = (SortOption)index;
        SortUI(_comparers[_currentShopSort], ItemContext.Shop);
        UpdateUI();
    }

    public void OnPlayerComparerChanged(int index)
    {
        if ((int)_currentPlayerSort == index) return;
        _currentPlayerSort = (SortOption)index;
        SortUI(_comparers[_currentPlayerSort], ItemContext.Player);
        UpdateUI();
    }
}