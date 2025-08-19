using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LinkedListVisualizer : MonoBehaviour
{
    [SerializeField] private TMP_Text _listVisualizerText;
    [SerializeField] private TMP_Text _listCountVisualizerText;
    [SerializeField] private TMP_Text _listIsEmptyText;
    [SerializeField] private TMP_Dropdown _typeDropdown;
    [SerializeField] private TMP_Dropdown _methodDropdown;
    [SerializeField] private TMP_InputField _inputField;

    private object _currentList;

    private void Start()
    {
        SwitchList();
    }

    public void SwitchList()
    {
        switch (_typeDropdown.value)
        {
            case 0: _currentList = new MyList<string>(); break;
            case 1: _currentList = new MyList<int>(); break;
            case 2: _currentList = new MyList<float>(); break;
            default: _currentList = new MyList<string>(); break;
        }

        UpdateList();
    }

    public void GetInput()
    {
        switch (_methodDropdown.value)
        {
            case 0: Add(_inputField.text); break;
            case 1: AddRangeFromInput(_inputField.text); break;
            case 2: Remove(_inputField.text); break;
            case 3: RemoveAt(_inputField.text); break;
            case 4: InsertFromInput(_inputField.text); break;
            case 5: Clear(); break;
            case 6: Sort(); break;
            default: 
                Debug.LogWarning("Unknown method selected.");
                break;
        }

        _inputField.text = string.Empty;
    }

    private void Add(string input)
    {
        Type elementType = _currentList.GetType().GetGenericArguments()[0];

        if (elementType == typeof(int))
        {
            if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                throw new FormatException($"Cannot parse '{input}' as an integer.");
            
            CheckType(parsed);
            ((MyList<int>)_currentList).Add(parsed);
        }
        else if (elementType == typeof(float))
        {
            if (!float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsed))
                throw new FormatException($"Cannot parse '{input}' as a float.");
            
            CheckType(parsed);
            ((MyList<float>)_currentList).Add(parsed);
        }
        else if (elementType == typeof(string))
        {
            string s = input;
            
            CheckType(s);
            ((MyList<string>)_currentList).Add(s);
        }
        else
        {
            throw new NotSupportedException($"The type {elementType} is not supported.");
        }

        UpdateList();
    }

    private void AddRangeFromInput(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input is empty.");

        // Split & trim, drop empty entries.
        string[] parts = input.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToArray();

        Type elementType = _currentList.GetType().GetGenericArguments()[0];

        if (elementType == typeof(int))
        {
            int[] items = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                    throw new FormatException($"Element {i} ('{parts[i]}') is not a valid int.");
                
                CheckType(parsed);
                items[i] = parsed;
            }
            ((MyList<int>)_currentList).AddRange(items);
        }
        else if (elementType == typeof(float))
        {
            float[] items = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsed))
                    throw new FormatException($"Element {i} ('{parts[i]}') is not a valid float.");
                
                CheckType(parsed);
                items[i] = parsed;
            }
            ((MyList<float>)_currentList).AddRange(items);
        }
        else if (elementType == typeof(string))
        {
            string[] items = parts;
            
            foreach (var s in items) CheckType(s);
            ((MyList<string>)_currentList).AddRange(items);
        }
        else
        {
            throw new NotSupportedException($"The type {elementType} is not supported.");
        }

        UpdateList();
    }

    private void Remove(string input)
    {
        Type elementType = _currentList.GetType().GetGenericArguments()[0];

        if (elementType == typeof(int))
        {
            if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                throw new FormatException($"Cannot parse '{input}' as an integer.");
            
            CheckType(parsed);
            bool removed = ((MyList<int>)_currentList).Remove(parsed);
            Debug.Log(removed ? "Item removed." : "Item not found.");
        }
        else if (elementType == typeof(float))
        {
            if (!float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsed))
                throw new FormatException($"Cannot parse '{input}' as a float.");
            
            CheckType(parsed);
            bool removed = ((MyList<float>)_currentList).Remove(parsed);
            Debug.Log(removed ? "Item removed." : "Item not found.");
        }
        else if (elementType == typeof(string))
        {
            CheckType(input);
            bool removed = ((MyList<string>)_currentList).Remove(input);
            Debug.Log(removed ? "Item removed." : "Item not found.");
        }
        else
        {
            throw new NotSupportedException($"The type {elementType} is not supported.");
        }

        UpdateList();
    }

    private void RemoveAt(string input)
    {
        if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index))
            throw new FormatException($"Cannot parse '{input}' as an integer index.");

        var count = (int)_currentList.GetType().GetProperty("Count").GetValue(_currentList);
        
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of bounds.");

        Type elementType = _currentList.GetType().GetGenericArguments()[0];
        
        if (elementType == typeof(int))
            ((MyList<int>)_currentList).RemoveAt(index);
        
        else if (elementType == typeof(float))
            ((MyList<float>)_currentList).RemoveAt(index);
        
        else if (elementType == typeof(string))
            ((MyList<string>)_currentList).RemoveAt(index);
        
        else
            throw new NotSupportedException($"The type {elementType} is not supported.");

        UpdateList();
    }

    private void InsertFromInput(string input) //Format: "index, value
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input is empty. For Insert use: index,value");

        var parts = input.Split(new[] { ", " }, 2, StringSplitOptions.None).Select(p => p.Trim()).ToArray();
        
        if (parts.Length < 2)
            throw new ArgumentException("Wrong format. For Insert use: index,value");

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int index))
            throw new FormatException($"Cannot parse index '{parts[0]}' as integer.");

        Type elementType = _currentList.GetType().GetGenericArguments()[0];
        
        var count = (int)_currentList.GetType().GetProperty("Count").GetValue(_currentList);
        if (index < 0 || index > count)
            throw new ArgumentOutOfRangeException(nameof(index), "Index is out of bounds (allowed: 0..Count).");

        if (elementType == typeof(int))
        {
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
                throw new FormatException($"Cannot parse '{parts[1]}' as an integer.");
                    
            CheckType(parsed);
            ((MyList<int>)_currentList).Insert(index, parsed);
        }
        else if (elementType == typeof(float))
        {
            if (!float.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsed))
                throw new FormatException($"Cannot parse '{parts[1]}' as a float.");
            
            CheckType(parsed);
            ((MyList<float>)_currentList).Insert(index, parsed);
        }
        else if (elementType == typeof(string))
        {
            string s = parts[1];
            CheckType(s);
            ((MyList<string>)_currentList).Insert(index, s);
        }
        else
        {
            throw new NotSupportedException($"The type {elementType} is not supported.");
        }

        UpdateList();
    }

    private void Clear()
    {
        Type elementType = _currentList.GetType().GetGenericArguments()[0];

        if (elementType == typeof(int))
            ((MyList<int>)_currentList).Clear();
        
        else if (elementType == typeof(float))
            ((MyList<float>)_currentList).Clear();
        
        else if (elementType == typeof(string))
            ((MyList<string>)_currentList).Clear();
        
        else
            throw new NotSupportedException($"The type {elementType} is not supported.");

        UpdateList();
    }

    private void Sort()
    {
        Type elementType = _currentList.GetType().GetGenericArguments()[0];

        if (elementType == typeof(int))
            ((MyList<int>)_currentList).Sort();
        
        else if (elementType == typeof(float))
            ((MyList<float>)_currentList).Sort();
        
        else if (elementType == typeof(string))
            ((MyList<string>)_currentList).Sort();
        
        else
            throw new NotSupportedException($"The type {elementType} is not supported.");

        UpdateList();
    }

    private void CheckType(object value)
    {
        if (_currentList == null)
            throw new InvalidOperationException("No list is currently active.");

        Type elementType = _currentList.GetType().GetGenericArguments()[0];
        
        if (value == null)
        {
            if (elementType.IsValueType)
                throw new ArgumentNullException($"Type mismatch: cannot pass null to list of {elementType.Name}.");
            return;
        }

        if (value.GetType() != elementType)
            throw new ArgumentException($"Type mismatch: expected {elementType.Name}, got {value.GetType().Name}.");
    }

    private void UpdateList()
    {
        _listVisualizerText.text = $"List: [{_currentList}]";
        _listCountVisualizerText.text =
            $"Count: {(int)_currentList.GetType().GetProperty("Count").GetValue(_currentList)}";
        _listIsEmptyText.text = $"Is empty? {_currentList.GetType().GetMethod("IsEmpty").Invoke(_currentList, null)}";
    }
}
