using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

public class GridManager : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase blockedTile;
    [SerializeField] private TileBase walkableTile;
    [SerializeField] private TileBase spawnTile;
    [SerializeField] private TileBase goalTile;
    [SerializeField] private TileBase pathTile;

    [Header("Buttons")] [SerializeField] private PaintButton[] _buttons;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private TextMeshProUGUI _failedText;
    [SerializeField] private GameObject _runner;
    
    public GridTile SpawnTile { get; private set; }
    public GridTile GoalTile { get; private set; }

    private Dictionary<Vector2Int, GridTile> _tiles;
    private TileBase _currentTile;
    private bool _isPainting;
    private List<Vector2Int> _greenPath = new();

    private void Awake()
    {
        BuildGridFromTilemap();
        foreach (PaintButton btn in _buttons)
            btn.Clicked += HandleButtonClick;
		
        _failedText.enabled = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            _isPainting = true;

        if (Input.GetMouseButtonUp(0))
            _isPainting = false;

        if (_isPainting)
        {
            if (_currentTile == null)
                throw new InvalidOperationException("Can't paint without tile selected.");
            
            //Get tile position.
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int tilePos = tilemap.WorldToCell(mousePos);
            Vector2Int tilePos2 = new Vector2Int(tilePos.x, tilePos.y);

            //If user clicked out of bounds, return.
            if (!tilemap.cellBounds.Contains(tilePos))
                throw new NullReferenceException("Can't paint outside of map.");
            
            //Decide new logical type from selected tile.
            GridTileType newType = _currentTile switch
            {
                _ when _currentTile == walkableTile => GridTileType.Walkable,
                _ when _currentTile == blockedTile => GridTileType.Blocked,
                _ when _currentTile == spawnTile => GridTileType.Spawn,
                _ when _currentTile == goalTile => GridTileType.Goal,
                _ => GridTileType.Empty // A default value in case none match
            };
            
            //If painting a unique tile (Spawn or Goal), replace the previous one (if different).
            if (newType == GridTileType.Spawn)
            {
                //If there's an existing spawn and it's a different position, turn it back to walkable.
                if (SpawnTile != null && SpawnTile.GridPosition != tilePos2)
                {
                    //Visual
                    tilemap.SetTile((Vector3Int)SpawnTile.GridPosition, walkableTile);
                    //Logical
                    if (_tiles.TryGetValue(SpawnTile.GridPosition, out GridTile oldSpawn))
                    {
                        oldSpawn.SetType(GridTileType.Walkable);
                        _tiles[SpawnTile.GridPosition] = oldSpawn;
                    }
                }
            }
            
            else if (newType == GridTileType.Goal)
            {
                if (GoalTile != null && GoalTile.GridPosition != tilePos2)
                {
                    tilemap.SetTile((Vector3Int)GoalTile.GridPosition, walkableTile);
                    if (_tiles.TryGetValue(GoalTile.GridPosition, out GridTile oldGoal))
                    {
                        oldGoal.SetType(GridTileType.Walkable);
                        _tiles[GoalTile.GridPosition] = oldGoal;
                    }
                }
            }
            
            //If painting non-unique onto an existing unique tile, clear the unique reference.
            if (newType == GridTileType.Walkable || newType == GridTileType.Blocked)
            {
                if (SpawnTile != null && SpawnTile.GridPosition == tilePos2) SpawnTile = null;
                if (GoalTile != null && GoalTile.GridPosition == tilePos2) GoalTile = null;
            }
            
            //If painting unique onto an existing unique, clear the old reference.
            if (newType == GridTileType.Spawn && GoalTile != null && GoalTile.GridPosition == tilePos2)
                GoalTile = null;

            if (newType == GridTileType.Goal && SpawnTile != null && SpawnTile.GridPosition == tilePos2)
                SpawnTile = null;
            
            //Apply visual & logical changes for the clicked cell.
            SetTileAndGrid(tilePos2, _currentTile, newType);
            
            //If this was unique tile, update the reference.
            if (newType == GridTileType.Spawn)
                SpawnTile = _tiles[tilePos2];
            
            else if (newType == GridTileType.Goal)
                GoalTile = _tiles[tilePos2];
        }
    }

    private void SetTileAndGrid(Vector2Int gridPos, TileBase tileBase, GridTileType type)
    {
        //Visual
        tilemap.SetTile((Vector3Int)gridPos, tileBase);
        
        //Logical: either update existing GridTile or create a new one.
        if (_tiles.TryGetValue(gridPos, out GridTile existing))
        {
            existing.SetType(type);
            _tiles[gridPos] = existing;
        }
        else
        {
            var newTile = new GridTile(gridPos, type);
            _tiles[gridPos] = newTile;
        }
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
                _ when tileBase == walkableTile => GridTileType.Walkable,
                _ when tileBase == blockedTile => GridTileType.Blocked,
                _ when tileBase == spawnTile => GridTileType.Spawn,
                _ when tileBase == goalTile => GridTileType.Goal,
                _ => GridTileType.Empty // A default value in case none match
            };

            var tile = new GridTile(gridPos, type);
            _tiles[gridPos] = tile;

            if (type == GridTileType.Spawn)
                SpawnTile = tile;

            if (type == GridTileType.Goal)
                GoalTile = tile;
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

    //Called by UI
    public void CalculatePath()
    {
        if (GoalTile == null || SpawnTile == null)
        {
            Debug.LogError("GridManager: No goal or spawn tile found!");
            _failedText.enabled = true;
            return;
        }

        _failedText.enabled = false;
        PathAlgorithm algo = (PathAlgorithm)_dropdown.value;
        var result = Pathfinder.CalculatePath(_tiles, SpawnTile.GridPosition, GoalTile.GridPosition, algo);
		if (!result.Found) 
			_failedText.enabled = true;
        Debug.Log(result);
        StartCoroutine(PaintGreen(result));
    }

    //Called by UI
    public void RunAllAlgorithms()
    {
        var result = Pathfinder.RunAllAlgorithms(_tiles, SpawnTile.GridPosition, GoalTile.GridPosition);
        foreach (var path in result)
        {
            Debug.Log(path);
        }
    }

    private IEnumerator PaintGreen(PathResult result)
    {
        //Reset previous gren path before painting current.
        PaintWhite();
        
        //Cache this path for cleanup later.
        _greenPath = result.Path;
        _runner.SetActive(true);
        
        //Paint green visually (Keep them the same logically for further path calculations).
        foreach (Vector2Int tilePos in _greenPath)
        {
            if (_tiles[tilePos].Type != GridTileType.Walkable) continue;
            //Update visually.
            tilemap.SetTile((Vector3Int)tilePos, pathTile);
            _runner.transform.position = tilePos + Vector2.one / 2;
            yield return new WaitForSeconds(1);
        }

        _runner.SetActive(false);
    }

    public void PaintWhite()
    {
        if (_greenPath.Count == 0) return;
        foreach (Vector2Int tilePos in _greenPath)
        {
            if (_tiles[tilePos].Type != GridTileType.Walkable) continue;
            //Update visually.
            tilemap.SetTile((Vector3Int)tilePos, walkableTile);
            //Update logically.
            _tiles[tilePos].SetType(GridTileType.Walkable);
        }
        _greenPath.Clear();
    }

    private void HandleButtonClick(PaintButton button)
    {
        _currentTile = button._tile;
        if (!button.Pressed) _currentTile = null;
    }

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