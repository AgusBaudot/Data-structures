using System.Collections;
using System.Collections.Generic;

public class MyStack<T> : IEnumerable<T>
{
    public int Count => _list.Count;

    private SimpleList<T> _list;

    public MyStack()
    {
        _list = new SimpleList<T>();
    }

    public void Push(T item) => _list.Add(item);

    public T Pop()
    {
        var item = _list[Count - 1];
        _list.Remove(item);
        return item;
    }

    public T Peek() => _list[Count - 1];

    public void Clear() => _list.Clear();

    public T[] ToArray() => _list.ToArray();

    public override string ToString()
    {
        return _list.ToString();
    }

    public bool TryPop(out T item)
    {
        if (_list.Count == 0)
        {
            item = default;
            return false;
        }
        item = _list[Count - 1];
        _list.Remove(item);
        return true;
    }

    public bool TryPeek(out T item)
    {
        if (_list.Count == 0)
        {
            item = default;
            return false;
        }
        item = _list[Count - 1];
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