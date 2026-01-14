using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridView : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap _baseTilemap;      // Layer nền (Nước)
    [SerializeField] private Tilemap _fogTilemap;       // Layer Fog
    [SerializeField] private Tilemap _iconTilemap;      // Layer Icon (Dấu chấm than, vị trí địch)
    [SerializeField] private Tilemap _highlightTilemap; // Layer Highlight
    [SerializeField] private Tilemap _vfxTilemap;       // Layer VFX (Hit, Miss)

    [Header("Tile Assets")]
    [SerializeField] private TileBase _waterTile;       // Tile nền nước
    [SerializeField] private TileBase _highlightTile;   // Tile dùng để highlight
    [SerializeField] private TileBase _fogTile;         // Tile dùng để hiển thị Fog
    [SerializeField] private TileBase _missTile;        // Tile dùng để hiển thị hiệu ứng Miss
    [SerializeField] private TileBase _hitTile;         // Tile dùng để hiển thị hiệu ứng Hit
    [SerializeField] private TileBase _indicatorTile;      // Tile dùng để hiển thị Icon (ví dụ dấu chấm than)



    [Header("Visual Settings")]
    [SerializeField] private float _iconPopDuration = 0.5f;

    private List<Vector2Int> _currentHighlights = new List<Vector2Int>();

    // --- 1. INITIALIZATION (Khởi tạo hình ảnh bàn cờ) ---

    /// <summary>
    /// Vẽ nền bàn cờ dựa trên kích thước. 
    /// </summary>
    public void InitializeGridVisuals(int width, int height, bool startWithFog)
    {
        _baseTilemap.ClearAllTiles();
        _highlightTilemap.ClearAllTiles();
        _iconTilemap.ClearAllTiles();
        _fogTilemap.ClearAllTiles(); 

        // Loop qua từng ô
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // 1. Luôn vẽ nước nền
                _baseTilemap.SetTile(tilePos, _waterTile);

                // 2. Nếu được yêu cầu có Fog -> Vẽ Fog đè lên
                if (startWithFog)
                {
                    // Check null để tránh lỗi nếu quên gán trong Inspector
                    if (_fogTile != null)
                    {
                        _fogTilemap.SetTile(tilePos, _fogTile);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Hiển thị màu Highlight lên các ô
    /// </summary>
    public void HighlightCells(List<Vector2Int> cells, Color color)
    {
        foreach (var pos in cells)
        {
            Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);

         
            _highlightTilemap.SetTile(tilePos, _highlightTile);

            _highlightTilemap.SetTileFlags(tilePos, TileFlags.None);
            _highlightTilemap.SetColor(tilePos, color);
        }
    }
    public void ShowShotResult(Vector2Int pos, bool isHit)
    {
        Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
        TileBase tileToUse = isHit ? _hitTile : _missTile;

        if (_vfxTilemap != null && tileToUse != null)
        {
            _vfxTilemap.SetTile(tilePos, tileToUse);
            // Có thể thêm animation rung lắc hoặc particle effect ở đây
        }
    }

    /// <summary>
    /// Xóa toàn bộ Highlight
    /// </summary>
    public void ClearHighlights()
    {
        _highlightTilemap.ClearAllTiles();
    }

    /// <summary>
    /// Đặt Icon lên ô (Dùng cho Sonar Skill: Hiển thị dấu ! hoặc vị trí địch)
    /// </summary>
    public void SetCellIcon(Vector2Int pos, TileBase iconTile)
    {
        Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);
        _iconTilemap.SetTile(tilePos, iconTile);

        // Gọi hiệu ứng Pop-up 
        StartCoroutine(AnimateTilePop(tilePos, _iconTilemap));
    }

    /// <summary>
    /// Xóa toàn bộ Icon (Dùng khi hết hiệu ứng Skill)
    /// </summary>
    public void ClearIcons()
    {
        _iconTilemap.ClearAllTiles();
    }

    // --- 2. ANIMATION & VFX ---

    private IEnumerator AnimateTilePop(Vector3Int tilePos, Tilemap targetTilemap)
    {
        targetTilemap.SetTileFlags(tilePos, TileFlags.None);
        targetTilemap.SetTransformMatrix(tilePos, Matrix4x4.identity);

        float timer = 0f;
        while (timer < _iconPopDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / _iconPopDuration;

        
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.5f; // Scale lên 1.5 rồi về 1

            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            targetTilemap.SetTransformMatrix(tilePos, matrix);

            yield return null;
        }


        targetTilemap.SetTransformMatrix(tilePos, Matrix4x4.identity);
    }

    // --- 3. HELPERS ---

    // Helper để lấy TileBase cho highlight (tránh null reference)
    private TileBase GetHighlightTileBase()
    {
        return _baseTilemap.GetTile(new Vector3Int(0, 0, 0)); 
    }

    // --- 4. COORDINATE CONVERSION (Giữ nguyên từ file cũ của bạn) ---

    public Vector3 GetWorldCenterPosition(Vector2Int gridPos)
    {
        Vector3Int cellPos = new Vector3Int(gridPos.x, gridPos.y, 0);
        // CellCenterWorld trả về tâm ô lưới
        return _baseTilemap.GetCellCenterWorld(cellPos);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3Int cellPos = _baseTilemap.WorldToCell(worldPos);
        return new Vector2Int(cellPos.x, cellPos.y);
    }
}