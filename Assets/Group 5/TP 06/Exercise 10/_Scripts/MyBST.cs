using System;
using System.IO;
using UnityEngine;

public class MyBST<T> where T : IComparable<T>
{
    public BSTNode<T> Root { get; private set; }
    
    public int Count { get; private set; }

    // private int _height;
    protected MyQueue<BSTNode<T>> _treeQueue = new();
    
    public MyBST() {}

    public MyBST(BSTNode<T> root)
    {
        Root = root;
    }

    public virtual void Insert(T data)
    {
        Insert(new BSTNode<T>(data));
    }
    
    public virtual void Insert(BSTNode<T> node)
    {
        if (Root == null)
        {
            Root = node;
            Count++;
            return;
        }
        InsertRecursively(Root, node);
    }

    protected virtual void InsertRecursively(BSTNode<T> current, BSTNode<T> value)
    {
        if (current == null) throw new ArgumentNullException(nameof(current));
        
        if (value.CompareTo(current) == 0) return;

        if (value.CompareTo(current) < 0)
        {
            if (current.Left == null)
            {
                current.SetLeft(value);
                Count++;
            }
            
            else
                InsertRecursively(current.Left, value);
        }

        else
        {
            if (current.Right == null)
            {
                current.SetRight(value);
                Count++;
            }
            
            else
                InsertRecursively(current.Right, value);
        }
    }

    public void Delete(T data) => Delete(GetNodeFromData(data));

    protected virtual void Delete(BSTNode<T> node)
    {
        if (node == null) return;

        var parent = GetFatherOfNode(node);
        
        //If leaf
        if (node.Left == null && node.Right == null)
        {
            ReplaceParentChild(parent, node, null);
            Count--;
            return;
        }

        //If 2 children
        if (node.Left != null && node.Right != null)
        {
            var pred = FindBiggest(node.Left);
            node.SetData(pred.Data);
            Delete(pred);
            return;
        }
        
        //If 1 child
        var child = node.Left ?? node.Right;
        ReplaceParentChild(parent, node, child);
        Count--;
    }

    protected BSTNode<T> FindBiggest(BSTNode<T> node)
    {
        BSTNode<T> current = node;
        while (current.Right != null)
        {
            current =  current.Right;
        }

        return current;
    }

    protected void ReplaceParentChild(BSTNode<T> parent, BSTNode<T> oldChild, BSTNode<T> newChild)
    {
        if (parent == null)
        {
            Root = newChild;
            return;
        }

        if (parent.Left == oldChild) parent.SetLeft(newChild);
        else if (parent.Right == oldChild) parent.SetRight(newChild);
    }

    public int GetBalanceFactor(T data) => GetBalanceFactor(GetNodeFromData(data));

    public int GetBalanceFactor(BSTNode<T> node = null)
    {
        node ??= Root;

        return GetHeight(node.Left) - GetHeight(node.Right);
    }

    protected int GetHeight(BSTNode<T> node)
    {
        if (node == null) return 0; //Return -1 if edge based instead of node based.
        int left = GetHeight(node.Left);
        int right = GetHeight(node.Right);

        return 1 + Math.Max(left, right);
    }

    protected BSTNode<T> GetNodeFromData(T data)
    {
        var n = GetNodeRecursively(Root, data);
        if (n == null) throw new InvalidDataException($"Data {data} is not present in tree.");
        return n;
    }

    protected BSTNode<T> GetNodeRecursively(BSTNode<T> node, T data)
    {
        if (node == null) return null;

        if (data.CompareTo(node.Data) == 0) return node;
        return data.CompareTo(node.Data) < 0
            ? GetNodeRecursively(node.Left, data)
            : GetNodeRecursively(node.Right, data);
    }

    protected BSTNode<T> GetFatherOfNode(BSTNode<T> node)
    {
        if (node == Root || node == null) return null;

        BSTNode<T> current = Root;
        BSTNode<T> parent = null;

        while (current != null)
        {
            if (ReferenceEquals(current.Left, node) || ReferenceEquals(current.Right, node))
                return current;

            parent = current;
            current = node.CompareTo(current) < 0
                ? current.Left
                : current.Right;
        }
        
        throw new InvalidDataException($"Node {node.Data} is not present in tree.");
    }

    public int GetHeight() => GetHeight(Root);

    public override string ToString()
    {
        if (Root == null) return string.Empty;

        _treeQueue.Clear();
        _treeQueue.Enqueue(Root);

        while (_treeQueue.Count > 0)
        {
            var node =  _treeQueue.Dequeue();
            Debug.Log(node.Data);
            if (node.Left != null)
                _treeQueue.Enqueue(node.Left);
            if (node.Right != null)
                _treeQueue.Enqueue(node.Right);
        }

        return "";
    }

    public bool Contains(T data) => GetNodeFromData(data) != null;

    public void Clear()
    {
        Clear(Root);
        Root = null;
        _treeQueue.Clear();
        Count = 0;
    }

    protected void Clear(BSTNode<T> node)
    {
        if (node == null) return;

        Clear(node.Left);
        Clear(node.Right);
        
        node.SetLeft(null);
        node.SetRight(null);
        node.SetData(default);
        //Maybe iterative of recursive to avoid stack overflow in deep trees.
    }

    public void PreOrderTraversal(Action<T> action)
    {
        PreOrderRecursive(Root, action);
    }

    protected void PreOrderRecursive(BSTNode<T> current, Action<T> action)
    {
        if (current == null) return;

        action(current.Data);
        PreOrderRecursive(current.Left, action);
        PreOrderRecursive(current.Right, action);
    }

    public void InOrderTraversal(Action<T> action)
    {
        InOrderRecursive(Root, action);
    }

    protected void InOrderRecursive(BSTNode<T> current, Action<T> action)
    {
        if (current == null) return;

        InOrderRecursive(current.Left, action);
        action(current.Data);
        InOrderRecursive(current.Right, action);
    }

    public void PostOrderTraversal(Action<T> action)
    {
        PostOrderRecursive(Root, action);
    }

    protected void PostOrderRecursive(BSTNode<T> current, Action<T> action)
    {
        if (current == null) return;

        PostOrderRecursive(current.Left, action);
        PostOrderRecursive(current.Right, action);
        action(current.Data);
    }

    public bool IsEmpty() => Count == 0;
}
