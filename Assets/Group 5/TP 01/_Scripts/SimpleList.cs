using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SimpleList<T> : ISimpleList<T>, IEnumerable<T>
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


    public SimpleList() : this(20) { }

    public SimpleList(int capacity)
    {
        _list = new T[capacity];
        _count = 0;
    }

    public SimpleList(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        if (collection is ICollection<T> c)
        {
            _list = new T[c.Count];
            c.CopyTo(_list, 0);
            _count = c.Count;
        }
        else
        {
            _count = 0;
            _list = new T[20];
            foreach (var item in collection)
                Add(item);
        }
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
            sb.Append(_list[i]);
        }
        return sb.ToString();
        // return string.Join(", ", _list.Take(_count));
    }

    public void Sort(IComparer<T> comparer)
    {
        comparer ??= Comparer<T>.Default;

        Array.Sort(_list, 0, _count, comparer); //Sort only the "active" 
    }

    public void Sort(Comparison<T> comparison)
    {
        if (comparison == null)
        {
            Sort((IComparer<T>)null);
            return;
        }
        
        IComparer<T> comparer = Comparer<T>.Create(new Comparison<T>(comparison));
        Array.Sort(_list, 0, _count, comparer);
    }
    
    public void BubbleSort(IComparer<T> comparer = null)
    {
        
        comparer ??= Comparer<T>.Default;

        for (int i = 0; i < _count - 1; i++) //Iterate through the list except the last element.
        {
            bool swapped = false; //Reset swapped value
            for (int j = 0; j < _count - i - 1; j++) //Iterate through the list except the last element and the amount of elements already correctly sorted.
            {
                if (comparer.Compare(_list[j], _list[j + 1]) > 0) //Compare current element with next (this is why we don't iterate through the last one).
                {
                    //Swap if necessary
                    (_list[j], _list[j + 1]) = (_list[j + 1], _list[j]);
                    swapped = true;
                }
            }
            if (!swapped) break; // optimization: stop early if already sorted
        }
    }
    
    public void SelectionSort(IComparer<T> comparer = null)
    {
        comparer ??= Comparer<T>.Default;

        for (int i = 0; i < _count - 1; i++) //Iterate through the list except the last element.
        {
            int minIndex = i; //Set min index to start as default first element.
            for (int j = i + 1; j < _count; j++) //Iterate the list from i onwards.
            {
                if (comparer.Compare(_list[j], _list[minIndex]) < 0) //If current element is less than our stored minIndex:
                    minIndex = j; //We have a new minIndex!
            }

            if (minIndex != i) //If the minIndex is not the index we are iterating on, swap them.
                (_list[i], _list[minIndex]) = (_list[minIndex], _list[i]);
        }
    }

    public T[] ToArray() => _list.ToArray();

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
            yield return _list[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}