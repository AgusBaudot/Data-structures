using System;
using System.Collections.Generic;

public class MySetList<T> : MySet<T>
{
    private readonly SimpleList<T> _items;

    public MySetList(int capacity = 8)
    {
        _items = new SimpleList<T>(Math.Max(0, capacity));
    }

    public override int Cardinality => _items.Count;

    public override bool Add(T item)
    {
        if (Contains(item)) return false;
        _items.Add(item);
        return true;
    }

    public override bool Remove(T item) =>  _items.Remove(item);

    public override void Clear() => _items.Clear();

    public override bool Contains(T item)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (Comparer.Equals(_items[i], item))
                return true;
        }

        return false;
    }

    protected override MySet<T> CreateEmpty() => new MySetList<T>(_items.Count);

    public override IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _items.Count; i++)
            yield return _items[i];
    }
}
