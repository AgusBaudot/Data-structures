using System;
using UnityEngine;

public class BSTNode<T> :MonoBehaviour, IComparable<T> where T : IComparable<T>
{
    public BSTNode<T> Left { get; private set; }
    public BSTNode<T> Right { get; private set; }
    public T Data { get; private set; }

    public BSTNode(T data)
    {
        Data = data;
    }

    public void SetLeft(BSTNode<T> node)
    {
        Left = node;
    }

    public void SetRight(BSTNode<T> node)
    {
        Right = node;
    }

    public int CompareTo(T other)
    {
        return Data.CompareTo(other);
    }

    public int CompareTo(BSTNode<T> other)
    {
        return Data.CompareTo(other.Data);
    }
}
