using UnityEngine;

public class PalindromeDetector : MonoBehaviour
{
    private void Start() => UIManager.Instance.OnPalindromePressed += DoPalindrome;

    private bool IsPalindrome(string input, int left, int right)
    {
        // Base case: if we've checked all the characters
        if (left >= right) return true;

        // If the characters at the current bounds don't match, it's not a palindrome
        if (input[left] != input[right]) return false;

        // Move towards the center
        return IsPalindrome(input, left + 1, right - 1);
    }

    public bool Palindrome(string input)
    {
        if (string.IsNullOrEmpty(input)) return false;

        input = input.ToLower().Replace(" ", ""); // Normalize input
        return IsPalindrome(input, 0, input.Length - 1);
    }

    public void DoPalindrome(string word) => UIManager.Instance.GetText().text = Palindrome(word).ToString();
}
