using System;

public class AVLTree<T> : MyBST<T> where T : IComparable<T>
{
    //Auto balancing tree.
    //Every time the tree is modified, check for balance.

    private bool ShouldBeBalanced(BSTNode<T> node) //Check if tree should be balanced after every modification of tree.
    {
        node ??= Root;
        int bf = GetBalanceFactor();
        if (bf < -1 || bf > 1)
        {
            return true;
        }
        return false;
    }

    private RotationType DefineRotationType(BSTNode<T> node)
    {
        node ??= Root;
        int bf = GetBalanceFactor(node);
        //LL occurs when node is inserted into the left subtree of the left child of node.
        if (bf > 1) return RotationType.LL;
        //RR occurs when node is inserted into the right subtree of the right child of node.
        //LR occurs when node is inserted into the right subtree of the left child of node.
        //RL occurs when node is inserted into the left subtree of the right child of node.
        return RotationType.LL;
    }

    public override void Insert(T data)
    {
        base.Insert(data);
    }

    public override void Insert(BSTNode<T> node)
    {
        base.Insert(node);
    }

    protected override void InsertRecursively(BSTNode<T> current, BSTNode<T> value)
    {
        base.InsertRecursively(current, value);
    }

    protected override void Delete(BSTNode<T> node)
    {
        base.Delete(node);
    }

    private void RotateRight()
    {
        //Select B and put it as root.
    }

    private void RotateLeft()
    {
        //Select B and put it as root.
    }
}

public enum RotationType
{
    RR,
    LL,
    RL,
    LR
}