using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

public class BSTVisualizer : MonoBehaviour
{
    #region TXTs and DDs

    [Header("UI")]
    [SerializeField] private TMP_Text _treeVisualizerText;
    [SerializeField] private TMP_Text _treeCountVisualizerText;
    [SerializeField] private TMP_Text _treeIsEmpty;
    [SerializeField] private TMP_Dropdown _typeDropdown;
    [SerializeField] private TMP_Dropdown _methodDropdown;
    [SerializeField] private TMP_InputField _inputField;

    #endregion

    #region Tree visualization

    [Header("Scene tree visualization")]
    [SerializeField] private RectTransform _treeContainer;
    [SerializeField] private GameObject _nodePrefab; //Connector is child

    [SerializeField] private float _horizontalSpacing = 160f;
    [SerializeField] private float _verticalSpacing = 120f;
    [SerializeField] private float _leafSpacing = 1f;
    [SerializeField] private Vector2 _rootOffset = default; //Offset for whole tree.
    
    private readonly Dictionary<object, float> _subtreeWidth = new();
    private readonly Dictionary<object, Vector2> _positions = new();
    private readonly Dictionary<object, RectTransform> _nodeToRect = new();
    private readonly List<GameObject> _connectors = new();

    #endregion

    // Holds the current MyBST<T> instance boxed as object
    private object _currentTree;

    private void Start()
    {
        SwitchTree();
    }

    // Called when user changes the type dropdown
    public void SwitchTree()
    {
        switch (_typeDropdown.value)
        {
            case 0: _currentTree = CreateGenericBSTInstance(typeof(string)); break;
            case 1: _currentTree = CreateGenericBSTInstance(typeof(int)); break;
            case 2: _currentTree = CreateGenericBSTInstance(typeof(float)); break;
            default: _currentTree = CreateGenericBSTInstance(typeof(int)); break;
        }

        UpdateTree();
    }

    // Called by UI button to execute the selected method
    public void GetInput()
    {
        try
        {
            switch (_methodDropdown.value)
            {
                case 0: Insert(_inputField.text); break;
                case 1: AddRangeFromInput(_inputField.text); break;
                case 2: Remove(_inputField.text); break;
                case 3: Contains(_inputField.text); break;
                case 4: GetHeight(); break;
                case 5: GetBalanceFactor(_inputField.text); break;
                case 6: PrintInOrder(); break;
                case 7: Clear(); break;
                default: break;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Operation failed: {ex.GetType().Name}: {ex.Message}");
        }

        _inputField.text = string.Empty;
    }

    private void Insert(string input)
    {
        Type elementType = GetTreeElementType();
        object parsed = ParseInputForType(input, elementType);
        if (parsed == null && elementType.IsValueType)
            throw new ArgumentException("Invalid input.");

        // Call Insert(T data)
        MethodInfo insertMethod = _currentTree.GetType().GetMethod("Insert", new Type[] { elementType });
        if (insertMethod == null) throw new MissingMethodException("Insert method not found on BST.");

        insertMethod.Invoke(_currentTree, new[] { parsed });
        UpdateTree();
    }

    private void AddRangeFromInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) throw new ArgumentException("Input is empty.");

        string[] parts = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(p => p.Trim()).Where(p => p.Length > 0).ToArray();

        Type elementType = GetTreeElementType();

        MethodInfo insertMethod = _currentTree.GetType().GetMethod("Insert", new Type[] { elementType });
        if (insertMethod == null) throw new MissingMethodException("Insert method not found on BST.");

        foreach (var p in parts)
        {
            object parsed = ParseInputForType(p, elementType);
            if (parsed == null && elementType.IsValueType)
                throw new FormatException($"Element '{p}' is not valid for type {elementType.Name}.");

            insertMethod.Invoke(_currentTree, new[] { parsed });
        }

        UpdateTree();
    }

    private void Remove(string input)
    {
        Type elementType = GetTreeElementType();
        object parsed = ParseInputForType(input, elementType);

        MethodInfo deleteMethod = _currentTree.GetType().GetMethod("Delete", new Type[] {elementType});
        if (deleteMethod == null) throw new MissingMethodException("Delete method not found on BST.");

        deleteMethod.Invoke(_currentTree, new[] { parsed });
        UpdateTree();
    }

    private void Contains(string input)
    {
        Type elementType = GetTreeElementType();
        object parsed = ParseInputForType(input, elementType);

        MethodInfo containsMethod = _currentTree.GetType().GetMethod("Contains", new Type[] { elementType });
        if (containsMethod == null) throw new MissingMethodException("Contains method not found on BST.");

        bool result = (bool)containsMethod.Invoke(_currentTree, new[] { parsed });
        Debug.Log(result ? $"Tree contains {input}." : $"Tree does NOT contain {input}.");
    }

    private void GetHeight()
    {
        MethodInfo heightMethod = _currentTree.GetType().GetMethod("GetHeight", Type.EmptyTypes);
        if (heightMethod == null) throw new MissingMethodException("GetHeight method not found on BST.");
        int h = (int)heightMethod.Invoke(_currentTree, null);
        Debug.Log($"Tree height: {h}");
        UpdateTree();
    }

    private void GetBalanceFactor(string input)
    {
        Type elementType = GetTreeElementType();
        object parsed = ParseInputForType(input, elementType);

        MethodInfo bfMethod = _currentTree.GetType().GetMethod("GetBalanceFactor", new Type[] { elementType });
        if (bfMethod == null) throw new MissingMethodException("GetBalanceFactor(T) not found on BST.");

        try
        {
            int bf = (int)bfMethod.Invoke(_currentTree, new[] { parsed });
            Debug.Log($"BalanceFactor({input}) = {bf}");
        }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
            Debug.LogWarning($"Error getting balance factor: {tie.InnerException.Message}");
        }
    }

    private void PrintInOrder()
    {
        // We'll build an in-order string by reflecting into the Root and traversing node objects.
        object root = _currentTree.GetType().GetProperty("Root").GetValue(_currentTree);
        string inOrder = BuildInOrderString(root);
        _treeVisualizerText.text = $"In-order: {inOrder}";
    }

    private void Clear()
    {
        MethodInfo clearMethod = _currentTree.GetType().GetMethod("Clear", Type.EmptyTypes);
        if (clearMethod == null) throw new MissingMethodException("Clear method not found on BST.");
        clearMethod.Invoke(_currentTree, null);
        UpdateTree();
    }

    // ----------------------
    // Update display / helpers
    // ----------------------
    private void UpdateTree()
    {
        if (_currentTree == null)
        {
            _treeVisualizerText.text = "No tree.";
            _treeCountVisualizerText.text = "Count: 0";
            _treeIsEmpty.text = "Is empty: null";
            ClearTreeVisuals();
            return;
        }
        
        var countProp = _currentTree.GetType().GetProperty("Count");
        int count = countProp != null ? (int)countProp.GetValue(_currentTree) : -1;

        int height = -1;
        MethodInfo heightMethod = _currentTree.GetType().GetMethod("GetHeight", Type.EmptyTypes);
        if (heightMethod != null)
            height = (int)heightMethod.Invoke(_currentTree, null);
        
        object root = _currentTree.GetType().GetProperty("Root")?.GetValue(_currentTree);
        string inOrder = BuildInOrderString(root);
        
        _treeVisualizerText.text = $"In-order: {inOrder}";
        _treeCountVisualizerText.text = $"Count: {count}\nHeight: {height}";
        _treeIsEmpty.text =
            $"Is empty: {_currentTree.GetType().GetMethod("IsEmpty", Type.EmptyTypes).Invoke(_currentTree, null)}";

        if (_treeContainer == null || _nodePrefab == null)
        {
            ClearTreeVisuals();
            return;
        }

        ClearTreeVisuals();

        if (root == null) return;
        
        //First, compute subtree widths.
        _subtreeWidth.Clear();
        ComputeSubtreeWidth(root);
        
        //Then, compute anchored positions (center parent over children).
        _positions.Clear();
        float startX = 0f;
        ComputePositionsRecursive(root, 0, ref startX);
        
        //Finally, instantiate visuals (nodes and respective connectors).
        CreateVisualsRecursive(root, null);

        // if (_currentTree == null)
        // {
        //     _treeVisualizerText.text = "No tree.";
        //     _treeCountVisualizerText.text = "Count: 0";
        //     _treeIsEmpty.text = $"Is empty: {_currentTree.GetType().GetMethod("IsEmpty", Type.EmptyTypes).Invoke(_currentTree, null)}";
        //     return;
        // }
        //
        // // Update Count
        // var countProp = _currentTree.GetType().GetProperty("Count");
        // int count = countProp != null ? (int)countProp.GetValue(_currentTree) : -1;
        //
        // // Update Height (try to call GetHeight())
        // int height = -1;
        // MethodInfo heightMethod = _currentTree.GetType().GetMethod("GetHeight", Type.EmptyTypes);
        // if (heightMethod != null) height = (int)heightMethod.Invoke(_currentTree, null);
        //
        // // Build in-order representation (safe)
        // object root = _currentTree.GetType().GetProperty("Root")?.GetValue(_currentTree);
        // string inOrder = BuildInOrderString(root);
        //
        // _treeVisualizerText.text = $"In-order: {inOrder}";
        // _treeCountVisualizerText.text = $"Count: {count}\nHeight: {height}";
        // _treeIsEmpty.text = $"Is empty: {_currentTree.GetType().GetMethod("IsEmpty", Type.EmptyTypes).Invoke(_currentTree, null)}";
    }

    // Build a simple in-order traversal by reflecting into node objects.
    private string BuildInOrderString(object root)
    {
        if (root == null) return "(empty)";

        Type nodeType = root.GetType();
        PropertyInfo leftProp = nodeType.GetProperty("Left");
        PropertyInfo rightProp = nodeType.GetProperty("Right");
        PropertyInfo dataProp = nodeType.GetProperty("Data");

        var sb = new StringBuilder();
        void Recur(object node)
        {
            if (node == null) return;
            var left = leftProp.GetValue(node);
            Recur(left);
            var data = dataProp.GetValue(node);
            if (sb.Length > 0) sb.Append(", ");
            sb.Append(data?.ToString() ?? "null");
            var right = rightProp.GetValue(node);
            Recur(right);
        }

        Recur(root);

        return sb.ToString();
    }

    // Create a MyBST<T> instance for the requested element type
    private object CreateGenericBSTInstance(Type elementType)
    {
        Type generic = typeof(MyBST<>).MakeGenericType(elementType);
        return Activator.CreateInstance(generic);
    }

    private Type GetTreeElementType()
    {
        return _currentTree.GetType().GetGenericArguments()[0];
    }

    // Parse user input string to the required element type
    private object ParseInputForType(string input, Type elementType)
    {
        if (elementType == typeof(string))
            return input;
        if (elementType == typeof(int))
        {
            if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
                throw new FormatException($"Cannot parse '{input}' as int.");
            return i;
        }
        if (elementType == typeof(float))
        {
            if (!float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float f))
                throw new FormatException($"Cannot parse '{input}' as float.");
            return f;
        }

        throw new NotSupportedException($"Type {elementType.Name} not supported by visualizer.");
    }

    #region Helpers for scene

    private void ClearTreeVisuals()
    {
        if (_treeContainer == null) return;
        
#if UNITY_EDITOR
        for (int i = _treeContainer.childCount - 1; i >= 0; i--)
            DestroyImmediate(_treeContainer.GetChild(i).gameObject);
#else
        for (int i = _treeContainer.childCount - 1; i >= 0; i--)
            Destroy(_treeContainer.GetChild(i).gameObject);
#endif
        
        _nodeToRect.Clear();
        foreach (var c in _connectors)
            if (c != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(c);
#else
                Destroy(c);
#endif
            }
        
        _connectors.Clear();
        _subtreeWidth.Clear();
        _positions.Clear();
    }

    private float ComputeSubtreeWidth(object node)
    {
        if (node == null) return 0f;

        Type nodeType = node.GetType();
        var left = nodeType.GetProperty("Left")?.GetValue(node);
        var right = nodeType.GetProperty("Right")?.GetValue(node);

        float leftW = ComputeSubtreeWidth(left);
        float rightW = ComputeSubtreeWidth(right);

        float width;
        if (leftW <= 0 && rightW <= 0)
        {
            //leaf
            width = _leafSpacing;
        }
        else
        {
            width = Math.Max(_leafSpacing, leftW + rightW);
        }

        _subtreeWidth[node] = width;
        return width;
    }

    private void ComputePositionsRecursive(object node, int depth, ref float startX)
    {
        if (node == null) return;
        
        Type nodeType = node.GetType();
        var left = nodeType.GetProperty("Left")?.GetValue(node);
        var right = nodeType.GetProperty("Right")?.GetValue(node);

        float width = _subtreeWidth.ContainsKey(node) ? _subtreeWidth[node] : _leafSpacing;
        
        // leaf case
        if (left == null && right == null)
        {
            float center = startX + width / 2f;
            _positions[node] = new Vector2(center * _horizontalSpacing, -depth * _verticalSpacing) + _rootOffset;
            startX += width;
            return;
        }

        // otherwise compute left and right first so startX advances
        if (left != null)
            ComputePositionsRecursive(left, depth + 1, ref startX);

        if (right != null)
            ComputePositionsRecursive(right, depth + 1, ref startX);

        // Determine leftmost and rightmost child unit positions
        float leftMost = left != null ? _positions[left].x / _horizontalSpacing : startX;
        float rightMost = right != null ? _positions[right].x / _horizontalSpacing : (startX);

        float centerUnit;
        if (left != null && right != null)
            centerUnit = (leftMost + rightMost) / 2f;
        else if (left != null)
            centerUnit = _positions[left].x / _horizontalSpacing + (width - _subtreeWidth[left]) / 2f;
        else //right != null
            centerUnit = _positions[right].x / _horizontalSpacing - (width - _subtreeWidth[right]) / 2f;

        _positions[node] = new Vector2(centerUnit * _horizontalSpacing, -depth * _verticalSpacing) + _rootOffset;
    }
    
    private void CreateVisualsRecursive(object node, object parent)
    {
        if (node == null) return;

        Type nodeType = node.GetType();
        var left = nodeType.GetProperty("Left")?.GetValue(node);
        var right = nodeType.GetProperty("Right")?.GetValue(node);
        var data = nodeType.GetProperty("Data")?.GetValue(node);

        // Instantiate node prefab
        RectTransform rt = InstantiateNode(node, data?.ToString() ?? "null", _positions[node]);

        // Create connector from parent to this node
        // if (parent != null && _nodeToRect.TryGetValue(parent, out var parentRt))
        //     CreateConnector(parentRt, rt);

        // recurse
        CreateVisualsRecursive(left, node);
        CreateVisualsRecursive(right, node);
        
        //After everything, update child connectors.
        if (left != null && _nodeToRect.TryGetValue(left, out var leftRt))
            UpdateNodeChildConnector(rt, leftRt, "LeftConnector", true);
        else
            UpdateNodeChildConnector(rt, null, "LeftConnector", false);

        // Right connector:
        if (right != null && _nodeToRect.TryGetValue(right, out var rightRt))
            UpdateNodeChildConnector(rt, rightRt, "RightConnector", true);
        else
            UpdateNodeChildConnector(rt, null, "RightConnector", false);
    }
    
    private void UpdateNodeChildConnector(RectTransform nodeRt, RectTransform childRt, string connectorName, bool active)
    {
        if (nodeRt == null) return;
        Transform connT = nodeRt.Find(connectorName);
        if (connT == null) return; // prefab doesn't have that child, nothing to do

        RectTransform connRt = connT as RectTransform;
        connT.gameObject.SetActive(active);

        if (!active || childRt == null) return;

        // Compute vector from node to child in container-space anchored positions
        Vector2 a = nodeRt.anchoredPosition;
        Vector2 b = childRt.anchoredPosition;
        Vector2 diff = b - a;
        float distance = diff.magnitude;

        // connector pivot/origin expected at node center (0.5,0.5) and anchored to container center
        // adjust size and rotation
        float thickness = Mathf.Max(2f, nodeRt.sizeDelta.y * 0.08f); // tweak thickness relative to node
        connRt.sizeDelta = new Vector2(distance, thickness);
        connRt.anchoredPosition = a + diff * 0.5f; // middle point
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        connRt.localEulerAngles = new Vector3(0f, 0f, angle);
    }
    
    private RectTransform InstantiateNode(object node, string label, Vector2 anchoredPos)
    {
        GameObject go = Instantiate(_nodePrefab, _treeContainer);
        go.name = $"Node_{label}";
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) throw new InvalidOperationException("nodePrefab must have a RectTransform.");
        rt.anchoredPosition = anchoredPos;

        var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = label;

        _nodeToRect[node] = rt;
        return rt;
    }
    
    // private void CreateConnector(RectTransform from, RectTransform to)
    // {
    //     GameObject conn;
    //     if (connectorPrefab != null)
    //         conn = Instantiate(connectorPrefab, _treeContainer);
    //
    //     var rt = conn.GetComponent<RectTransform>();
    //     Vector2 a = from.anchoredPosition;
    //     Vector2 b = to.anchoredPosition;
    //     Vector2 diff = b - a;
    //     float dist = diff.magnitude;
    //
    //     rt.pivot = new Vector2(0.5f, 0.5f);
    //     rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
    //     rt.anchoredPosition = a + diff * 0.5f;
    //     float thickness = 4f;
    //     rt.sizeDelta = new Vector2(dist, thickness);
    //     float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
    //     rt.localEulerAngles = new Vector3(0f, 0f, angle);
    //
    //     _connectors.Add(conn);
    // }

    #endregion
}
