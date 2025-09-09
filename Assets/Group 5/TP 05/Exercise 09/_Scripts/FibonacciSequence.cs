using UnityEngine;

public class FibonacciSequence : MonoBehaviour
{
    private void Start() => UIManager.Instance.OnFibonacciPressed += DoFibonacci;

    //private int Fibonacci(int amount, int currentTotal, int lastDigit, int currentStep)
    //{
    //    /*
    //    0 + 1 = 1
    //    1 + 1 = 2
    //    1 + 2 = 3
    //    2 + 3 = 5
    //    3 + 5 = 8
    //    5 + 8 = 13
    //    */
    //    if (amount <= 1)
    //        return (amount == 0) ? 0 : 1;
    //    if (currentStep == amount) return currentTotal;
        
    //    return Fibonacci(amount, lastDigit + currentTotal, currentTotal, currentStep + 1);
    //}

    private int Fibonacci (int n)
    {
        if (n <= 1) return n;
        return Fibonacci(n - 1) + Fibonacci(n - 2);
    }
    
    public void DoFibonacci(int amount)
    {
        UIManager.Instance.GetText().text = Fibonacci(amount).ToString();
    }
}