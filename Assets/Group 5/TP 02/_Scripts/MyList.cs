using System;
using System.Collections;
using System.Collections.Generic;
using MyLinkedList;
using UnityEngine;

public class MyList<T> : IDisposable, IEnumerable<T> where T : IComparable<T>
{
    private MyNode<T> _head;
    private MyNode<T> _tail;
    private int _count;
    
    public MyNode<T> First => _head;
    public MyNode<T> Last => _tail;
    public int Count => _count;
    
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index));

            MyNode<T> current = _head;
            for (int i = 0; i < index; i++)
                current = current.Next;

            return current.Data;
        }
        
        // Do we really want a setter?
        //
        //  set
        //  {
        //      if (index < 0 || index >= _count)
        //          throw new ArgumentOutOfRangeException(nameof(index));
        //
        //      var current = _head;
        //      for (int i = 0; i < index; i++)
        //          current = current.Next;
        //      
        //      current.Data = value;
        //  }
    }

    public MyList(MyNode<T> root = null)
    {
        _head = _tail = root;
        _count = root != null ? 1 : 0;
    }

    public void Add(T value)
    {
        var newNode = new MyNode<T>(value, null, _tail);

        if (_tail != null)
            _tail.SetNext(newNode);

        else 
            _head = newNode;

        _tail = newNode;
        _count++;
    }

    public void AddRange(T[] values)
    {
        foreach (var n in values)
        {
            Add(n);
        }
    }

    public void AddRange(MyList<T> values)
    {
        if (values.Count == 0)
            return;

        if (IsEmpty())
        {
            _head = values._head;
            _tail = values._tail;
            _count = values.Count;
        }
        else
        {
            _tail.SetNext(values._head);
            values._head.SetPrevious(_tail);
            _tail = values._tail;
            _count += values.Count;
        }
        
        values.SemiDispose();
    }
    
    public bool Remove(T value)
    {
        if (IsEmpty()) return false;

        MyNode<T> current = _head;

        while (current != null)
        {
            if (current.Data.Equals(value))
            {
                if (current.Previous != null)
                    current.Previous.SetNext(current.Next);
                else _head = current.Next;
                
                if (current.Next != null)
                    current.Next.SetPrevious(current.Previous);
                else
                    _tail = current.Previous;

                _count--;
                current.Dispose();
                return true;
            }

            current = current.Next;
        }

        return false;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        var current = _head;
        for (int i = 0; i < index; i++)
            current = current.Next;

        if (current.Previous != null)
            current.Previous.SetNext(current.Next);
        else
            _head = current.Next;

        if (current.Next != null)
            current.Next.SetPrevious(current.Previous);
        else
            _tail = current.Previous;

        _count--;
    }

    public void Insert(int index, T value)
    {
        if (index < 0 || index > _count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index == _count)
        {
            Add(value);
            return;
        }
        if (index == 0)
        {
            MyNode<T> newHead = new MyNode<T>(value, _head);
            _head.SetPrevious(newHead);
            _head = newHead;
            _count++;
            return;
        }
        
		MyNode<T> next = GetNodeFromIndex(index);
        MyNode<T> current = new MyNode<T>(value, next, next.Previous);
        next.SetPrevious(current);
        current.Previous.SetNext(current);
        _count++;
    }

    public bool IsEmpty() => _count == 0;

    public void Clear() => Dispose();

    private MyNode<T> GetNodeFromIndex(int index)
    {
        //Use Math.Min between index and count - 1 - index?
        if (index < Count / 2)
        {
            var current = First;
            for (int i = 0; i < index; i++)
                current = current.Next; 
            return current;
        }
        else
        {
            var current = Last;
            for (int i = Count - 1; i > index; i--)
                current = current.Previous;
            return current;
        }
    }
    
    // private int GetIndexFromData(T value)
    // {
    //     if (IsEmpty())
    //         throw new InvalidOperationException("Cannot remove a value from an empty list");
    //     
    //     MyNode<T> current = _head;
    //     int counter = 0;
    //
    //     while (current != null)
    //     {
    //         if (current.Data.Equals(value))
    //             return counter;
    //         current = current.Next;
    //         counter++;
    //     }
    //
    //     return -1;
    // }
    
    public void SemiDispose()
    {
        _head = null;
        _tail = null;
        _count = 0;
    }

    public void Dispose()
    {
        MyNode<T> current = _head;

        while (current != null)
        {
            MyNode<T> next = current.Next;
            
            //Break links:
            current.SetNext(null);
            current.SetPrevious(null);
            
            //If node's data holds unmanaged resources, dispose them
            if (current.Data is IDisposable disposable)
                disposable.Dispose();

            current = next;
        }
        
        _head = null;
        _tail = null;
        _count = 0;
    }

    public override string ToString()
    {
        return string.Join(" -> ", this);
    }

    public IEnumerator<T> GetEnumerator()
    {
        var current = _head;
        while (current != null)
        {
            yield return current.Data;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator(); //Delegate to generic version.
    }

    public void Sort(IComparer<T> comparer = null)
    {
        if (_count <= 1) return;
        
        //Ensures fallback to default comparison if no custom comparer is provided.
        comparer ??= Comparer<T>.Default;

        _head = MergeSort(_head, comparer);

        _tail = _head;
        while (_tail.Next != null)
            _tail = _tail.Next;
    }

    //Used merge sort because it's the best on linked lists with an O(n log n) worst-case scenario.
    //Other options:
    //Quick sort: not used because needs random access, better suited for arrays.
    //Insertion sort: not used because is worse in the average-case than MergeSort.
    //Bubble sort: not used due to impracticality in longer lists. O(n^2) performance.
    private MyNode<T> MergeSort(MyNode<T> head, IComparer<T> comparer)
    {
        if (head == null || head.Next == null)
            return head;
        
        //Split list into halves
        MyNode<T> middle = GetMiddle(head);
        MyNode<T> nextOfMiddle = middle.Next;
        middle.SetNext(null);
        if (nextOfMiddle != null) nextOfMiddle.SetPrevious(null);
        
        MyNode<T> left = MergeSort(head, comparer);
        MyNode<T> right = MergeSort(nextOfMiddle, comparer);
        
        return SortedMerge(left, right, comparer);
    }

    private MyNode<T> SortedMerge(MyNode<T> left, MyNode<T> right, IComparer<T> comparer)
    {
        //If either half is empty, return the other one.
        if (left == null) return right;
        if (right == null) return left;

        MyNode<T> result;

        //Compare the first nodes of each half. Set the smaller one as the result of the merge.
        result = comparer.Compare(left.Data, right.Data) <= 0 ? left : right;

        //Recursively merge the rest, repeating the process.
        //Merge remaining nodes of the two lists.
        result.SetNext(SortedMerge(result.Equals(left) ? left.Next : left, result.Equals(left) ? right : right.Next, comparer));

        //Update references.
        if (result.Next != null) 
            result.Next.SetPrevious(result);
        // result.SetPrevious(null);

        //Return merged list.
        return result;
    }

    private MyNode<T> GetMiddle(MyNode<T> head)
    {
        if (head == null) return null;
        
        MyNode<T> slow = head;
        MyNode<T> fast = head;
        
        while (fast.Next != null && fast.Next.Next != null)
        {
            slow = slow.Next;
            fast = fast.Next.Next;
        }

        return slow;
    }
    
    // public void DebugPrintLinks()
    // {
    //     Debug.Log("Debugging linked list structure:");
    //     var current = _head;
    //     int index = 0;
    //
    //     while (current != null)
    //     {
    //         string prevData = current.Previous != null ? current.Previous.Data.ToString() : "null";
    //         string nextData = current.Next != null ? current.Next.Data.ToString() : "null";
    //         Debug.Log($"Index {index}: Data={current.Data}, Prev={prevData}, Next={nextData}");
    //         current = current.Next;
    //         index++;
    //     }
    //
    //     Debug.Log($"Head: {_head.Data}, Tail: {_tail.Data}, Count: {_count}");
    // }

}
