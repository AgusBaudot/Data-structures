using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase[] blockedTile;
    [SerializeField] private TileBase[] walkableTile;
    [SerializeField] private TileBase[] spawnTile;
    [SerializeField] private TileBase[] goalTile;

    public GridTile SpawnTile { get; private set; }
    public GridTile GoalTile { get; private set; }

    private Dictionary<Vector2Int, GridTile> _tiles;

    private void Awake()
    {
        Instance = this;
        BuildGridFromTilemap();
        CalculatePath();
    }

    private void BuildGridFromTilemap()
    {
        _tiles = new Dictionary<Vector2Int, GridTile>();

        BoundsInt bounds = tilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            Vector3Int cellPos = new Vector3Int(pos.x, pos.y, 0);
            TileBase tileBase = tilemap.GetTile(cellPos);
            if (tileBase == null) continue;

            Vector2Int gridPos = new Vector2Int(pos.x, pos.y);


            GridTileType type = tileBase switch
            {
                _ when walkableTile.Contains(tileBase) => GridTileType.Walkable,
                _ when blockedTile.Contains(tileBase) => GridTileType.Blocked,
                _ when spawnTile.Contains(tileBase) => GridTileType.Spawn,
                _ when goalTile.Contains(tileBase) => GridTileType.Goal,
                _ => GridTileType.Empty // A default value in case none match
            };

            var tile = new GridTile(gridPos, type);
            _tiles[gridPos] = tile;

            if (type == GridTileType.Spawn) SpawnTile = tile;
            if (type == GridTileType.Goal) GoalTile = tile;
        }
    }

    public GridTile GetTile(Vector2Int gridPos)
    {
        _tiles.TryGetValue(gridPos, out GridTile tile);
        return tile;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return tilemap.CellToWorld(new Vector3Int(gridPos.x, gridPos.y, 0))
               + tilemap.cellSize / 2;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return (Vector2Int)tilemap.WorldToCell(worldPos);
    }

    private void CalculatePath()
    {
        if (GoalTile == null || SpawnTile == null)
        {
            Debug.LogError("GridManager: No goal or spawn tile found!");
            return;
        }

        var result = Pathfinder.RunAllAlgorithms(_tiles, SpawnTile.GridPosition, GoalTile.GridPosition);
        foreach (var path in result)
        {
            Debug.Log(path);
        }
        // foreach (var kvp in mappedTiles.mappedPos)
        // {
        //     GridTile tile = GetTile(kvp.Key);
        //     foreach (var neighbor in kvp.Value)
        //     {
        //         tile.SetNext(neighbor);
        //     }
        // }
    }

    // private void SearchForSpawn(Vector2Int position)
    // {
    //     //Candidate is one of 4 adjacent tiles that is spawn tile.
    //     var candidate = GetTile(position + Vector2Int.up);
    //     if (candidate?.Type == GridTileType.Spawn)
    //         candidate.SetNext(position);
    //
    //     candidate = GetTile(position + Vector2Int.right);
    //     if (candidate?.Type == GridTileType.Spawn)
    //         candidate.SetNext(position);
    //
    //     candidate = GetTile(position + Vector2Int.down);
    //     if (candidate?.Type == GridTileType.Spawn)
    //         candidate.SetNext(position);
    //
    //     candidate = GetTile(position + Vector2Int.left);
    //     if (candidate?.Type == GridTileType.Spawn)
    //         candidate.SetNext(position);
    // }

    private void OnDrawGizmosSelected()
    {
        if (_tiles == null || _tiles.Count == 0) return;

        var walkables = _tiles.Values.Where(a => a.Walkable).ToArray();
        foreach (GridTile tile in walkables)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(GridToWorld(tile.GridPosition), GridToWorld(tile.Next));
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(GridToWorld(tile.Next), 0.1f);
        }
    }
}