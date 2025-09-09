using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private RectTransform diskParent;

    public int DiskCount => _disks.Count;
    
    private readonly MyStack<Disk> _disks = new MyStack<Disk>();

    private void Awake()
    {
        for (int i = diskParent.childCount - 1; i >= 0; i--)
            _disks.Push(diskParent.GetChild(i).GetComponent<Disk>());
    }

    private void AddDisk(Disk disk)
    { 
        _disks.Push(disk);
        disk.ChangeTower(this);
        MoveDiskVisual(disk);

        TowerGameManager.Instance?.OnDiskPlaced(this);
    }

    public Disk RemoveTop()
    {
        if (_disks.Count ==  0) return null;
        Disk top = _disks.Pop();
        return top;
    }
    
    public bool TryAdd(Disk disk)
    {
        if (disk == null || disk.Tower == null)
        {
            Debug.Log("Either disk or its tower is null");
            return false;
        }
        
        Tower source = disk.Tower;

        if (source._disks.Count == 0 || !ReferenceEquals(source._disks.Peek(), disk))
        {
           Debug.LogWarning($"{name}: {disk.name} is not the top disk of its source tower ({source.name})"); 
           return false;
        }

        if (_disks.Count > 0 && _disks.Peek().Size <= disk.Size)
        {
            Debug.LogWarning($"Cannot place {disk.name} on top of {_disks.Peek().name}");
            return false;
        }

        Disk popped = source.RemoveTop();
        if (!ReferenceEquals(popped, disk))
        {
            if (popped != null)
                source._disks.Push(popped);
            Debug.LogWarning($"{name}: source.RemoveTop popped unexpected disk. Aborting.");
            return false;
        }
        
        AddDisk(disk);
        return true;
    }

    public void RemoveDisk()
    {
        if (_disks.Count > 0) _disks.Pop();
    }

    private void MoveDiskVisual(Disk disk)
    {
        disk.ChangeTower(this);
        disk.transform.SetParent(diskParent);
        disk.transform.SetSiblingIndex(0);
    }

    public string DebugStack()
    {
        var arr = _disks.ToArray();
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"{name} ({_disks.Count}: ");
        for (int i = 0; i < arr.Length; i++)
        {
            sb.Append(arr[i]?.name ?? "null");
            if (i < arr.Length - 1) sb.Append(", ");
        }
        return sb.ToString();
    }
}