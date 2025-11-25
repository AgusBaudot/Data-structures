using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MeinMenuButtons : MonoBehaviour
{
    public int level;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(HandleOnClick);
    }

    private void HandleOnClick()
    {
        SceneManager.LoadScene(level);
    }
}