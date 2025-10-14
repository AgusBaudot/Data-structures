using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;


public class LeaderboardManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField scoreInput;
    [SerializeField] private Button insertButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_Text leaderboardText;
    [SerializeField] private TMP_Text traversalOutputText;
    [SerializeField] private GameObject textPrefab;
    [SerializeField] private GameObject content;

    [Header("Traversal Buttons")]
    [SerializeField] private Button preorderButton;
    [SerializeField] private Button inorderButton;
    [SerializeField] private Button postorderButton;
    [SerializeField] private Button levelorderButton;

    [Header("Seed settings")]
    [SerializeField] private int seedCount = 10;
    [SerializeField] private int randomMin = 0;
    [SerializeField] private int randomMax = 1000;

    private string[] names = new string[]
    {
        "James", "Mary", "John", "Patricia", "Robert", "Jennifer", "Michael", "Linda", "William", "Elizabeth",
        "David", "Barbara", "Richard", "Susan", "Joseph", "Jessica", "Thomas", "Sarah", "Charles", "Karen",
        "Christopher", "Nancy", "Daniel", "Lisa", "Matthew", "Margaret", "Anthony", "Betty", "Mark", "Sandra",
        "Donald", "Ashley", "Steven", "Dorothy", "Paul", "Kimberly", "Andrew", "Emily", "Joshua", "Donna",
        "Kenneth", "Michelle", "Kevin", "Carol", "Brian", "Amanda", "George", "Melissa", "Edward", "Deborah",
        "Ronald", "Stephanie", "Timothy", "Rebecca", "Jason", "Laura", "Jeffrey", "Sharon", "Ryan", "Cynthia",
        "Jacob", "Kathleen", "Gary", "Amy", "Nicholas", "Shirley", "Eric", "Angela", "Jonathan", "Helen",
        "Stephen", "Anna", "Larry", "Brenda", "Justin", "Pamela", "Scott", "Nicole", "Brandon", "Emma",
        "Benjamin", "Samantha", "Samuel", "Katherine", "Gregory", "Christine", "Frank", "Debra", "Alexander", "Rachel",
        "Raymond", "Catherine", "Patrick", "Carolyn", "Jack", "Janet", "Dennis", "Ruth", "Jerry", "Maria"

    };

    private AVLTree<int> _scores = new AVLTree<int>();
    private Dictionary<int, GameObject> texts = new Dictionary<int, GameObject>();

    private void Awake()
    {
        // hook up UI callbacks (if buttons not wired in inspector)
        if (insertButton != null) insertButton.onClick.AddListener(OnInsertClicked);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClicked);
        if (preorderButton != null) preorderButton.onClick.AddListener(OnPreorderClicked);
        if (inorderButton != null) inorderButton.onClick.AddListener(OnInorderClicked);
        if (postorderButton != null) postorderButton.onClick.AddListener(OnPostorderClicked);
        if (levelorderButton != null) levelorderButton.onClick.AddListener(OnLevelorderClicked);
    }

    private void Start()
    {
        SeedRandomScores(seedCount);
        RefreshLeaderboardUI();
    }

    private void OnDestroy()
    {
        // cleanup listeners to avoid possible memory leaks in editor
        if (insertButton != null)
            insertButton.onClick.RemoveListener(OnInsertClicked);
        if (deleteButton != null)
            deleteButton.onClick.RemoveListener(OnDeleteClicked);
        if (preorderButton != null)
            preorderButton.onClick.RemoveListener(OnPreorderClicked);
        if (inorderButton != null)
            inorderButton.onClick.RemoveListener(OnInorderClicked);
        if (postorderButton != null)
            postorderButton.onClick.RemoveListener(OnPostorderClicked);
        if (levelorderButton != null)
            levelorderButton.onClick.RemoveListener(OnLevelorderClicked);
    }

    private void SeedRandomScores(int count)
    {
        var rng = new System.Random();
        while (_scores.Count < seedCount)
        {
            int score = rng.Next(randomMin, randomMax + 1);
            int name = rng.Next(0, 100);
            _scores.Insert(score);
            var go = Instantiate(textPrefab, content.transform);
            texts[score] = go;

            go.GetComponent<TextMeshProUGUI>().text = $"Player: {names[name]}, score: {score.ToString()}.";
        }
    }

    // Called by Insert button
    public void OnInsertClicked()
    {
        string text = scoreInput != null ? scoreInput.text : string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning("Enter a score to insert.");
            return;
        }

        if (!int.TryParse(text.Trim(), out int value))
        {
            Debug.LogWarning($"Invalid integer: '{text}'.");
            return;
        }

        if (texts.TryGetValue(value, out var scoreText)) return;

        _scores.Insert(value);
        // clear input for convenience
        if (scoreInput != null) scoreInput.text = string.Empty;

        var go = Instantiate(textPrefab, content.transform);
        texts[value] = go;

        var rng = new System.Random();
        int name = rng.Next(0, 100);

        go.GetComponent<TextMeshProUGUI>().text = $"Player: {names[name]}, score: {value.ToString()}.";

        RefreshLeaderboardUI();
    }

    public void OnDeleteClicked()
    {
        string text = scoreInput != null ? scoreInput.text : string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogWarning("Enter a score to delete.");
            return;
        }
        if (!int.TryParse (text.Trim(), out int value))
        {
            Debug.LogWarning($"Invalid integer: '{text}'.");
            return;
        }

        _scores.Delete(value);
        // clear input for convenience
        if (scoreInput != null) scoreInput.text = string.Empty;

        Destroy(texts[value]);
        texts.Remove(value);

        RefreshLeaderboardUI();
    }

    private void RefreshLeaderboardUI()
    {
    }

    #region Traversal button callbacks

    private void OnPreorderClicked()
    {
        if (traversalOutputText == null) return;
        StringBuilder sb = new StringBuilder();
        bool first = true;
        _scores.PreOrderTraversal((v) =>
        {
            if (!first) sb.Append(", ");
            sb.Append(v);
            first = false;
        });
        traversalOutputText.text = $"Pre-order:\n{sb}";
    }

    private void OnInorderClicked()
    {
        if (traversalOutputText == null) return;
        StringBuilder sb = new StringBuilder();
        bool first = true;
        _scores.InOrderTraversal((v) =>
        {
            if (!first) sb.Append(", ");
            sb.Append(v);
            first = false;
        });
        traversalOutputText.text = $"In-order (sorted ascending):\n{sb}";
    }

    private void OnPostorderClicked()
    {
        if (traversalOutputText == null) return;
        StringBuilder sb = new StringBuilder();
        bool first = true;
        _scores.PostOrderTraversal((v) =>
        {
            if (!first) sb.Append(", ");
            sb.Append(v);
            first = false;
        });
        traversalOutputText.text = $"Post-order:\n{sb}";
    }

    private void OnLevelorderClicked()
    {
        if (traversalOutputText == null) return;
        var nodes = LevelOrderValues();
        traversalOutputText.text = $"Level-order:\n{string.Join(", ", nodes)}";
    }

    #endregion

    // Level-order traversal implemented locally using the BSTNode<int> type.
    private List<int> LevelOrderValues()
    {
        var result = new List<int>();
        var root = _scores.Root;
        if (root == null) return result;

        var q = new Queue<BSTNode<int>>();
        q.Enqueue(root);

        while (q.Count > 0)
        {
            var n = q.Dequeue();
            if (n == null) continue;
            result.Add(n.Data);
            if (n.Left != null) q.Enqueue(n.Left);
            if (n.Right != null) q.Enqueue(n.Right);
        }

        return result;
    }
}