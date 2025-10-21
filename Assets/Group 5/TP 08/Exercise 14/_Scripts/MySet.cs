using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MySet<T> : IEnumerable<T>
{
    protected readonly IEqualityComparer<T> Comparer = EqualityComparer<T>.Default;
    
    public abstract bool Add(T item);
    public abstract bool Remove(T item);
    public abstract void Clear();
    public abstract bool Contains(T item);
    public abstract int Cardinality { get; }
    public int Count => Cardinality;
    public bool IsEmpty => Cardinality == 0;

    public override string ToString()
    {
        //Returns string like "{a, b, c}"
        var sb = new System.Text.StringBuilder();
        sb.Append("{");
        bool first = true;
        foreach (var x in this)
        {
            if (!first) sb.Append(", ");
            sb.Append(x);
            first = false;
        }
        sb.Append("}");
        return sb.ToString();
    }

    public virtual string Show()
    {
        string s = ToString();
        Debug.Log(s);
        return s;
    }
    
    // "Factory" method used by set operations so derived classes return saem concrete type
    protected abstract MySet<T> CreateEmpty();

    public virtual MySet<T> Union(MySet<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        MySet<T> res = CreateEmpty();
        foreach (var x in this) res.Add(x);
        foreach (var x in other) res.Add(x);
        
        return res;
    }

    public virtual MySet<T> Intersect(MySet<T> other)
    {
        if (other == null) throw new ArgumentNullException(nameof(other));
        MySet<T> res = CreateEmpty();
        //Iterate smaller set first for slight optimization.
        if (Cardinality <= other.Cardinality)
        {
            foreach (var x in this)
                if (other.Contains(x)) res.Add(x);
        }
        else
        {
            foreach (var x in other)
                if (Contains(x)) res.Add(x);
        }
        
        return res;
    }

    public virtual MySet<T> Difference(MySet<T> other)
    {
        if  (other == null) throw new ArgumentNullException(nameof(other));
        MySet<T> res = CreateEmpty();
        foreach (var x in this)
            if (!other.Contains(x)) res.Add(x);
        
        return res;
    }

    public abstract IEnumerator<T> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}