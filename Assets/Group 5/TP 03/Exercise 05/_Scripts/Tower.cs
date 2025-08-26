using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private RectTransform diskParent;
    
    private readonly MyStack<Disk> _disks = new MyStack<Disk>();

    private void Awake()
    {
        for (int i = diskParent.childCount - 1; i >= 0; i--)
            AddDisk(diskParent.GetChild(i).GetComponent<Disk>());
    }

    private void AddDisk(Disk disk) => _disks.Push(disk);
    
    public bool TryAdd(Disk disk)
    {
        if (disk == null || disk.Tower == null)
        {
            Debug.Log("Either disk or its tower is null");
            return false;
        }
        
        Tower source = disk.Tower;
        
        if (disk.Tower._disks.Count == 0 || !ReferenceEquals(disk.Tower._disks.Peek(), disk)) return false; //Selected disk is not at the top of its tower.

        if (_disks.Count > 0 && _disks.Peek().Size <= disk.Size)
        {
            Debug.LogWarning($"Cannot place {disk.name} on top of {_disks.Peek().name}");
            return false;
        }

        disk.Tower.RemoveDisk();
        AddDisk(disk);
        MoveDiskVisual(disk);
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
}