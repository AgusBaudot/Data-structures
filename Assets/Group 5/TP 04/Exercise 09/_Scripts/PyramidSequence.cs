using System.Linq;
using UnityEngine;

public class PyramidSequence : MonoBehaviour
{
    private string _pyramidString = string.Empty;

    private string Pyramid(int height, int currentStep)
    {
        if (height <= 1) return (height == 0) ? string.Empty : "x";
        //_pyramidString.Append($"\n");

        //The amount of x's is always 1 + (currentStep - 1) * 2 or just start currentStep from 0.
        //The amount of spaces is always (height * 2) - 1 - INCOMPLETE
        //"""
        //    x    
        //   xxx   
        //  xxxxx  
        // xxxxxxx 
        //xxxxxxxxx
        //"""

        return Pyramid(height, currentStep + 1);
    }

    public void DoPyramid(int height)
    {
        Pyramid(height, 1);
    }
}