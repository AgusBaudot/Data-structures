using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class MyList<T>
{
    public Pointer _head;
    public Pointer _tail;

    public T this[int index]
    {
        get
        {
            return this[index];
        }
        set
        {
            this [index] = value;
        }
    }

    public int Count { get => _count; }

    private int _count;

    public MyList()
    {
        _head = null;
        _tail = null;
        _count = 0;
    }

    public void Add(T value)
    {
        //Make tails.Next point to this new node.
        //Make thisNode.Previous point to tail.
        //Make tail = this node.
    }

    public void AddRange(T[] values)
    {

    }

    public void AddRange(MyList<T> values)
    {

    }
    
    public bool Remove()
    {
        return true;
    }

    public void RemoveAt()
    {

    }

    public void Insert()
    {

    }

    public bool IsEmpty()
    {
        return Count == 0;
    }

    public void Clear()
    {

    }

    public override string ToString()
    {
        return base.ToString();
    }
}
