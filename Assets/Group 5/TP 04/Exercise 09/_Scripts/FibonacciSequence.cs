using UnityEngine;

public class FibonacciSequence : MonoBehaviour
{
    private int _starter = 1;
    private int _total = 0;

    private int Fibonacci(int amount, int lastDigit)
    {
        if (amount <= 1)
        {
            return (amount == 0) ? 0 : 1;
        }
        lastDigit = _total; //Last summed digit equals total before the addition.
        _total += lastDigit; //Add last digit to total.
        
        return Fibonacci(amount - 1, lastDigit); //Call recursively this method.
    }

    public void DoFibonacci(int amount)
    {
        int fibonacci = Fibonacci(amount, 0);
        //Modify string text with number obtained from sequence.
    }
}