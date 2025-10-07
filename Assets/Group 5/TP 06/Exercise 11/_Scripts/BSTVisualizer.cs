using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

public class BSTVisualizer : MonoBehaviour
{
    [SerializeField] private TMP_Text _treeVisualizerText;
    [SerializeField] private TMP_Text _treeCountVisualizerText;
    [SerializeField] private TMP_Text _treeIsEmpty;
    [SerializeField] private TMP_Dropdown _typeDropdown;
    [SerializeField] private TMP_Dropdown _methodDropdown;
    [SerializeField] private TMP_InputField _inputField;

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
            _treeIsEmpty.text = $"Is empty: {_currentTree.GetType().GetMethod("IsEmpty", Type.EmptyTypes).Invoke(_currentTree, null)}";
            return;
        }

        // Update Count
        var countProp = _currentTree.GetType().GetProperty("Count");
        int count = countProp != null ? (int)countProp.GetValue(_currentTree) : -1;

        // Update Height (try to call GetHeight())
        int height = -1;
        MethodInfo heightMethod = _currentTree.GetType().GetMethod("GetHeight", Type.EmptyTypes);
        if (heightMethod != null) height = (int)heightMethod.Invoke(_currentTree, null);

        // Build in-order representation (safe)
        object root = _currentTree.GetType().GetProperty("Root")?.GetValue(_currentTree);
        string inOrder = BuildInOrderString(root);

        _treeVisualizerText.text = $"In-order: {inOrder}";
        _treeCountVisualizerText.text = $"Count: {count}\nHeight: {height}";
        _treeIsEmpty.text = $"Is empty: {_currentTree.GetType().GetMethod("IsEmpty", Type.EmptyTypes).Invoke(_currentTree, null)}";
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
}
