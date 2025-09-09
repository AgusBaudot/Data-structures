using UnityEngine;

public class FactorialSequence : MonoBehaviour
{
    private void Start() => UIManager.Instance.OnFactorialPressed += DoFactorial;

    //private int Factorial(int finalNumber, int currentTotal, int currentStep)
    //{
    //    if (finalNumber <= 1) return 1; //The factorial of 0 & 1 is always 1.
    //    if (currentStep > finalNumber) return currentTotal;
    //    currentTotal *= currentStep;
    //    return Factorial(finalNumber, currentTotal, currentStep + 1);
    //}

    private int Factorial(int n)
    {
        if (n <= 1) return n;
        return n * Factorial(n - 1);
    }

    public void DoFactorial(int amount) => UIManager.Instance.GetText().text = Factorial(amount).ToString();
}