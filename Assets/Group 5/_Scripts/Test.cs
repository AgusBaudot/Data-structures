using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour
{
    private List<int> _arrayList =  new List<int>(2);
    private SimpleList<int> _simpleList = new SimpleList<int>(2);

    private LinkedList<int> _linkedList = new LinkedList<int>();
    private MyList<int>  _myList = new MyList<int>();

    private MyQueue<int> _myQueue = new MyQueue<int>();
    private Queue<int> _queue = new Queue<int>();

    private MyStack<int> _myStack = new MyStack<int>();
    private Stack<int> _stack = new Stack<int>();

    private MyBST<int> _bst = new MyBST<int>();
    
    
    private void Start()
    {
        //ArrayListTest();
        //LinkedListTest();
        //QueueTest();
        // StackTest();
        BSTTest();
    }

    private void ArrayListTest()
    {
        //Initial declaration
        Debug.Assert(_simpleList.Count == _arrayList.Count);

        //Basic Add usage
        _simpleList.Add(10);
        _arrayList.Add(10);
        Debug.Assert(_simpleList.Count == _arrayList.Count);
        Debug.Assert(_simpleList[0] == _arrayList[0]);
        
        _simpleList.Add(20);
        _arrayList.Add(20);
        Debug.Assert(_simpleList.Count == _arrayList.Count);
        Debug.Assert(_simpleList[0] == _arrayList[0]);
        Debug.Assert(_simpleList[1] == _arrayList[1]);
        
        _simpleList.AddRange(new int[] { 30, 40, 50 });
        _arrayList.AddRange(new int[] { 30, 40, 50 });
        Debug.Assert(_simpleList.Count == _arrayList.Count);
        Debug.Assert(_simpleList[4] == _arrayList[4]);

        _simpleList[1] = 90;
        Debug.Assert(_simpleList[1] == 90);

        bool removed = _simpleList.Remove(90);
        Debug.Assert(removed && _simpleList.Count == 4 && _simpleList[1] == 30);

        bool failed = _simpleList.Remove(12390);
        Debug.Assert(!failed && _simpleList.Count == 4);
        
        _simpleList.Clear();
        _arrayList.Clear();
        Debug.Assert(_simpleList.Count == 0 && _simpleList.Count == _arrayList.Count);
    }
    
    private void LinkedListTest()
    {
        _myList.Add(10);
        _linkedList.AddLast(10);
        Debug.Assert(Compare(_myList, _linkedList));

        _myList.Add(20);
        _linkedList.AddLast(20);
        Debug.Assert(Compare(_myList, _linkedList));

        
        _myList.Insert(0, 99);
        _linkedList.AddFirst(99);
        Debug.Assert(Compare(_myList, _linkedList));

        _myList.Insert(2, 77); // insert in middle
        var node = NodeAt(_linkedList, 2);
        _linkedList.AddBefore(node, 77);
        Debug.Assert(Compare(_myList, _linkedList));

        
        _myList.RemoveAt(0);
        _linkedList.RemoveFirst();
        Debug.Assert(Compare(_myList, _linkedList));

        _myList.RemoveAt(_myList.Count - 1);
        _linkedList.RemoveLast();
        Debug.Assert(Compare(_myList, _linkedList));

        
        bool removed1 = _myList.Remove(77);
        bool removed2 = _linkedList.Remove(77);
        Debug.Assert(removed1 == removed2 && Compare(_myList, _linkedList));

        
        _myList.AddRange(new int[] { 5, 3, 8, 1, 2 });
        foreach (var v in new int[] { 5, 3, 8, 1, 2 })
            _linkedList.AddLast(v);
        Debug.Assert(Compare(_myList, _linkedList));
        
        
        _myList.Sort();
        var sorted = new List<int>(_linkedList);
        sorted.Sort();
        _linkedList = new LinkedList<int>(sorted);
        Debug.Assert(Compare(_myList, _linkedList));

        // === Clear ===
        _myList.Clear();
        _linkedList.Clear();
        Debug.Assert(Compare(_myList, _linkedList));
    }

    private void QueueTest()
    {
        _myQueue.Enqueue(10);
        _queue.Enqueue(10);
        Debug.Assert(CompareQueues(_myQueue, _queue));

        _myQueue.Enqueue(20);
        _queue.Enqueue(20);
        Debug.Assert(CompareQueues(_myQueue, _queue));

        int myPeek = _myQueue.Peek();
        int sysPeek = _queue.Peek();
        Debug.Assert(myPeek == sysPeek);

        int myDeq = _myQueue.Dequeue();
        int sysDeq = _queue.Dequeue();
        Debug.Assert(myDeq == sysDeq && CompareQueues(_myQueue, _queue));

        _myQueue.Enqueue(30);
        _myQueue.Enqueue(40);
        _queue.Enqueue(30);
        _queue.Enqueue(40);
        Debug.Assert(CompareQueues(_myQueue, _queue));

        _myQueue.Clear();
        _queue.Clear();
        Debug.Assert(CompareQueues(_myQueue, _queue));
    }

    private void StackTest()
    {
        _myStack.Push(5);
        _stack.Push(5);
        Debug.Assert(CompareStacks(_myStack, _stack));

        _myStack.Push(10);
        _stack.Push(10);
        Debug.Assert(CompareStacks(_myStack, _stack));

        int myPeek = _myStack.Peek();
        int sysPeek = _stack.Peek();
        Debug.Assert(myPeek == sysPeek);

        int myPop = _myStack.Pop();
        int sysPop = _stack.Pop();
        Debug.Assert(myPop == sysPop && CompareStacks(_myStack, _stack));

        _myStack.Push(15);
        _myStack.Push(20);
        _stack.Push(15);
        _stack.Push(20);
        Debug.Assert(CompareStacks(_myStack, _stack));

        _myStack.Clear();
        _stack.Clear();
        Debug.Assert(CompareStacks(_myStack, _stack));
    }

    private void BSTTest()
    {
        #region Insertion testing
        
        _bst.Insert(10);
        _bst.Insert(5);
        _bst.Insert(15);

        Debug.Assert(_bst.Root != null);
        Debug.Assert(_bst.Root.Data == 10);
        Debug.Assert(_bst.Root.Left.Data == 5);
        Debug.Assert(_bst.Root.Right.Data == 15);

        var node = new BSTNode<int>(2);
        _bst.Insert(node);
        Debug.Assert(_bst.Root.Left.Left != null && _bst.Root.Left.Left.Data == 2);

        #endregion

        #region BalanceFactor calculation testing
        
        _bst.Insert(7);

        int rootBalance = _bst.GetBalanceFactor(_bst.Root);
        Debug.Assert(rootBalance == 1);
        int leftBalance = _bst.GetBalanceFactor(5);
        Debug.Assert(leftBalance == 0);
        int rightBalance = _bst.GetBalanceFactor(_bst.Root.Right);
        Debug.Assert(rightBalance == 0);
        
        #endregion

        #region Height testing

        _bst.ToString();
        Debug.Assert(_bst.GetHeight() == 3);

        #endregion
    }

    
    private bool Compare(MyList<int> my, LinkedList<int> sys)
    {
        if (my.Count != sys.Count)
            return false;

        int i = 0;
        foreach (var v in sys)
        {
            if (!my[i].Equals(v))
                return false;
            i++;
        }
        return true;
    }

    
    private LinkedListNode<int> NodeAt(LinkedList<int> list, int index)
    {
        var node = list.First;
        for (int i = 0; i < index; i++)
            node = node.Next;
        return node;

    }

    private bool CompareQueues(MyQueue<int> myQueue, Queue<int> systemQueue)
    {
        return myQueue.SequenceEqual(systemQueue);
    }

    private bool CompareStacks(MyStack<int> myStack, Stack<int> systemStack)
    {
        return myStack.SequenceEqual(systemStack.Reverse());
    }
}
