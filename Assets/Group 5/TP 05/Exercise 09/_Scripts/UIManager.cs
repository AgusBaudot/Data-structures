using System;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultsText;
    [SerializeField] private TMP_InputField _input;
    public static UIManager Instance { get; private set; }

    public event Action<int> OnSumPressed;
    public event Action<int> OnFactorialPressed;
    public event Action<int> OnPyramidPressed;
    public event Action<int> OnFibonacciPressed;
    public event Action<string> OnPalindromePressed;
    
    private void Awake()
    {
        Instance = this;
    }

    public TextMeshProUGUI GetText() => _resultsText;

    public void Sum() => OnSumPressed?.Invoke(int.Parse(_input.text));

    public void Factorial() => OnFactorialPressed?.Invoke(int.Parse(_input.text));

    public void Fibonacci() => OnFibonacciPressed?.Invoke(int.Parse(_input.text));

    public void Pyramid() => OnPyramidPressed?.Invoke(int.Parse(_input.text));

    public void Palindrome() => OnPalindromePressed?.Invoke(_input.text);
}
