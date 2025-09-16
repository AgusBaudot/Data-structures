using System.Collections.Generic;
using UnityEngine;

public class Comparers : MonoBehaviour
{
    public static class ItemSOComparers
    {
        public static readonly IComparer<ItemSO> ID = Comparer<ItemSO>.Create((x, y) => x.ID.CompareTo(y.ID));
        public static readonly IComparer<ItemSO> Name = Comparer<ItemSO>.Create((x, y) => string.Compare(x.Name, y.Name));
        public static readonly IComparer<ItemSO> Price = Comparer<ItemSO>.Create((x, y) => x.Price.CompareTo(y.Price));
        public static readonly IComparer<ItemSO> Type = Comparer<ItemSO>.Create((x, y) => x.Type.CompareTo(y.Type));
        public static readonly IComparer<ItemSO> Rarity = Comparer<ItemSO>.Create((x, y) => x.Rarity.CompareTo(y.Rarity));
    }
}
