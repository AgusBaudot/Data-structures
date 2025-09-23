using UnityEngine;

public class PyramidSequence : MonoBehaviour
{
    private string _pyramidString = string.Empty;
    private void Start() => UIManager.Instance.OnPyramidPressed += DoPyramid;


    private string Pyramid(int height, int currentStep)
    {
        if (height <= 1) return (height == 0) ? string.Empty : "x";
        if (currentStep == height) return _pyramidString;

        _pyramidString += new string(' ', (height * 2 - 1) - (2 * currentStep + 1)); //Should mathematically be all divided by 2, but this way seems better aligned.
        _pyramidString += "x" + new string('x', currentStep * 2);
        _pyramidString += "\n";

        return Pyramid(height, currentStep + 1);
    }

    public void DoPyramid(int height)
    {
        UIManager.Instance.GetText().text = Pyramid(height, 0);
    }
}