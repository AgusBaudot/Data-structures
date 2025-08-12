using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MyLinkedList
{
    public class MyNode<T>
    {
        public Pointer Next { get; set; }
        public Pointer Previous { get; set; }

        public T data { get; set; }

        //Ctor.

        public override string ToString()
        {
            return base.ToString();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }

}