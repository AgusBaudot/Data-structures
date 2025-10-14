using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using TMPro;

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

        EnforceSingleChildPlacement(root);

        CenterTreeOnRoot(root);
        
        //Finally, instantiate visuals (nodes and respective connectors).
        CreateVisualsRecursive(root, null);
    }

    private void CenterTreeOnRoot(object root)
    {
        if (root == null) return;

        if (!_positions.TryGetValue(root, out var rootPos)) return;
        if (Mathf.Approximately(_rootOffset.x - rootPos.x, 0f)) return;

        var keys = _positions.Keys.ToArray();
        foreach (var k in keys)
        {
            _positions[k] = new Vector2(_positions[k].x + (_rootOffset.x - rootPos.x), _positions[k].y);
        }
    }

    private void EnforceSingleChildPlacement(object root)
    {
        if (root == null) return;
        Type nodeType = root.GetType();
        var left = nodeType.GetProperty("Left")?.GetValue(root);
        var right = nodeType.GetProperty("Right")?.GetValue(root);

        // Recurse first so children positions exist
        if (left != null) EnforceSingleChildPlacement(left);
        if (right != null) EnforceSingleChildPlacement(right);

        // If only left child exists
        if (left != null && right == null)
        {
            Vector2 parentPos = _positions[root];
            // child should be under-left of parent: x - horizontalSpacing, y - verticalSpacing
            Vector2 desiredLeftPos = parentPos + new Vector2(-_horizontalSpacing, -_verticalSpacing);
            Vector2 currentLeftPos = _positions[left];
            Vector2 delta = desiredLeftPos - currentLeftPos;
            if (delta != Vector2.zero) ShiftSubtree(left, delta);
        }
        // If only right child exists
        else if (right != null && left == null)
        {
            Vector2 parentPos = _positions[root];
            // child should be under-right of parent: x + horizontalSpacing, y - verticalSpacing
            Vector2 desiredRightPos = parentPos + new Vector2(_horizontalSpacing, -_verticalSpacing);
            Vector2 currentRightPos = _positions[right];
            Vector2 delta = desiredRightPos - currentRightPos;
            if (delta != Vector2.zero) ShiftSubtree(right, delta);
        }
    }

    private void ShiftSubtree(object node, Vector2 delta)
    {
        if (node == null || delta == Vector2.zero) return;

        if (_positions.ContainsKey(node))
            _positions[node] += delta;

        //Recurse
        Type nodeType = node.GetType();
        var left = nodeType.GetProperty("Left")?.GetValue(node, null);
        var right = nodeType.GetProperty("Right")?.GetValue(node, null);
        if (left != null) ShiftSubtree(left, delta);
        if (right != null) ShiftSubtree(right, delta);
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

        // Find the connector object
        Transform connT = nodeRt.Find(connectorName);
        if (connT == null) return; // prefab doesn't have that child, nothing to do

        // Ensure the connector GameObject is active/inactive
        connT.gameObject.SetActive(active);

        // Re-parent connector into the container so we can work in container-local anchored positions
        if (connT.parent != _treeContainer && _treeContainer != null)
        {
            connT.SetParent(_treeContainer, true); // keep world position while moving under container
        }

        if (!active || childRt == null) return;

        // Compute vector from node to child in container-space anchored positions
        Vector2 a = nodeRt.anchoredPosition;
        Vector2 b = childRt.anchoredPosition;

        // Check for LineRenderer component
        var line = connT.GetComponent<LineRenderer>();
        if (line != null)
        {
            // Ensure the line uses local space
            line.useWorldSpace = false; // This allows us to work with local anchored positions
            line.positionCount = 2;

            // Convert 2D anchored positions into 3D local positions (z = 0)
            Vector3 p0 = new Vector3(a.x, a.y, 0f); // Parent position (node)
            Vector3 p1 = new Vector3(b.x, b.y, 0f); // Child position (node)

            // Set the positions of the line
            line.SetPosition(0, p0);
            line.SetPosition(1, p1);
            return;
        }

        // Otherwise, treat the connector as a RectTransform (fallback)
        RectTransform connRt = connT as RectTransform;
        if (connRt == null) return;

        float distance = Vector2.Distance(a, b);
        float thickness = Mathf.Max(2f, nodeRt.sizeDelta.y * 0.08f); // tweak thickness relative to node
        connRt.sizeDelta = new Vector2(distance, thickness); // Adjust the length of the connector
        connRt.anchoredPosition = a + (b - a) * 0.5f; // Middle point

        // Calculate angle between the two nodes
        float angle = Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
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

    #endregion
}
