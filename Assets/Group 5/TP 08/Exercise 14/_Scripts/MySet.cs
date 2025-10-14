public abstract class MySet<T>
{
    public abstract bool Add(T item);
    public abstract bool Remove(T item);
    public abstract void Clear();
    public abstract bool Contains(T item);
    public abstract void Show();
    public abstract int Cardinality();
    public abstract bool IsEmpty();
    public abstract MySet<T> Union(MySet<T> other);
    public abstract MySet<T> Intersect(MySet<T> other);
    public abstract MySet<T> Difference(MySet<T> other);
}