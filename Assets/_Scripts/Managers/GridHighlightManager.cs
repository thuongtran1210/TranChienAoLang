using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps; // Cần thêm để dùng TileBase

public class GridHighlightManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TilemapGridView _tilemapGridView;
    [SerializeField] private BattleEventChannelSO _battleEvents;

    [Header("Identity")]
    [SerializeField] private Owner _gridOwner; 

    private void OnEnable()
    {
        if (_battleEvents == null) return;

        // Đăng ký Highlight (Preview)
        _battleEvents.OnGridHighlightRequested += HandleHighlightRequested;
        _battleEvents.OnGridHighlightClearRequested += HandleClearRequested;

        // Đăng ký Skill Impact (Execution)
        _battleEvents.OnSkillImpactVisualRequested += HandleImpactVisualRequested;

        // [NEW] Đăng ký Tile Indicator (Icon phát hiện địch)
        _battleEvents.OnTileIndicatorRequested += HandleTileIndicatorRequested;
    }

    private void OnDisable()
    {
        if (_battleEvents == null) return;
        _battleEvents.OnGridHighlightRequested -= HandleHighlightRequested;
        _battleEvents.OnGridHighlightClearRequested -= HandleClearRequested;
        _battleEvents.OnSkillImpactVisualRequested -= HandleImpactVisualRequested;
        _battleEvents.OnTileIndicatorRequested -= HandleTileIndicatorRequested;
    }

    // --- HANDLERS ---

    private void HandleHighlightRequested(Owner target, List<Vector2Int> positions, Color color)
    {
        if (target != _gridOwner) return; // Filter Owner
        _tilemapGridView.HighlightCells(positions, color);
    }

    private void HandleClearRequested()
    {
        // Clear global thì ai cũng clear, hoặc có thể thêm param Owner vào event Clear nếu muốn clear cụ thể
        _tilemapGridView.ClearHighlights();
        _tilemapGridView.ClearIcons(); // Giả sử View có hàm xóa Icon
    }

    private void HandleImpactVisualRequested(Owner target, List<Vector2Int> positions, Color color, float duration)
    {
        if (target != _gridOwner) return;
        StartCoroutine(ImpactRoutine(positions, color, duration));
    }

    private void HandleTileIndicatorRequested(Owner target, List<Vector2Int> positions, TileBase tile, float duration)
    {
        if (target != _gridOwner) return;

        foreach (var pos in positions)
        {
            _tilemapGridView.SetCellIcon(pos, tile);
        }

        // Tự động tắt icon sau duration 
        StartCoroutine(ClearIconsDelay(duration));
    }

    // --- COROUTINES ---

    private IEnumerator ImpactRoutine(List<Vector2Int> positions, Color color, float duration)
    {
        _tilemapGridView.HighlightCells(positions, color);
        yield return new WaitForSeconds(duration);
        _tilemapGridView.ClearHighlights();
    }

    private IEnumerator ClearIconsDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        _tilemapGridView.ClearIcons(); // Cần implement trong View
    }
}