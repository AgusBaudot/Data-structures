using System.Collections.Generic;
using UnityEngine;
using Weapons;
using TMPro;

public class Shop : MonoBehaviour
{
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private TextMeshProUGUI moneyText;
    // [SerializeField] private Sprite swordSprite;
    // [SerializeField] private Sprite bowSprite;
    // [SerializeField] private Sprite gunSprite;
    // [SerializeField] private Sprite staffSprite;
    
    private enum SortOption
    {
        ID,
        Name, 
        Price, 
        Type,
        Rarity
    }

    private Dictionary<int, Item> _shopItems = new Dictionary<int, Item>();
    private Dictionary<int, Item> _playerItems = new Dictionary<int, Item>();
    private int _money = 200;
    private SortOption _currentSortOption = SortOption.ID;

    private readonly Dictionary<SortOption, IComparer<Item>> _comparers = new Dictionary<SortOption, IComparer<Item>>()
    {
        {SortOption.ID , Comparer<Item>.Default},
        {SortOption.Name, new Item.NameComparer()},
        {SortOption.Price, new Item.PriceComparer()},
        {SortOption.Type, new Item.TypeComparer()},
        {SortOption.Rarity, new Item.RarityComparer()}
    };

    private void Start()
    {
        InitializeShop();
    }

    public void Buy(Item item)
    {
        if (!_shopItems.ContainsValue(item) || _money < item.Price) return;
        
        _shopItems.Remove(item.ID);
        _playerItems.Add(item.ID, item);
        _money -= item.Price;
        UpdateMoney();
        UpdateUI();
    }

    public void Sell(Item item)
    {
        _playerItems.Remove(item.ID);
        _shopItems.Add(item.ID, item);
        _money += item.Price;
        UpdateMoney();
        UpdateUI();
    }

    public SimpleList<Item> GetItemsSorted(IComparer<Item> comparer)
    {
        SimpleList<Item> items = new SimpleList<Item>();
        items.Sort(comparer);
        return items;
    }

    private void InitializeShop()
    {
        foreach (var kvp in ItemDatabase.Items)
        {
            _shopItems.Add(kvp.Key, kvp.Value);
            
            GameObject uiObj = Instantiate(itemPrefab, itemContainer);
            ShopItemUI ui = uiObj.GetComponent<ShopItemUI>();
            ui.Setup(kvp.Value, this);
        }
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        SimpleList<Item> sortedItems = new SimpleList<Item>(_shopItems.Values);
        sortedItems.Sort(_comparers[_currentSortOption]);

        for (int i = 0; i < sortedItems.Count; i++)
        {
            Item item = sortedItems[i];
            
            //Find the child UI representing this item.
            ShopItemUI ui = FindUIForItem(item);
            if (ui != null)
                ui.transform.SetSiblingIndex(i);
        }
    }

    private void UpdateMoney() => moneyText.text = _money.ToString();

    private ShopItemUI FindUIForItem(Item item)
    {
        foreach (Transform child in itemContainer)
        {
            ShopItemUI ui = child.GetComponent<ShopItemUI>();
            if (ui != null && ui.ItemID == item.ID)
                return ui;
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