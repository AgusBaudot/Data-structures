using System.Collections.Generic;
using UnityEngine;
using Weapons;

public static class ItemDatabase
{
    public static readonly Dictionary<int, Item> Items = new()
    {
        {1, new Item(1, "Common sword", 25, Type.Sword, Rarity.Common, LoadIcon("Common sword", Type.Sword))},
        {2, new Item(2, "Uncommon sword", 50, Type.Sword, Rarity.Uncommon, LoadIcon("Uncommon sword",  Type.Sword))},
        {3, new Item(3, "Epic sword", 100, Type.Sword, Rarity.Epic,  LoadIcon("Epic sword", Type.Sword))},
        {4, new Item(4, "Legendary sword", 200, Type.Sword, Rarity.Legendary,  LoadIcon("Legendary sword", Type.Sword))},
        {5, new Item(5, "Common bow", 25, Type.Bow, Rarity.Common, LoadIcon("Common bow",  Type.Bow))},
        {6, new Item(6, "Uncommon bow", 50, Type.Bow, Rarity.Uncommon, LoadIcon("Uncommon bow", Type.Bow))},
        {7, new Item(7, "Epic bow", 100, Type.Bow, Rarity.Epic, LoadIcon("Epic bow", Type.Bow))},
        {8, new Item(8, "Legendary bow", 200, Type.Bow, Rarity.Legendary,  LoadIcon("Legendary bow", Type.Bow))},
        {9, new Item(9, "Common gun", 25, Type.Gun, Rarity.Common, LoadIcon("Common gun", Type.Gun))},
        {10, new Item(10, "Uncommon gun", 50, Type.Gun, Rarity.Uncommon, LoadIcon("Uncommon gun", Type.Gun))},
        {11, new Item(11, "Epic gun", 100, Type.Gun, Rarity.Epic, LoadIcon("Epic gun", Type.Gun))},
        {12, new Item(12, "Legendary gun", 200, Type.Gun, Rarity.Legendary, LoadIcon("Legendary gun", Type.Gun))},
        {13, new Item(13, "Common staff", 25, Type.Staff, Rarity.Common, LoadIcon("Common staff", Type.Staff))},
        {14, new Item(14, "Uncommon staff", 50, Type.Staff, Rarity.Uncommon,  LoadIcon("Uncommon staff", Type.Staff))},
        {15, new Item(15, "Epic staff", 100, Type.Staff, Rarity.Epic,  LoadIcon("Epic staff", Type.Staff))},
        {16, new Item(16, "Legendary staff", 200, Type.Staff, Rarity.Legendary, LoadIcon("Legendary staff", Type.Staff))},
    };

    public static readonly Dictionary<Type, Sprite> SoldItems = new()
    {
        { Type.Sword, LoadSoldSprite("Sword")},
        { Type.Bow, LoadSoldSprite("Bow")},
        { Type.Gun, LoadSoldSprite("Gun")},
        { Type.Staff, LoadSoldSprite("Staff")}
    };

    private static Sprite LoadIcon(string spriteName, Type type)
    {
        Sprite[] sprites = type switch
        {
            Type.Sword => Resources.LoadAll<Sprite>("TP 03/Sword"),
            Type.Bow => Resources.LoadAll<Sprite>("TP 03/Bow"),
            Type.Staff => Resources.LoadAll<Sprite>("TP 03/Staff"),
            Type.Gun => Resources.LoadAll<Sprite>("TP 03/Gun"),
            _ => new Sprite[16]
        };
        foreach (var sprite in sprites)
        {
            if (sprite.name == spriteName)
                return sprite;
        }
        Debug.LogWarning($"Sprite {spriteName} not found in Assets.");
        return null;
    }

    private static Sprite LoadSoldSprite(string spriteName)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("TP 03/Sold items");
        foreach (var sprite in sprites)
        {
            if (sprite.name == spriteName)
                return sprite;
        }
        Debug.LogWarning($"Sprite {spriteName} not found in Assets.");
        return null;
    }
}
