using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGridView : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap _baseTilemap;
    [SerializeField] private Tilemap _fogTilemap;
    [SerializeField] private Tilemap _iconTilemap;
    [SerializeField] private Tilemap _highlightTilemap; 

    [Header("Tile Assets")]
    [SerializeField] private TileBase _waterTile;
    [SerializeField] private TileBase _fogTile;
    [SerializeField] private TileBase _hitTile;
    [SerializeField] private TileBase _missTile;
    [SerializeField] private TileBase _highlightTile; 

    [Header("Visual Settings")]
    [SerializeField] private float _fadeDuration = 0.2f;
    [SerializeField] private float _iconPopDuration = 0.4f;

    private Coroutine _highlightFadeRoutine;
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

        // A. Xử lý sương mù (Có thể làm hiệu ứng tan biến nếu muốn phức tạp hơn)
        if (_fogTilemap.HasTile(tilePos))
        {
            _fogTilemap.SetTile(tilePos, null);
            // TODO: Ở level Advanced, ta sẽ spawn một Particle Effect "Mây tan" tại đây
        }

        // B. Xử lý Icon Hit/Miss với hiệu ứng Pop-up
        TileBase iconTile = null;
        switch (result)
        {
            case ShotResult.Hit:
            case ShotResult.Sunk:
                iconTile = _hitTile;
                break;
            case ShotResult.Miss:
                iconTile = _missTile;
                break;
        }

        if (iconTile != null)
        {
            _iconTilemap.SetTile(tilePos, iconTile);

            // [JUICE] Gọi animation cho tile vừa đặt
            StartCoroutine(AnimateTilePop(tilePos, _iconTilemap));
        }
    }
    private IEnumerator AnimateTilePop(Vector3Int tilePos, Tilemap targetTilemap)
    {
        targetTilemap.SetTileFlags(tilePos, TileFlags.None);

        float timer = 0f;
        while (timer < _iconPopDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / _iconPopDuration;

            // Animation Curve đơn giản: Tăng lên 1.5 lần rồi về 1
            float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.5f;

            Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            targetTilemap.SetTransformMatrix(tilePos, matrix);

            yield return null;
        }

        // Reset về mặc định
        targetTilemap.SetTransformMatrix(tilePos, Matrix4x4.identity);
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
    public void HighlightCells(List<Vector2Int> positions, Color targetColor)
    {
        // 1. Dừng hiệu ứng cũ nếu đang chạy
        if (_highlightFadeRoutine != null) StopCoroutine(_highlightFadeRoutine);

        // 2. Setup Tiles
        _highlightTilemap.ClearAllTiles();

        Vector3Int[] tilePositions = new Vector3Int[positions.Count];
        TileBase[] tiles = new TileBase[positions.Count];

        for (int i = 0; i < positions.Count; i++)
        {
            tilePositions[i] = new Vector3Int(positions[i].x, positions[i].y, 0);
            tiles[i] = _highlightTile;
        }
        _highlightTilemap.SetTiles(tilePositions, tiles);

        // 3. Bắt đầu Fade In màu mới
        _highlightFadeRoutine = StartCoroutine(FadeHighlightColor(targetColor));
    }
    private IEnumerator FadeHighlightColor(Color targetColor, bool clearAfter = false)
    {
        Color startColor = _highlightTilemap.color;
        float timer = 0f;

        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            // Lerp màu từ trạng thái hiện tại sang màu đích
            _highlightTilemap.color = Color.Lerp(startColor, targetColor, timer / _fadeDuration);
            yield return null;
        }

        _highlightTilemap.color = targetColor;

        if (clearAfter)
        {
            _highlightTilemap.ClearAllTiles();
            // Reset alpha về 1 (hoặc màu trắng) để lần sau dùng
            _highlightTilemap.color = Color.white;
        }
    }
    public void ClearHighlights()
    {
        if (_highlightFadeRoutine != null) StopCoroutine(_highlightFadeRoutine);
        _highlightFadeRoutine = StartCoroutine(FadeHighlightColor(Color.clear, true));
    }

    private void OnDestroy()
    {
        if (_gridSystem != null)
        {
            _gridSystem.OnGridStateChanged -= HandleGridStateChanged;
        }
    }
}