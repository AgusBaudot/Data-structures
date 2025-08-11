using System;
using System.Collections.Generic;
using System.Linq;

public class SimpleList<T> : ISimpleList<T>
{
    private T[] _list;
    private int _count;

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();
            return _list[index];
        }
        set
        {
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();
            _list[index] = value;
        }
    }

    public int Count => _count;


    public SimpleList() : this(20) {}

    public SimpleList(int capacity)
    {
        _list = new T[capacity];
        _count = 0;
    }

    public void Add(T item)
    {
        EnsureCapacity(_count + 1);
        _list[_count] = item;
        _count++;
    }

    public void AddRange(T[] collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        EnsureCapacity(_count + collection.Length);
        for (int i = 0; i < collection.Length; i++)
        {
            _list[_count + i] = collection[i];
        }
        _count += collection.Length;
    }

    public bool Remove(T item)
    {
        for (int i = 0; i < _count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_list[i], item))
            {
                for (int j = i; j < _count - 1; j++)
                {
                    _list[j] = _list[j + 1];
                }
                _list[_count - 1] = default;
                _count--;
                return true;
            }
        }
        return false;
    }

    public void Clear()
    {
        for (int i = 0; i < _count; i++)
        {
            _list[i] = default;
        }
        _count = 0; 
    }

    private void EnsureCapacity(int min)
    {
        if (_list.Length < min)
        {
            int newCapacity = Math.Max(_list.Length * 2, min);
            T[] newList = new T[newCapacity];
            for (int i = 0; i < _count; i++)
            {
                newList[i] = _list[i];
            }
            _list = newList;
        }
    }

    public override string ToString()
    {
        if (_count == 0)
            return string.Empty;
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < _count; i++)
        {
            if (i > 0)
                sb.Append(", ");
            sb.Append(_list[i]?.ToString());
        }
        return sb.ToString();
        // return string.Join(", ", _list.Take(_count));
    }
}