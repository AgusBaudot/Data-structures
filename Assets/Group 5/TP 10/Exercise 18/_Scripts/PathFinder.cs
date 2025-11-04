using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public enum PathAlgorithm
{
    BFS,
    DFS,
    Dijkstra,
    AStar
}

public class PathResult
{
    public PathAlgorithm Algorithm { get; set; }
    public bool Found { get; set; }
    public List<Vector2Int> Path { get; set; } = new List<Vector2Int>();
    public List<Vector2Int> Visited { get; set; } = new List<Vector2Int>();
    public int NodesExpanded { get; set; } = 0;
    public long LookupChecks { get; set; } = 0;
    public long ElapsedMilliseconds { get; set; } = 0;
    public double TotalCost { get; set; } = 0; // path cost (relevant to Dijkstra/A*)
    public override string ToString()
    {
        return $"[{Algorithm}] Found: {Found}, Path length: {Path?.Count ?? 0}, Cost: {TotalCost:F2}, Visited: {Visited.Count}, NodesExpanded: {NodesExpanded}, LookupChecks: {LookupChecks}, Time(ms): {ElapsedMilliseconds}";
    }
}

/// <summary>
/// Generic grid pathfinder: BFS, DFS, Dijkstra and A* (with optional cost function).
/// </summary>
public static class Pathfinder
{
    /// <summary>
    /// Single-call factory.
    /// </summary>
    public static PathResult CalculatePath(Dictionary<Vector2Int, GridTile> grid, Vector2Int start, Vector2Int end, PathAlgorithm algo, bool allowDiagonals = false, Func<Vector2Int, double> costFunc = null)
    {
        // Build hashset of walkable positions for O(1) checks
        var walkables = new HashSet<Vector2Int>();
        foreach (var tile in grid.Values)
            if (tile != null && tile.Walkable)
                walkables.Add(tile.GridPosition);

        // Quick invalid cases
        if (!walkables.Contains(start) || !walkables.Contains(end))
        {
            return new PathResult
            {
                Algorithm = algo,
                Found = false,
                ElapsedMilliseconds = 0,
                TotalCost = double.PositiveInfinity
            };
        }

        switch (algo)
        {
            case PathAlgorithm.BFS:
                return BFS(grid, walkables, start, end, allowDiagonals);
            case PathAlgorithm.DFS:
                return DFS(grid, walkables, start, end, allowDiagonals);
            case PathAlgorithm.Dijkstra:
                return Dijkstra(grid, walkables, start, end, allowDiagonals, costFunc ?? (p => 1.0));
            case PathAlgorithm.AStar:
                return AStar(grid, walkables, start, end, allowDiagonals, costFunc ?? (p => 1.0));
            default:
                throw new ArgumentOutOfRangeException(nameof(algo));
        }
    }

    /// <summary>
    /// Run all algorithms (BFS, DFS, Dijkstra, A*) and return results list.
    /// </summary>
    public static List<PathResult> RunAllAlgorithms(Dictionary<Vector2Int, GridTile> grid, Vector2Int start, Vector2Int end, bool allowDiagonals = false, Func<Vector2Int, double> costFunc = null)
    {
        var algos = new[] { PathAlgorithm.BFS, PathAlgorithm.DFS, PathAlgorithm.Dijkstra, PathAlgorithm.AStar };
        var results = new List<PathResult>();
        foreach (var a in algos)
        {
            results.Add(CalculatePath(grid, start, end, a, allowDiagonals, costFunc));
        }
        return results;
    }

    #region BFS
    public static PathResult BFS(Dictionary<Vector2Int, GridTile> grid, HashSet<Vector2Int> walkables, Vector2Int start, Vector2Int end, bool allowDiagonals = false)
    {
        var sw = Stopwatch.StartNew();
        var result = new PathResult { Algorithm = PathAlgorithm.BFS };

        if (start == end)
        {
            result.Found = true;
            result.Path.Add(start);
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            result.TotalCost = 0;
            return result;
        }

        var dirs = GetDirections(allowDiagonals);
        var q = new Queue<Vector2Int>();
        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        q.Enqueue(start);
        visited.Add(start);
        result.LookupChecks++;
        result.Visited.Add(start);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            result.NodesExpanded++;

            foreach (var d in dirs)
            {
                var nb = cur + d;
                result.LookupChecks++;
                if (!walkables.Contains(nb)) continue;

                if (visited.Contains(nb)) continue;
                visited.Add(nb);
                parent[nb] = cur;
                result.Visited.Add(nb);
                q.Enqueue(nb);

                if (nb == end)
                {
                    result.Found = true;
                    result.Path = ReconstructPath(parent, start, end);
                    result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    result.TotalCost = result.Path.Count > 0 ? (result.Path.Count - 1) : 0; // unit cost
                    return result;
                }
            }
        }

        result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        result.Found = false;
        result.TotalCost = double.PositiveInfinity;
        return result;
    }
    #endregion

    #region DFS (iterative)
    public static PathResult DFS(Dictionary<Vector2Int, GridTile> grid, HashSet<Vector2Int> walkables, Vector2Int start, Vector2Int end, bool allowDiagonals = false)
    {
        var sw = Stopwatch.StartNew();
        var result = new PathResult { Algorithm = PathAlgorithm.DFS };

        if (start == end)
        {
            result.Found = true;
            result.Path.Add(start);
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            result.TotalCost = 0;
            return result;
        }

        var dirs = GetDirections(allowDiagonals);
        var stack = new Stack<Vector2Int>();
        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        stack.Push(start);
        visited.Add(start);
        result.LookupChecks++;
        result.Visited.Add(start);

        while (stack.Count > 0)
        {
            var cur = stack.Pop();
            result.NodesExpanded++;

            for (int i = dirs.Length - 1; i >= 0; i--)
            {
                var nb = cur + dirs[i];
                result.LookupChecks++;
                if (!walkables.Contains(nb)) continue;
                if (visited.Contains(nb)) continue;

                visited.Add(nb);
                parent[nb] = cur;
                result.Visited.Add(nb);
                stack.Push(nb);

                if (nb == end)
                {
                    result.Found = true;
                    result.Path = ReconstructPath(parent, start, end);
                    result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                    result.TotalCost = result.Path.Count > 0 ? (result.Path.Count - 1) : 0;
                    return result;
                }
            }
        }

        result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        result.Found = false;
        result.TotalCost = double.PositiveInfinity;
        return result;
    }
    #endregion

    #region Dijkstra
    public static PathResult Dijkstra(Dictionary<Vector2Int, GridTile> grid, HashSet<Vector2Int> walkables, Vector2Int start, Vector2Int end, bool allowDiagonals = false, Func<Vector2Int, double> costFunc = null)
    {
        var sw = Stopwatch.StartNew();
        var result = new PathResult { Algorithm = PathAlgorithm.Dijkstra };

        if (start == end)
        {
            result.Found = true;
            result.Path.Add(start);
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            result.TotalCost = 0;
            return result;
        }

        costFunc ??= (pos => 1.0);

        var dirs = GetDirections(allowDiagonals);

        var dist = new Dictionary<Vector2Int, double>();
        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        var pq = new SimplePriorityQueue<Vector2Int, double>();

        dist[start] = 0;
        pq.Enqueue(start, 0);

        while (pq.Count > 0)
        {
            var (cur, curDist) = pq.DequeueWithPriority();
            if (visited.Contains(cur)) continue;
            visited.Add(cur);
            result.NodesExpanded++;
            result.Visited.Add(cur);

            if (cur == end)
            {
                result.Found = true;
                result.TotalCost = curDist;
                result.Path = ReconstructPath(parent, start, end);
                result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                return result;
            }

            foreach (var d in dirs)
            {
                var nb = cur + d;
                result.LookupChecks++;
                if (!walkables.Contains(nb)) continue;

                double moveCost = costFunc(nb);
                double candidate = curDist + moveCost;

                if (!dist.TryGetValue(nb, out var known) || candidate < known)
                {
                    dist[nb] = candidate;
                    parent[nb] = cur;
                    pq.Enqueue(nb, candidate);
                }
            }
        }

        result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        result.Found = false;
        result.TotalCost = double.PositiveInfinity;
        return result;
    }
    #endregion

    #region A*
    public static PathResult AStar(Dictionary<Vector2Int, GridTile> grid, HashSet<Vector2Int> walkables, Vector2Int start, Vector2Int end, bool allowDiagonals = false, Func<Vector2Int, double> costFunc = null)
    {
        var sw = Stopwatch.StartNew();
        var result = new PathResult { Algorithm = PathAlgorithm.AStar };

        if (start == end)
        {
            result.Found = true;
            result.Path.Add(start);
            result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
            result.TotalCost = 0;
            return result;
        }

        costFunc ??= (pos => 1.0);

        var dirs = GetDirections(allowDiagonals);

        // compute conservative min cost over walkables to scale heuristic (admissible)
        double minCost = 1.0;
        try
        {
            var candidates = walkables.Select(p => costFunc(p));
            if (candidates.Any())
                minCost = Mathf.Max(0.000001f, (float)candidates.Min());
        }
        catch
        {
            minCost = 1.0;
        }

        // g-score (cost from start), f-score = g + h
        var g = new Dictionary<Vector2Int, double>();
        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        var pq = new SimplePriorityQueue<Vector2Int, double>();

        g[start] = 0;
        double hstart = Heuristic(start, end, allowDiagonals) * minCost;
        pq.Enqueue(start, g[start] + hstart);

        while (pq.Count > 0)
        {
            var (cur, _) = pq.DequeueWithPriority();
            if (visited.Contains(cur)) continue;
            visited.Add(cur);
            result.NodesExpanded++;
            result.Visited.Add(cur);

            if (cur == end)
            {
                result.Found = true;
                result.Path = ReconstructPath(parent, start, end);
                result.TotalCost = g.ContainsKey(end) ? g[end] : 0;
                result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                return result;
            }

            double curG = g[cur];

            foreach (var d in dirs)
            {
                var nb = cur + d;
                result.LookupChecks++;
                if (!walkables.Contains(nb)) continue;

                double moveCost = costFunc(nb);
                double tentativeG = curG + moveCost;

                if (!g.TryGetValue(nb, out var knownG) || tentativeG < knownG)
                {
                    g[nb] = tentativeG;
                    parent[nb] = cur;
                    double f = tentativeG + Heuristic(nb, end, allowDiagonals) * minCost;
                    pq.Enqueue(nb, f);
                }
            }
        }

        result.ElapsedMilliseconds = sw.ElapsedMilliseconds;
        result.Found = false;
        result.TotalCost = double.PositiveInfinity;
        return result;
    }

    // admissible heuristic depending on connectivity
    private static double Heuristic(Vector2Int a, Vector2Int b, bool diag)
    {
        int dx = Math.Abs(a.x - b.x);
        int dy = Math.Abs(a.y - b.y);
        if (diag)
        {
            // Chebyshev distance for 8-neighbour grid (admissible if cost per step >= 1)
            return Math.Max(dx, dy);
        }
        // Manhattan for 4-neighbor
        return dx + dy;
    }
    #endregion

    #region Helpers

    private static Vector2Int[] GetDirections(bool diag)
    {
        if (!diag)
        {
            return new[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0)
            };
        }
        else
        {
            return new[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 1),
                new Vector2Int(1, 0),
                new Vector2Int(1, -1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(-1, 1)
            };
        }
    }

    private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> parent, Vector2Int start, Vector2Int end)
    {
        var path = new List<Vector2Int>();
        var cur = end;
        path.Add(cur);
        while (!cur.Equals(start))
        {
            if (!parent.TryGetValue(cur, out cur))
            {
                return new List<Vector2Int>();
            }
            path.Add(cur);
        }
        path.Reverse();
        return path;
    }

    #endregion

    #region Simple Priority Queue (min-heap)
    private class SimplePriorityQueue<TItem, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly List<(TItem item, TPriority priority)> _data = new();
        public int Count => _data.Count;

        public void Enqueue(TItem item, TPriority priority)
        {
            _data.Add((item, priority));
            HeapifyUp(_data.Count - 1);
        }

        public (TItem item, TPriority priority) DequeueWithPriority()
        {
            if (_data.Count == 0) throw new InvalidOperationException("Queue empty");
            var root = _data[0];
            var last = _data[_data.Count - 1];
            _data.RemoveAt(_data.Count - 1);
            if (_data.Count > 0)
            {
                _data[0] = last;
                HeapifyDown(0);
            }
            return root;
        }

        private void HeapifyUp(int i)
        {
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (_data[i].priority.CompareTo(_data[p].priority) < 0)
                {
                    Swap(i, p);
                    i = p;
                }
                else break;
            }
        }

        private void HeapifyDown(int i)
        {
            int n = _data.Count;
            while (true)
            {
                int l = 2 * i + 1;
                int r = 2 * i + 2;
                int smallest = i;
                if (l < n && _data[l].priority.CompareTo(_data[smallest].priority) < 0) smallest = l;
                if (r < n && _data[r].priority.CompareTo(_data[smallest].priority) < 0) smallest = r;
                if (smallest != i)
                {
                    Swap(i, smallest);
                    i = smallest;
                }
                else break;
            }
        }

        private void Swap(int a, int b)
        {
            var tmp = _data[a];
            _data[a] = _data[b];
            _data[b] = tmp;
        }
    }
    #endregion
}
