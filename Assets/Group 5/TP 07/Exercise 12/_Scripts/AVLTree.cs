using System;

public class AVLTree<T> : MyBST<T> where T : IComparable<T>
{
    //Auto balancing tree.
    //Every time the tree is modified, check for balance.

    public AVLTree() : base() {}

    public AVLTree(BSTNode<T> root) : base(root) {}

    public override void Insert(T data)
    {
        base.Insert(data);
        Root = RebalanceSubtree(Root);
    }

    public override void Insert(BSTNode<T> node)
    {
        base.Insert(node);
        Root = RebalanceSubtree(Root);
    }

    protected override void Delete(BSTNode<T> node)
    {
        if (node == null) return;
        base.Delete(node);
        Root = RebalanceSubtree(Root);
    }
    
    //Rebalancing logic (post-order): returns new root of this subtree.
    //Could be further optimized to O(log n). (Now O(n.h) where h is height of tree. Practically O(n^2) in pathological cases).
    //Add per-node height for max-optimization.
    private BSTNode<T> RebalanceSubtree(BSTNode<T> node)
    {
        if (node == null) return null;

        BSTNode<T> left = RebalanceSubtree(node.Left);
        BSTNode<T> right = RebalanceSubtree(node.Right);

        //Ensure children references are updated in case rotations below replaced them.
        node.SetLeft(left);
        node.SetRight(right);

        int bf = GetBalanceFactor(node);

        if (bf > 1)
        {
            if (GetBalanceFactor(node.Left) < 0)
                node.SetLeft(RotateLeftNode(node.Left));

            return RotateRightNode(node);
        }

        if (bf < -1)
        {
            if (GetBalanceFactor(node.Right) > 0)
                node.SetRight(RotateRightNode(node.Right));

            return RotateLeftNode(node);
        }

        return node;
    }

    private BSTNode<T> RotateRightNode(BSTNode<T> n)
    {
        if (n == null) return null;
        BSTNode<T> l = n.Left;
        if (l == null) return n;

        BSTNode<T> r = l.Right;

        l.SetRight(n);
        n.SetLeft(r);

        return l;
    }

    private BSTNode<T> RotateLeftNode(BSTNode<T> n)
    {
        if (n == null) return null;
        BSTNode<T> r = n.Right;
        if (r == null) return n;
        
        BSTNode<T> l = r.Left;

        r.SetLeft(n);
        n.SetRight(l);
        
        return r;
    }
}