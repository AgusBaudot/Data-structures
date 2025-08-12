using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private List<int> test =  new List<int>(2);
    private SimpleList<int> list = new SimpleList<int>(2);
    
    private void Start()
    {
        //Initial declaration
        Debug.Assert(list.Count == test.Count);

        //Basic Add usage
        list.Add(10);
        test.Add(10);
        Debug.Assert(list.Count == test.Count);
        Debug.Assert(list[0] == test[0]);
        
        list.Add(20);
        test.Add(20);
        Debug.Assert(list.Count == test.Count);
        Debug.Assert(list[0] == test[0]);
        Debug.Assert(list[1] == test[1]);
        
        list.AddRange(new int[] { 30, 40, 50 });
        test.AddRange(new int[] { 30, 40, 50 });
        Debug.Assert(list.Count == test.Count);
        Debug.Assert(list[4] == test[4]);

        list[1] = 90;
        Debug.Assert(list[1] == 90);

        bool removed = list.Remove(90);
        Debug.Assert(removed && list.Count == 4 && list[1] == 30);

        bool failed = list.Remove(12390);
        Debug.Assert(!failed && list.Count == 4);
        
        list.Clear();
        test.Clear();
        Debug.Assert(list.Count == 0 && list.Count == test.Count);
    }
}
