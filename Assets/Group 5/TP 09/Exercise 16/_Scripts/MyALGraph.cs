using System;
using System.Collections.Generic;
using System.Text;

public class MyALGraph<TKey>
{
    private Dictionary<TKey, Dictionary<TKey, double>> _adjacency;

    public int VertexCount => _adjacency.Count;
    public int EdgeCount
    {
        get
        {
            int total = 0;
            foreach (var kvp in _adjacency)
                total += kvp.Value.Count;
            return total;
        }
    }

    public MyALGraph()
    {
        _adjacency = new Dictionary<TKey, Dictionary<TKey, double>>();
    }

    #region Vertex operations

    public bool AddVertex(TKey v) => _adjacency.TryAdd(v, new Dictionary<TKey, double>());
    //{
    //    if (_adjacency.ContainsKey(v))
    //        return false;

    //    _adjacency[v] = new Dictionary<TKey, double>();
    //    return true;

    //    //_adjacency.TryAdd
    //}

    public bool RemoveVertex(TKey v)
    {
        if (!_adjacency.Remove(v))
            return false;

        foreach (var kvp in _adjacency.Values)
            kvp.Remove(v);

        return true;
    }

    public bool ContainsVertex(TKey v) => _adjacency.ContainsKey(v);

    #endregion

    #region Edge operations

    public bool AddEdge(TKey from, TKey to, float weight)
    {
        if (!_adjacency.ContainsKey(from))
            AddVertex(from);
        if (!_adjacency.ContainsKey(to))
            AddVertex(to);

        var edges = _adjacency[from];
        if (edges.ContainsKey(to))
            return false;

        edges[to] = weight;

        return true;
    }

    public bool RemoveEdge(TKey from, TKey to)
    {
        if (_adjacency.TryGetValue(from, out var edges))
            return edges.Remove(to);

        return false;
    }

    public bool ContainsEdge(TKey from, TKey to) => _adjacency.TryGetValue(from, out var edges) && edges.ContainsKey(to);

    public double GetWeight(TKey from, TKey to)
    {
        if (_adjacency.TryGetValue(from, out var edges) && edges.TryGetValue(to, out var weight))
            return weight;

        throw new ArgumentException($"Edge {from} -> {to} not found.");
    }

    #endregion

    #region Utility

    public IEnumerable<TKey> GetNeighbors(TKey v)
    {
        if (_adjacency.TryGetValue(v, out var edges))
            return edges.Keys;

        return Array.Empty<TKey>();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var (from, edges) in _adjacency)
        {
            foreach (var (to, weight) in edges)
                sb.AppendLine($"{from} -> {to} [w={weight}]");
        }

        return sb.ToString();
    }

    #endregion
}
