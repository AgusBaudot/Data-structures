using System;
using UnityEngine;

public class Disk : MonoBehaviour, IComparable<Disk>
{
    [SerializeField] private int size;
    [SerializeField] private Tower defaultTower;
    
    public int Size { get; private set; }
    public Tower Tower { get; private set; }

    private void Awake()
    {
        Size = size;
        ChangeTower(defaultTower);
    }

    public void ChangeTower(Tower tower) =>  Tower = tower;
    
    public int CompareTo(Disk other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Size.CompareTo(other.Size);
    }

    public override string ToString() => gameObject.name;
}