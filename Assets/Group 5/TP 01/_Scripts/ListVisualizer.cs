using System;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

public class ListVisualizer : MonoBehaviour
{
    [SerializeField] private TMP_Text _listVisualizerText;
    [SerializeField] private TMP_Text _listCountVisualizerText;
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
			case 0: _currentList = new SimpleList<string>(2); break;
			case 1: _currentList = new SimpleList<int>(2); break;
			case 2: _currentList = new SimpleList<float>(2); break;
		}
		UpdateList();
	}

	public void GetInput()
	{
		switch ((_methodDropdown.value))
		{
			case 0: Add(_inputField.text); break;
			case 1: AddRangeFromInput(_inputField.text); break;
			case 2: Remove(_inputField.text); break;
			case 3: Clear(); break;
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
			((SimpleList<int>)_currentList).Add(parsed);
		}
		else if (elementType == typeof(float))
		{
			if (!float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsed))	
				throw new FormatException($"Cannot parse '{input}' as a float.");
			CheckType(parsed);
			((SimpleList<float>)_currentList).Add(parsed);
		}
		else if (elementType == typeof(string))
		{
			string s = input;
			CheckType(s);
			((SimpleList<string>)_currentList).Add(s);
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
		
		//Split & trim, drop empty entries.
		string[] parts = input.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(p => p.Length > 0).ToArray();

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
			((SimpleList<int>)_currentList).AddRange(items);
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
			((SimpleList<float>)_currentList).AddRange(items);
		}
		else if (elementType == typeof(string))
		{
			string[] items = parts;
			foreach (var s in items) CheckType(s);
			((SimpleList<string>)_currentList).AddRange(items);
		}
		else
		{
			throw new NotSupportedException($"The type {elementType} is not supported.");
		}
		UpdateList();
	}

	private void Remove(string input)
	{
		Type elementType =  _currentList.GetType().GetGenericArguments()[0];

		if (elementType == typeof(int))
		{
			if (!int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
				throw new FormatException($"Cannot parse '{input}' as an integer.");
			CheckType(parsed);
			bool removed = ((SimpleList<int>)_currentList).Remove(parsed);
			Debug.Log(removed ? "Item removed." : "Item not found.");
		}
		else if (elementType == typeof(float))
		{
			if (!float.TryParse(input, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float parsed))
				throw new  FormatException($"Element {input} ('{input}') is not a valid float.");
			CheckType(parsed);
			bool removed = ((SimpleList<float>)_currentList).Remove(parsed);
			Debug.Log(removed ? "Item removed." : "Item not found.");
		}
		else if (elementType == typeof(string))
		{
			CheckType(input);
			bool removed = ((SimpleList<string>)_currentList).Remove(input);
			Debug.Log(removed ? "Item removed." : "Item not found.");
		}
		else
		{
			throw new NotSupportedException($"The type  {elementType} is not supported.");
		}
		UpdateList();
	}

	private void Clear()
	{
		Type elementType = _currentList.GetType().GetGenericArguments()[0];

		if (elementType == typeof(int))
		{
			((SimpleList<int>)_currentList).Clear();
		}
		else if (elementType == typeof(float))
		{
			((SimpleList<float>)_currentList).Clear();
		}
		else if (elementType == typeof(string))
		{
			((SimpleList<string>)_currentList).Clear();
		}
		else
		{
			Debug.LogError($"Type {elementType} is not supported.");
			return;
		}
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
			throw new ArgumentException($"Type mismatch: expected {elementType.Name}, got  {value.GetType().Name}.");
	}

	private void UpdateList()
	{
		_listVisualizerText.text = $"List: [{_currentList}]";
		_listCountVisualizerText.text = $"Count: {(int)_currentList.GetType().GetProperty("Count").GetValue(_currentList)}";
    }
}