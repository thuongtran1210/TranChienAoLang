using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// [Refactor] Đổi tên class để phản ánh đúng vai trò quản lý State
public class GridHighlightManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TilemapGridView _tilemapGridView;
    [SerializeField] private BattleEventChannelSO _battleEvents;

    [Header("Identity")]
    [SerializeField] private Owner _gridOwner;

    // [New] State Tracking: Lưu trữ trạng thái Preview hiện tại
    private List<Vector2Int> _activePreviewPositions = new List<Vector2Int>();
    private Color _activePreviewColor;
    private bool _hasActivePreview = false;

    private void OnEnable()
    {
        if (_battleEvents == null) return;
        _battleEvents.OnGridHighlightRequested += HandlePreviewRequested; // Đổi tên handler cho rõ nghĩa
        _battleEvents.OnGridHighlightClearRequested += HandlePreviewClearRequested;
        _battleEvents.OnSkillImpactVisualRequested += HandleImpactVisualRequested;
        _battleEvents.OnTileIndicatorRequested += HandleTileIndicatorRequested;
    }

    private void OnDisable()
    {
        if (_battleEvents == null) return;
        _battleEvents.OnGridHighlightRequested -= HandlePreviewRequested;
        _battleEvents.OnGridHighlightClearRequested -= HandlePreviewClearRequested;
        _battleEvents.OnSkillImpactVisualRequested -= HandleImpactVisualRequested;
        _battleEvents.OnTileIndicatorRequested -= HandleTileIndicatorRequested;
    }

    // --- PREVIEW HANDLERS (Persistent State) ---

    private void HandlePreviewRequested(Owner target, List<Vector2Int> positions, Color color)
    {
        if (target != _gridOwner) return;

        // [Logic] Cập nhật State
        _activePreviewPositions = new List<Vector2Int>(positions); // Copy list để an toàn
        _activePreviewColor = color;
        _hasActivePreview = true;

        // [Visual] Vẽ ngay lập tức
        RenderPreview();
    }

    private void HandlePreviewClearRequested()
    {
        // [Logic] Reset State
        _activePreviewPositions.Clear();
        _hasActivePreview = false;

        // [Visual] Xóa Visual
        _tilemapGridView.ClearHighlights();
        // Clear Icons nếu cần, nhưng cẩn thận không clear nhầm icon của Skill Indicator (như Sonar)
    }

    private void RenderPreview()
    {
        if (_hasActivePreview)
        {
            _tilemapGridView.HighlightCells(_activePreviewPositions, _activePreviewColor);
        }
    }

    // --- IMPACT HANDLERS (Transient State) ---

    private void HandleImpactVisualRequested(Owner target, List<Vector2Int> positions, Color color, float duration)
    {
        if (target != _gridOwner) return;
        StartCoroutine(ImpactRoutine(positions, color, duration));
    }

    private IEnumerator ImpactRoutine(List<Vector2Int> positions, Color color, float duration)
    {
        // 1. Ghi đè Highlight bằng màu FX (Skill Execution)
        _tilemapGridView.HighlightCells(positions, color);

        // 2. Chờ hiệu ứng
        yield return new WaitForSeconds(duration);

        // 3. [CRITICAL FIX] Thay vì Clear toàn bộ, ta kiểm tra xem có cần Restore Preview không
        _tilemapGridView.ClearHighlights();

        // 4. Khôi phục lại Preview nếu người chơi vẫn đang trỏ chuột vào đâu đó
        if (_hasActivePreview)
        {
            RenderPreview();
        }
    }

    // --- INDICATOR HANDLERS ---

    private void HandleTileIndicatorRequested(Owner target, List<Vector2Int> positions, TileBase tile, float duration)
    {
        if (target != _gridOwner) return;

        foreach (var pos in positions)
        {
            _tilemapGridView.SetCellIcon(pos, tile);
        }

        // Icon hoạt động độc lập với Highlight màu, nên có thể clear độc lập
        StartCoroutine(ClearIconsDelay(duration));
    }

    private IEnumerator ClearIconsDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        _tilemapGridView.ClearIcons();
    }
}