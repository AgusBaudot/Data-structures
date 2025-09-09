using UnityEngine;

public class TowerManager : MonoBehaviour
{
    private Disk _currentDisk;
    private bool _hasSelectedDisk;
    private MyStack<Move> _moveHistory = new MyStack<Move>();
    
    public void SelectDisk(GameObject disk)
    {
        //When a disk is selected, store it into a private variable and turn a bool on.
        _currentDisk = disk.GetComponent<Disk>();
        _hasSelectedDisk = true;
    }

    public void SelectedTower(GameObject tower)
    {
        //If player doesn't have a disk selected, exit. Otherwise, move disk to selected tower if applicable.
        if (!_hasSelectedDisk) return;
        Tower from = _currentDisk.Tower;
        if (tower.GetComponent<Tower>().TryAdd(_currentDisk))
        {
            RegisterMove(from, tower.GetComponent<Tower>(), _currentDisk);
            _currentDisk = null;
            _hasSelectedDisk = false;
        }
    }

    private void RegisterMove(Tower from, Tower to, Disk disk)
    {
        _moveHistory.Push(new Move(disk, from, to));
    }

    public void UndoLastMove()
    {
        if (_moveHistory.Count == 0) return;
        
        Move last = _moveHistory.Pop();

        if (last.Disk == null)
        {
            Debug.LogWarning("undo desync: disk or its tower is null");
            return;
        }

        if (last.From == null || last.To == null)
        {
            Debug.LogWarning("Undo desync: one of the towers is null");
            return;
        }

        if (!ReferenceEquals(last.Disk.Tower, last.To))
        {
            Debug.LogWarning("Undo desync: disk in not on the expected tower.");
            return;
        }

        bool success = last.From.TryAdd(last.Disk);
        if (!success)
        {
            Debug.LogWarning("Undo failed: reverse move invalid (disk not on top or size rule");
        }
    }
}
