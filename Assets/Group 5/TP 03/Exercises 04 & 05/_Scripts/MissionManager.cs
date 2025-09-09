using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _missionText;
    [SerializeField] private List<MissionSO> _missionsList;
    private MyQueue<MissionSO> _missionsQueue;

    private void Start()
    {
        _missionsQueue = new MyQueue<MissionSO>(_missionsList);
        ShowNextMission();
    }

    public void CompleteMission()
    {
        if (_missionsQueue.Count > 0)
        {
            _missionsQueue.Dequeue();
            ShowNextMission();
        }
        else
        {
            _missionText.text = "Game complete";
        }
    }

    private void ShowNextMission()
    {
        if (_missionsQueue.Count > 0)
            _missionText.text = _missionsQueue.Peek().description;
        else
            _missionText.text = "Game complete";
    }
}
