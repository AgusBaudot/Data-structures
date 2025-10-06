using System;

public class BSTNode<T> : IComparable<T> where T : IComparable<T>
{
    public BSTNode<T> Left { get; private set; }
    public BSTNode<T> Right { get; private set; }
    public T Data { get; private set; }

    public BSTNode(T data)
    {
        Data = data;
    }

    public void SetLeft(BSTNode<T> n) => Left = n;

    public void SetRight(BSTNode<T> n) => Right = n;

    public void SetData(T d) => Data = d;

    public int CompareTo(T other)
    {
        return Data.CompareTo(other);
    }

    public int CompareTo(BSTNode<T> other)
    {
        return Data.CompareTo(other.Data);
    }

    public bool Equals(BSTNode<T> obj)
    {
        return Data.Equals(obj.Data);
    }
}
