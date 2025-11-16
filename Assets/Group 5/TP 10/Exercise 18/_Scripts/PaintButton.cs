using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PaintButton : MonoBehaviour
{
    public event Action<PaintButton> Clicked;
    public bool Pressed => _pressed;
    public TileBase _tile;
    
    [SerializeField] private Button[] _others;
    
    private bool _pressed;
    private Image _img;
    
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(HandleOnClick);
        foreach (Button btn in _others)
            btn.onClick.AddListener(HandleOthers);
    }

    private void HandleOthers()
    {
        if (_pressed)
        {
            _img.color = Color.white;
            _pressed = false;
        }
    }

    private void HandleOnClick()
    {
        _img ??= GetComponent<Image>();
        _img.color = _pressed
            ? Color.white
            : Color.grey;
        _pressed = !_pressed;
        Clicked?.Invoke(this);
    }
}
