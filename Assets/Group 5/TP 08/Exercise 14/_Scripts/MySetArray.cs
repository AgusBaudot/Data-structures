using System;
using System.Collections.Generic;

public class MySetArray<T> : MySet<T>
{
    private T[] _items;
    private int _count;

    public MySetArray(int initialCapacity = 8)
    {
        if (initialCapacity < 1) initialCapacity = 8;
        _items = new T[initialCapacity];
        _count = 0;
    }

    public override int Cardinality => _count;

    private void EnsureCapacity(int min)
    {
        if (_items.Length >= min) return;
        int newCap = Math.Max(_items.Length * 2, min);
        Array.Resize(ref _items, newCap);
    }

    private int IndexOf(T item)
    {
        for (int i = 0; i < _count; i++)
            if(Comparer.Equals(_items[i], item)) 
                return i;

        return -1;
    }

    public override bool Add(T item)
    {
        if (IndexOf(item) >= 0) return false;
        EnsureCapacity(_count + 1);
        _items[_count++] = item;
        return true;
    }

    public override bool Remove(T item)
    {
        int idx = IndexOf(item);
        if (idx < 0) return false;
        //Shift left
        int move = _count - idx - 1;
        if (move > 0) Array.Copy(_items, idx + 1, _items, idx, move);
        
        //Pre-order decrement to avoid OutOfRange.
        _items[--_count] = default;
        return true;
    }

    public override void Clear()
    {
        for (int i = 0; i < _count; i++) _items[i] = default;
        _count = 0;
    }

    public override bool Contains(T item)
    {
        for (int i = 0; i < _count; i++)
        {
            if (Comparer.Equals(_items[i], item)) 
                return true;
        }

        return false;
    }

    protected override MySet<T> CreateEmpty() => new MySetArray<T>(_items.Length);

    public override IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++) yield return _items[i];
    }
}
