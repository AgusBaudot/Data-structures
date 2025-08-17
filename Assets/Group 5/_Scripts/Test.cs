using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private List<int> _arrayList =  new List<int>(2);
    private SimpleList<int> _simpleList = new SimpleList<int>(2);

    private LinkedList<int> _linkedList = new LinkedList<int>();
    private MyList<int>  _myList = new MyList<int>();
    
    
    private void Start()
    {
        //ArrayListTest();
        LinkedListTest();
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
        Debug.Log($"My list: {_myList} ||  LinkedList: {_linkedList}");
        Debug.Assert(Compare(_myList, _linkedList));

        // === Clear ===
        _myList.Clear();
        _linkedList.Clear();
        Debug.Assert(Compare(_myList, _linkedList));
    }

    
    static bool Compare(MyList<int> my, LinkedList<int> sys)
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

    
    static LinkedListNode<int> NodeAt(LinkedList<int> list, int index)
    {
        var node = list.First;
        for (int i = 0; i < index; i++)
            node = node.Next;
        return node;

    }
}
