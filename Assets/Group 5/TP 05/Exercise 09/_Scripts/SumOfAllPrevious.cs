using UnityEngine;

public class SumOfAllPrevious : MonoBehaviour
{
    private void Start() => UIManager.Instance.OnSumPressed += DoSum;

    //private int Sum(int goal, int currentTotal, int currentStep)
    //{
    //    currentTotal += currentStep;
    //    if (currentStep == goal) return currentTotal;
    //    return Sum(goal, currentTotal, currentStep + 1);
    //}

    private int Sum(int n)
    {
        if (n <= 1) return 0;
        return (n - 1) + Sum(n - 1);
    }

    public void DoSum(int goal) => UIManager.Instance.GetText().text = Sum(goal).ToString();
}
