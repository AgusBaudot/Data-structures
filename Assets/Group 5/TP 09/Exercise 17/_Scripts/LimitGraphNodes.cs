using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LimitGraphNodes : MonoBehaviour
{
    public int maxAttempts = 20;
    private Button _button;
    private int _addedNodes = 0;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleOnClick);
    }

    private void HandleOnClick()
    {
        _addedNodes++;
        _button.interactable = _addedNodes < maxAttempts;
    }
}
