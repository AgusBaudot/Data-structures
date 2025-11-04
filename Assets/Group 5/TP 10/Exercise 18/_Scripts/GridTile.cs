using UnityEngine;
using System.Collections.Generic;

public enum GridTileType
{
    Walkable,
    Blocked,
    Spawn,
    Goal,
    Empty
}

public class GridTile
{
    public Vector2Int GridPosition { get; private set; }
    public bool Walkable { get; private set; }
    public bool Buildable { get; private set; }
    public GridTileType Type { get; private set; }

    public Vector2 Center { get; private set; }

    public Vector2Int Next => _next;
    
    private Vector2Int _next;
    private int currentIndex = -1;

    public GridTile(Vector2Int pos, GridTileType type)
    {
        GridPosition = pos;
        SetType(type);
        Center = pos + Vector2.one / 2;
    }

    public void SetType(GridTileType type)
    {
        Type = type;
        Walkable = type == GridTileType.Walkable || type == GridTileType.Goal || type == GridTileType.Spawn;
    }

    public void SetNext(Vector2Int pos) => _next = pos;
}

