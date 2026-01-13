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
    [SerializeField] private Tilemap _vfxTilemap;

    [Header("Tile Assets")]
    [SerializeField] private TileBase _waterTile;
    [SerializeField] private TileBase _fogTile;
    [SerializeField] private TileBase _hitTile;
    [SerializeField] private TileBase _missTile;
    [SerializeField] private TileBase _highlightTile;


    [Header("Visual Settings")]
    [SerializeField] private float _fadeDuration = 2f;
    [SerializeField] private float _iconPopDuration = 2f;

    private Coroutine _highlightFadeRoutine;
    private GridSystem _gridSystem;
    private Owner _myOwner;

    [Header("Event Listeners")]
    [SerializeField] private BattleEventChannelSO _battleEvents;

    private void OnEnable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnTileIndicatorRequested += HandleTileIndicatorRequested;
            _battleEvents.OnSkillImpactVisualRequested += HandleSkillImpactVisualRequested;
        }
    }

    private void OnDisable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnTileIndicatorRequested -= HandleTileIndicatorRequested;
            _battleEvents.OnSkillImpactVisualRequested -= HandleSkillImpactVisualRequested;
        }
    }
    // --- 1. SETUP ---
    public void InitializeBoard(int width, int height, GridSystem gridSystem, Owner owner)
    {
        _gridSystem = gridSystem;
        _myOwner = owner; // [IMPORTANT] Cache lại Owner khi khởi tạo

        _gridSystem.OnGridStateChanged += HandleGridStateChanged;
        RefreshBoard(width, height, owner);
    }
    // ---  INDICATOR SYSTEM ---

    private void HandleTileIndicatorRequested(Owner target, List<Vector2Int> positions, TileBase tileAsset, float duration)
    {
        Debug.Log($"[TilemapGridView] Indicator Requested. Count: {positions.Count}, Tile: {tileAsset?.name}");
        if (tileAsset == null) Debug.LogError("TileAsset is NULL!");

        StartCoroutine(ShowTemporaryTiles(positions, tileAsset, duration));
    }

    private void HandleSkillImpactVisualRequested(Owner owner, List<Vector2Int> positions, Color color, float duration)
    {
        Debug.Log($"[TilemapGridView] Skill VFX Requested. Count: {positions.Count}, Color: {color}");
        StartCoroutine(ShowVfxRoutine(positions, color, duration));
    }
    private IEnumerator ShowVfxRoutine(List<Vector2Int> positions, Color color, float duration)
    {
        if (_vfxTilemap == null)
        {
            Debug.LogError("TilemapGridView: _vfxTilemap is NULL!");
            yield break;
        }

        // Debug xem Highlight tile có null không
        if (_highlightTile == null) Debug.LogError("TilemapGridView: _highlightTile is NULL inside Inspector!");

        _vfxTilemap.ClearAllTiles();
        _vfxTilemap.color = color;

        Vector3Int[] tilePositions = new Vector3Int[positions.Count];
        TileBase[] tiles = new TileBase[positions.Count];

        for (int i = 0; i < positions.Count; i++)
        {
            tilePositions[i] = new Vector3Int(positions[i].x, positions[i].y, 0);
            tiles[i] = _highlightTile;
        }

        _vfxTilemap.SetTiles(tilePositions, tiles);

        // Kiểm tra xem Tile có thực sự được set không
        if (positions.Count > 0)
        {
            var checkPos = new Vector3Int(positions[0].x, positions[0].y, 0);
            Debug.Log($"Checking tile at {checkPos}: {_vfxTilemap.GetTile(checkPos)}");
        }

        yield return new WaitForSeconds(duration);

        _vfxTilemap.ClearAllTiles();
        _vfxTilemap.color = Color.white;
    }
    private IEnumerator ShowTemporaryHighlightRoutine(List<Vector2Int> positions, Color color, float duration)
    {
        // 1. Lưu lại trạng thái cũ của Highlight Tilemap (nếu cần) hoặc chỉ đơn giản là vẽ đè
        // Ở đây ta dùng _highlightTilemap để hiển thị vùng Scan

        _highlightTilemap.ClearAllTiles();

        Vector3Int[] tilePositions = new Vector3Int[positions.Count];
        TileBase[] tiles = new TileBase[positions.Count];

        for (int i = 0; i < positions.Count; i++)
        {
            tilePositions[i] = new Vector3Int(positions[i].x, positions[i].y, 0);
            tiles[i] = _highlightTile; // Sử dụng tile trắng cơ bản để tint màu
        }

        _highlightTilemap.SetTiles(tilePositions, tiles);
        _highlightTilemap.color = color; // Áp dụng màu từ SkillSO (VD: màu xám trong suốt)

        // 2. Chờ hết thời gian hiệu ứng
        yield return new WaitForSeconds(duration);

        // 3. Clean up (Fade out nhẹ nhàng sẽ đẹp hơn, nhưng ở đây làm đơn giản trước)
        _highlightTilemap.ClearAllTiles();
        _highlightTilemap.color = Color.white; // Reset về mặc định
    }

    private IEnumerator ShowTemporaryTiles(List<Vector2Int> positions, TileBase tileAsset, float duration)
    {
        // 1. Vẽ Tile lên 

        foreach (var pos in positions)
        {
            Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);

            // Vẽ đè lên (hoặc kiểm tra nếu cần)
            _vfxTilemap.SetTile(tilePos, tileAsset);

            // Juice: Pop effect
            StartCoroutine(AnimateTilePop(tilePos, _vfxTilemap));
        }

        // 2. Chờ
        yield return new WaitForSeconds(duration);

        // 3. Xóa Tile (Cleanup)
        foreach (var pos in positions)
        {
            Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);

            if (_vfxTilemap.GetTile(tilePos) == tileAsset)
            {
                _vfxTilemap.SetTile(tilePos, null);
            }
        }
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