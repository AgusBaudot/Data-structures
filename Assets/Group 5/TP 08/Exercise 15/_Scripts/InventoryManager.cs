using System.Collections.Generic;
using TMPro;
using UnityEngine;

// InventoryManager: two inventories (A, B) shown in left/right panels; central panel shows set results.
public class InventoryManager : MonoBehaviour
{
    [Header("ScrollViews (Content Transforms)")]
    [SerializeField] private Transform contentA;      // Content transform for inventory A (left)
    [SerializeField] private Transform contentB;      // Content transform for inventory B (right)
    [SerializeField] private Transform contentCenter; // Content transform for central viewport (results)

    [Header("Item entry prefab")]
    [SerializeField] private GameObject itemEntryPrefab; // prefab with a TextMeshPro child

    [Header("Info text fields")]
    [SerializeField] private TMP_Text infoTextA;
    [SerializeField] private TMP_Text infoTextB;
    [SerializeField] private TMP_Text infoTextCenter; // shows current operation summary

    private MySetList<Item> _inventoryA;
    private MySetList<Item> _inventoryB;
    private List<Item> _allItems; // 40 items pool

    private const int InventorySlots = 20;
    private const float OccupiedProbability = 0.7f;

    private void Start()
    {
        if (itemEntryPrefab == null || contentA == null || contentB == null || contentCenter == null)
        {
            Debug.LogError("InventoryManager: assign prefab and content parents in inspector.");
            return;
        }

        GenerateItems();
        _inventoryA = new MySetList<Item>();
        _inventoryB = new MySetList<Item>();

        PopulateRandomInventory(_inventoryA);
        PopulateRandomInventory(_inventoryB);

        // initial display
        RefreshAllViews();
    }

    private void GenerateItems()
    {
        _allItems = new List<Item>(40);
        for (int i = 0; i < 40; i++)
        {
            string name = $"Item {i + 1:00}";
            float price = Random.Range(5f, 200f);
            _allItems.Add(new Item(name, price));
        }
    }

    private void PopulateRandomInventory(MySetList<Item> inv)
    {
        for (int slot = 0; slot < InventorySlots; slot++)
        {
            if (Random.value <= OccupiedProbability)
            {
                Item pick = _allItems[Random.Range(0, _allItems.Count)];
                // MySet should ignore duplicates; Add only once
                if (!inv.Contains(pick))
                    inv.Add(pick);
            }
        }
    }

    // --------- Primary UI entry points (wire to buttons) ---------
    public void ShowInventoryA()
    {
        infoTextCenter.text = "Inventory A (left panel)";
        PopulateScrollView(contentA, _inventoryA);
        // keep B and center unchanged
        infoTextA.text = $"Inventory A: {_inventoryA.Cardinality}";
    }

    public void ShowInventoryB()
    {
        infoTextCenter.text = "Inventory B (right panel)";
        PopulateScrollView(contentB, _inventoryB);
        infoTextB.text = $"Inventory B: {_inventoryB.Cardinality}";
    }

    public void ShowUnion()
    {
        var union = _inventoryA.Union(_inventoryB);
        infoTextCenter.text = $"Union (A union B): {union.Cardinality} items";
        PopulateScrollView(contentCenter, union);
    }

    public void ShowIntersection()
    {
        var inter = _inventoryA.Intersect(_inventoryB);
        infoTextCenter.text = $"Intersection (A intersection B): {inter.Cardinality} items";
        PopulateScrollView(contentCenter, inter);
    }

    public void ShowDifferenceAminusB()
    {
        var diff = _inventoryA.Difference(_inventoryB); // A \ B
        infoTextCenter.text = $"A \\ B: {diff.Cardinality} items";
        PopulateScrollView(contentCenter, diff);
    }

    public void ShowDifferenceBminusA()
    {
        var diff = _inventoryB.Difference(_inventoryA); // B \ A
        infoTextCenter.text = $"B \\ A: {diff.Cardinality} items";
        PopulateScrollView(contentCenter, diff);
    }

    public void ShowMissingInBoth()
    {
        var union = _inventoryA.Union(_inventoryB);
        var missing = new MySetList<Item>();
        foreach (var it in _allItems)
            if (!union.Contains(it))
                missing.Add(it);

        infoTextCenter.text = $"In none: {missing.Cardinality} items";
        PopulateScrollView(contentCenter, missing);
    }

    public void ShowCounts()
    {
        infoTextCenter.text = "Counts";
        infoTextA.text = $"Inventory A: {_inventoryA.Cardinality}";
        infoTextB.text = $"Inventory B: {_inventoryB.Cardinality}";
        // central stays as-is (or you can clear it)
    }

    // Utility to refresh everything (call after buys/sells or start)
    private void RefreshAllViews()
    {
        PopulateScrollView(contentA, _inventoryA);
        PopulateScrollView(contentB, _inventoryB);
        contentCenter.SafeClearChildren();
        infoTextA.text = $"Inventory A: {_inventoryA.Cardinality}";
        infoTextB.text = $"Inventory B: {_inventoryB.Cardinality}";
        infoTextCenter.text = "Central viewport: results here";
    }

    // --------- UI helpers ----------
    private void PopulateScrollView(Transform contentParent, IEnumerable<Item> items)
    {
        // clear previous children
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // instantiate entries
        foreach (var item in items)
        {
            var go = Instantiate(itemEntryPrefab, contentParent);
            var tmp = go.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
                tmp.text = $"{item.Name} - ${item.Price:F2}";
        }
    }
}

// simple item class
[System.Serializable]
public class Item
{
    public string Name;
    public float Price;

    public Item(string name, float price)
    {
        Name = name;
        Price = price;
    }

    public override string ToString() => $"{Name} (${Price:F2})";

    public override bool Equals(object obj)
    {
        if (obj is Item other) return string.Equals(Name, other.Name);
        return false;
    }

    public override int GetHashCode() => Name != null ? Name.GetHashCode() : 0;
}

// small extension helper to clear children safely
public static class TransformExtensions
{
    public static void SafeClearChildren(this Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(t.GetChild(i).gameObject);
#else
            Object.Destroy(t.GetChild(i).gameObject);
#endif
        }
    }
}
