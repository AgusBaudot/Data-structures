using System.Collections.Generic;
using UnityEngine;

public class Shop : Object
{
    [SerializeField] private Transform itemContainer;
    
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
        { SortOption.ID , Comparer<Item>.Default},
        {SortOption.Name, new Item.NameComparer()},
        { SortOption.Price, new Item.PriceComparer()},
        {SortOption.Type, new Item.TypeComparer()},
        {SortOption.Rarity, new Item.RarityComparer()}
    };

    private void Start()
    {
        //Initialize shop dictionary with every 
        UpdateUI();
    }

    public void Buy(Item item)
    {
        if (!_shopItems.ContainsValue(item) || _money < item.Price) return;
        
        _shopItems.Remove(item.ID);
        _playerItems.Add(item.ID, item);
        _money -= item.Price;
        UpdateUI();
    }

    public void Sell(Item item)
    {
        _playerItems.Remove(item.ID);
        _shopItems.Add(item.ID, item);
        _money += item.Price;
        UpdateUI();
    }

    public SimpleList<Item> GetItemsSorted(IComparer<Item> comparer)
    {
        SimpleList<Item> items = new SimpleList<Item>();
        items.Sort(comparer);
        return items;
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
        
        // //Destroy old UI
        // foreach (Transform child in itemContainer)
        //     Destroy(child.gameObject);
        //
        // //Sort items using the currently selected comparer
        // SimpleList<Item> sortedItems = new SimpleList<Item>(_shopItems.Values);
        // sortedItems.Sort(_comparers[_currentSortOption]);
        //
        // //Rebuild UI
        // foreach (Item item in sortedItems)
        // {
        //     GameObject uiObj = Instantiate(itemPrefab, itemContainer);
        //     
        //     
        // }
    }

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
        _currentSortOption = (SortOption)index;
        UpdateUI();
        // IComparer<Item> comparer = index switch
        // {
        //     0 => new Item.NameComparer(),
        //     1 => new Item.PriceComparer(),
        //     2 => new Item.TypeComparer(),
        //     3 => new Item.RarityComparer(),
        //     _ => null
        // };
    }
}