using System;
using System.Collections;
using System.Collections.Generic;

public class MyQueue<T> : IEnumerable<T> where T : IComparable<T>
{
    public int Count => _list.Count;

    private MyList<T> _list;


    public MyQueue()
    {
        _list = new MyList<T>();
    }

    public void Enqueue(T item)
    {
        if (item == null)
            throw new NullReferenceException(nameof(item));

        _list.Add(item);
    }

    public T Dequeue()
    {
        if (_list.Count == 0)
            throw new InvalidOperationException("Cannot dequeue item from empty stack");

        var toRemove = _list[0];
        _list.RemoveAt(0);
        return toRemove;
    }

    public T Peek() => _list[0];

    public void Clear() => _list.Clear();

    public T[] ToArray() => _list.ToArray();

    public override string ToString()
    {
        return _list.ToString();
    }

    public bool TryDequeue(out T item)
    {
        if (_list.Count == 0)
        {
            item = default;
            return false;
        }
        item = Dequeue();
        return true;
    }

    public bool TryPeek(out T item)
    {
        if ( _list.Count == 0)
        {
            item = default;
            return false;
        }
        item = _list[0];
        return true;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}