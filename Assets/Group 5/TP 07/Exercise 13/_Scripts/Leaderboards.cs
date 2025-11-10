using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

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
    private Dictionary<int, GameObject> _texts = new Dictionary<int, GameObject>();

    private void Awake()
    {
        //Hook up UI callbacks (if buttons not wired in inspector)
        if (insertButton != null) insertButton.onClick.AddListener(OnInsertClicked);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClicked);
        if (preorderButton != null) preorderButton.onClick.AddListener(OnPreorderClicked);
        if (inorderButton != null) inorderButton.onClick.AddListener(OnInorderClicked);
        if (postorderButton != null) postorderButton.onClick.AddListener(OnPostorderClicked);
        if (levelorderButton != null) levelorderButton.onClick.AddListener(OnLevelorderClicked);
    }

    private void Start()
    {
        SeedRandomScores();
        RefreshLeaderboardUI();
    }

    private void OnDestroy()
    {
        //Cleanup listeners to avoid possible memory leaks in editor
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

    private void SeedRandomScores()
    {
        var rng = new Random();
        int attempts = 0;
        int maxAttempts = Mathf.Max(10, seedCount * 10); //Safety cap
        
        while (_scores.Count < seedCount && attempts < maxAttempts)
        {
            attempts++;
            
            int score = rng.Next(randomMin, randomMax + 1);
            int name = rng.Next(0, names.Length);
            
            if (!_scores.Insert(score)) continue;
            
            var go = Instantiate(textPrefab, content.transform);
            go.GetComponent<TextMeshProUGUI>().text = $"Player: {names[name]}, score: {score.ToString()}.";
            _texts[score] = go;
            if (_scores.Count == seedCount) break;
        }
    }

    //Called by Insert button
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

        if (_texts.ContainsKey(value)) return;

        if (!_scores.Insert(value)) return;
        
        //Clear input for convenience
        if (scoreInput != null) scoreInput.text = string.Empty;

        var go = Instantiate(textPrefab, content.transform);
        go.GetComponent<TextMeshProUGUI>().text = $"Player: {names[new Random().Next(names.Length)]}, score: {value}.";
        _texts[value] = go;

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
        
        //Clear input for convenience
        if (scoreInput != null) scoreInput.text = string.Empty;

        if (_texts.TryGetValue(value, out var go))
        {
            Destroy(go);
            _texts.Remove(value);
        }

        RefreshLeaderboardUI();
    }

    private void RefreshLeaderboardUI()
    {
        int order = 0;
        _scores.InOrderTraversal(v =>
        {
            _texts[v].transform.SetSiblingIndex(order);
            order++;
        });
    }

    #region Traversal button callbacks

    private void OnPreorderClicked()
    {
        //If (traversalOutputText == null) return;
        StringBuilder sb = new StringBuilder();
        bool first = true;
        _scores.PreOrderTraversal((v) =>
        {
            if (!first) sb.Append(", ");
            sb.Append(v);
            first = false;
        });
        //TraversalOutputText.text = $"Pre-order:\n{sb}";
        Debug.Log($"Pre-order:\n{sb}");
    }

    private void OnInorderClicked()
    {
        //If (traversalOutputText == null) return;
        StringBuilder sb = new StringBuilder();
        bool first = true;
        _scores.InOrderTraversal((v) =>
        {
            if (!first) sb.Append(", ");
            sb.Append(v);
            first = false;
        });
        //TraversalOutputText.text = $"In-order (sorted ascending):\n{sb}";
        Debug.Log($"In-order (sorted ascending):\n{sb}");
    }

    private void OnPostorderClicked()
    {
        //If (traversalOutputText == null) return;
        StringBuilder sb = new StringBuilder();
        bool first = true;
        _scores.PostOrderTraversal((v) =>
        {
            if (!first) sb.Append(", ");
            sb.Append(v);
            first = false;
        });
        //TraversalOutputText.text = $"Post-order:\n{sb}";
        Debug.Log($"Post-order:\n{sb}");
    }

    private void OnLevelorderClicked()
    {
        //If (traversalOutputText == null) return;
        var nodes = LevelOrderValues();
        //TraversalOutputText.text = $"Level-order:\n{string.Join(", ", nodes)}";
        Debug.Log($"Level-oder:\n{string.Join(", ", nodes)}");
    }

    #endregion

    //Level-order traversal implemented locally using the BSTNode<int> type.
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