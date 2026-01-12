using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridView : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap _baseTilemap;
    [SerializeField] private Tilemap _fogTilemap;
    [SerializeField] private Tilemap _iconTilemap;

    [Header("Tile Assets (ScriptableObjects)")]
    // Bạn tạo các Tile Asset trong Project (Create -> 2D -> Tiles -> Rectangular Tile)
    [SerializeField] private TileBase _waterTile;
    [SerializeField] private TileBase _fogTile;
    [SerializeField] private TileBase _hitTile;
    [SerializeField] private TileBase _missTile;

    private GridSystem _gridSystem;

    // --- 1. KHỞI TẠO BẢN ĐỒ (BATCHING TỐI ĐA) ---
    public void InitializeBoard(int width, int height, GridSystem gridSystem, Owner owner)
    {
        _gridSystem = gridSystem;

        // Clear map cũ
        _baseTilemap.ClearAllTiles();
        _fogTilemap.ClearAllTiles();
        _iconTilemap.ClearAllTiles();

        // Tối ưu: Sử dụng SetTiles (Số nhiều) thay vì SetTile trong vòng lặp để giảm overhead
        // Tạo mảng vị trí và mảng Tile
        Vector3Int[] positions = new Vector3Int[width * height];
        TileBase[] baseTiles = new TileBase[width * height];
        TileBase[] fogTiles = new TileBase[width * height];

        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                positions[index] = new Vector3Int(x, y, 0);

                // Setup Base Tile (Nước)
                baseTiles[index] = _waterTile;

                // Setup Fog: Nếu không phải map của Player thì che mù
                if (owner != Owner.Player)
                {
                    fogTiles[index] = _fogTile;
                }

                index++;
            }
        }

        // Apply 1 lần duy nhất cho toàn bộ map -> SIÊU NHANH
        _baseTilemap.SetTiles(positions, baseTiles);
        _fogTilemap.SetTiles(positions, fogTiles);

        // Đăng ký sự kiện lắng nghe thay đổi
        _gridSystem.OnGridStateChanged += HandleGridStateChanged;
    }

    // --- 2. CẬP NHẬT TRẠNG THÁI (RUNTIME) ---
    private void HandleGridStateChanged(Vector2Int gridPos, ShotResult result)
    {
        Vector3Int tilePos = new Vector3Int(gridPos.x, gridPos.y, 0);

        // Xóa sương mù ở vị trí đó (bất kể trúng hay trượt đều lộ diện)
        _fogTilemap.SetTile(tilePos, null);

        // Đặt Icon tương ứng
        switch (result)
        {
            case ShotResult.Hit:
            case ShotResult.Sunk: // Sunk cũng dùng hình Hit hoặc hình riêng tùy bạn
                _iconTilemap.SetTile(tilePos, _hitTile);
                break;
            case ShotResult.Miss:
                _iconTilemap.SetTile(tilePos, _missTile);
                break;
        }
    }

    // --- 3. CLEANUP ---
    private void OnDestroy()
    {
        if (_gridSystem != null)
        {
            _gridSystem.OnGridStateChanged -= HandleGridStateChanged;
        }
    }

    // --- 4. HIGHLIGHT SYSTEM (Dùng Tilemap Color hoặc Tile Overlay) ---
    public void HighlightCell(Vector2Int pos, Color color)
    {
        Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);

        // Cách 1: Set Color trực tiếp cho Tile (Cần Tilemap 'Lock Color' tắt)
        _baseTilemap.SetTileFlags(tilePos, TileFlags.None);
        _baseTilemap.SetColor(tilePos, color);
    }

    public void ClearHighlight(Vector2Int pos)
    {
        Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
        _baseTilemap.SetColor(tilePos, Color.white);
    }
}