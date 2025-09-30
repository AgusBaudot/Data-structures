using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MyBST<T> : MonoBehaviour where T : IComparable<T>
{
    public BSTNode<T> Root;

    private int _height;

    public MyBST(BSTNode<T> root)
    {
        Root = root;
    }

    public void Insert(T data)
    {
        //Create node with data and insert it.
        Insert(new BSTNode<T>(data));
    }
    
    public void Insert(BSTNode<T> node)
    {
        InsertRecursively(Root, node);
    }

    private void InsertRecursively(BSTNode<T> current, BSTNode<T> value)
    {
        if (value.CompareTo(current) == 0) return;

        if (value.CompareTo(current) < 0)
        {
            if (current.Left == null)
            {
                current.SetLeft(value);
                return;
            }
            InsertRecursively(current.Left, value);
        }

        if (value.CompareTo(current) > 0)
        {
            if (current.Right == null)
            {
                current.SetRight(value);
                return;
            }
            InsertRecursively(current.Right, value);
        }
    }

    public int GetBalanceFactor(T data)
    {
        //Search for node with data and perform based on that node.
        //Call other function and pass node.
        return 0;
    }

    public int GetBalanceFactor(BSTNode<T> node = null)
    {
        if (node == null)
            node = Root;

        //return GetHeight(node.Left - node.Right);
        return 0;
    }

    private int GetHeight(BSTNode<T> node, int depth)
    {
        //Add 1 to depth on each call. When leaf is found, start returning value.
        //Return max value.
        return 0;
    }

    private BSTNode<T> GetNodeFromData(T data)
    {
        //Return node when data matches.
        return null;
    }

    public int GetHeight() => _height;
}
