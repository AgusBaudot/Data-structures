using System;
using System.Collections.Generic;
using Weapons;

namespace Weapons
{
    public enum Type
    {
        Sword,
        Bow,
        Staff,
        Gun
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Epic,
        Legendary
    }
}

public class Item : IComparable<Item>
{
    public int ID { get; private set; }
    public string Name { get; private set; }
    public int Price { get; private set; }
    public Weapons.Type Type { get; private set; }
    
    public Rarity Rarity { get; private set; }

    public Item(int id, string name, int price, Weapons.Type type,  Rarity rarity)
    {
        ID = id;
        Name = name;
        Price = price;
        Type = type;
        Rarity = rarity;
    }

    public int CompareTo(Item other) => ID.CompareTo(other.ID);

    #region Custom comparers

    public class NameComparer : IComparer<Item>
    {
        public int Compare(Item x, Item y) => String.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }

    public class PriceComparer : IComparer<Item>
    {
        public int Compare(Item x, Item y) => x.Price.CompareTo(y.Price);
    }

    public class TypeComparer : IComparer<Item>
    {
        public int Compare(Item x, Item y)
        {
            if (x == null || y == null) return 0;
            
            int typeCompare = x.Type.CompareTo(y.Type);
            if (typeCompare != 0) return typeCompare;
            
            return x.ID.CompareTo(y.ID);
        }
    }

    public class RarityComparer : IComparer<Item>
    {
        public int Compare(Item x, Item y)
        {
            if (x == null || y == null) return 0;

            int rarityCompare = x.Rarity.CompareTo(y.Rarity);
            if (rarityCompare != 0) return rarityCompare;

            return x.ID.CompareTo(y.ID);
        }
    }

    #endregion
}
