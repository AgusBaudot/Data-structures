using System;
using System.Collections.Generic;

namespace MyLinkedList
{
    public class MyNode<T> : IComparer<MyNode<T>>, IEquatable<MyNode<T>>, IDisposable where T : IComparable<T>
    {
        public MyNode<T> Next { get; private set; }
        public MyNode<T> Previous { get; private set; }
        public T Data { get; private set; }
        
        public MyNode(T data, MyNode<T> next = null, MyNode<T> previous = null)
        {
            Data = data;
            Next = next;
            Previous = previous;
        }
        
        public void SetNext(MyNode<T> next) => Next = next;
        public void SetPrevious(MyNode<T> previous) => Previous = previous;
        

        public override string ToString() => $"Data: {Data}";

        public int Compare(MyNode<T> x, MyNode<T> y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.Data.CompareTo(y.Data);
        }

        public bool Equals(MyNode<T> other)
        {
            if (other == null) return false;
            return Compare(this, other) == 0;
        }

        public override bool Equals(object obj) => Equals(obj as MyNode<T>);

        public override int GetHashCode() => Data?.GetHashCode() ?? 0;

        public void Dispose()
        {
            Data = default;
            Next = null;
            Previous = null;
        }
    }

}