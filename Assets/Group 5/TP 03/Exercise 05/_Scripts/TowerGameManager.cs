using System;
using UnityEngine;
using TMPro;

public class TowerGameManager : MonoBehaviour
{
    public static TowerGameManager Instance { get; private set; }
    
    [Header("Win settings")]
    [SerializeField] private Tower winTower;

    [SerializeField] private GameObject winPanel;

    private int _totalDisks;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        _totalDisks = FindObjectsOfType<Disk>().Length;
        winPanel.SetActive(false);
    }

    public void OnDiskPlaced(Tower tower)
    {
        if (tower == null) return;

        if (tower == winTower && tower.DiskCount == _totalDisks) Win();
    }

    private void Win() => winPanel.SetActive(true);
}