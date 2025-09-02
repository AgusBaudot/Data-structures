using UnityEngine;

public class FactorialSequence : MonoBehaviour
{
    private int _total = 1; //If we multiply by 0, then the result will always be 0.

    private int Factorial(int finalNumber, int currentNumber)
    {
        if (finalNumber <= 1) return 1; //The factorial of 0 & 1 is always 1.
        if (currentNumber == finalNumber) return _total;
        _total *= currentNumber;
        return Factorial(finalNumber, currentNumber + 1);
    }

    public void DoFactorial(int amount)
    {
        int factorial = Factorial(amount, 1);
        //Change string text with number obtained from sequence.
    }
}