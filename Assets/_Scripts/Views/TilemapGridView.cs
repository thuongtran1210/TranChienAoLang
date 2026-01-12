using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridView : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap _baseTilemap;
    [SerializeField] private Tilemap _fogTilemap;
    [SerializeField] private Tilemap _iconTilemap;
    [SerializeField] private Tilemap _highlightTilemap; // Tách layer highlight riêng để dễ quản lý

    [Header("Tile Assets")]
    [SerializeField] private TileBase _waterTile;
    [SerializeField] private TileBase _fogTile;
    [SerializeField] private TileBase _hitTile;
    [SerializeField] private TileBase _missTile;
    [SerializeField] private TileBase _highlightTile; // Tile màu trắng bán trong suốt cho highlight

    private GridSystem _gridSystem;

    // --- 1. SETUP ---
    public void InitializeBoard(int width, int height, GridSystem gridSystem, Owner owner)
    {
        _gridSystem = gridSystem;
        _gridSystem.OnGridStateChanged += HandleGridStateChanged;

        RefreshBoard(width, height, owner);
    }

    private void RefreshBoard(int width, int height, Owner owner)
    {
        _baseTilemap.ClearAllTiles();
        _fogTilemap.ClearAllTiles();
        _iconTilemap.ClearAllTiles();
        _highlightTilemap.ClearAllTiles();

        // Batching setup
        Vector3Int[] positions = new Vector3Int[width * height];
        TileBase[] baseTiles = new TileBase[width * height];
        TileBase[] fogTiles = new TileBase[width * height];

        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                positions[index] = new Vector3Int(x, y, 0);
                baseTiles[index] = _waterTile;

                // Logic Fog of War
                if (owner != Owner.Player)
                {
                    fogTiles[index] = _fogTile;
                }
                index++;
            }
        }

        _baseTilemap.SetTiles(positions, baseTiles);
        _fogTilemap.SetTiles(positions, fogTiles);
    }

    // --- 2. STATE HANDLING ---
    private void HandleGridStateChanged(Vector2Int gridPos, ShotResult result)
    {
        Vector3Int tilePos = new Vector3Int(gridPos.x, gridPos.y, 0);

        // Luôn xóa sương mù khi có tương tác
        _fogTilemap.SetTile(tilePos, null);

        switch (result)
        {
            case ShotResult.Hit:
            case ShotResult.Sunk:
                _iconTilemap.SetTile(tilePos, _hitTile);
                break;
            case ShotResult.Miss:
                _iconTilemap.SetTile(tilePos, _missTile);
                break;
        }
    }

    // --- 3. COORDINATE CONVERSION ---

    public Vector3 GetWorldCenterPosition(Vector2Int gridPos)
    {
        Vector3Int cellPos = new Vector3Int(gridPos.x, gridPos.y, 0);
        return _baseTilemap.GetCellCenterWorld(cellPos);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3Int cellPos = _baseTilemap.WorldToCell(worldPos);
        return new Vector2Int(cellPos.x, cellPos.y);
    }

    // --- 4. HIGHLIGHT SYSTEM ---
    public void HighlightCells(List<Vector2Int> positions, Color color)
    {
        _highlightTilemap.ClearAllTiles();
        _highlightTilemap.color = color; // Tint màu cho toàn bộ tilemap highlight

        Vector3Int[] tilePositions = new Vector3Int[positions.Count];
        TileBase[] tiles = new TileBase[positions.Count];

        for (int i = 0; i < positions.Count; i++)
        {
            tilePositions[i] = new Vector3Int(positions[i].x, positions[i].y, 0);
            tiles[i] = _highlightTile;
        }

        _highlightTilemap.SetTiles(tilePositions, tiles);
    }

    public void ClearHighlights()
    {
        _highlightTilemap.ClearAllTiles();
    }

    private void OnDestroy()
    {
        if (_gridSystem != null)
        {
            _gridSystem.OnGridStateChanged -= HandleGridStateChanged;
        }
    }
}