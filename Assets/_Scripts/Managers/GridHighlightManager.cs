using System.Collections.Generic;
using UnityEngine;

// Áp dụng Clean Code: Class này chỉ lo việc hiển thị Visual
public class GridHighlightManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Owner _gridOwner; 
    [SerializeField] private BattleEventChannelSO _battleEvents; 

    private Dictionary<Vector2Int, GridCellView> _cellViews = new Dictionary<Vector2Int, GridCellView>();
    private List<GridCellView> _activeHighlights = new List<GridCellView>();

    // --- 1. ĐĂNG KÝ SỰ KIỆN ---
    private void OnEnable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnGridHighlightRequested += HandleHighlightRequested;
            _battleEvents.OnGridHighlightClearRequested += ClearHighlight;
        }
    }
    private void OnDisable()
    {
        if (_battleEvents != null)
        {
            _battleEvents.OnGridHighlightRequested -= HandleHighlightRequested;
            _battleEvents.OnGridHighlightClearRequested -= ClearHighlight;
        }
    }
    // --- 2. XỬ LÝ SỰ KIỆN CÓ LỌC  ---
    private void HandleHighlightRequested(Owner target, List<Vector2Int> positions, Color color)
    {
        if (target != _gridOwner) return;

        HighlightPositions(positions, color);
    }

    public void Initialize(Dictionary<Vector2Int, GridCellView> cellViews)
    {
        _cellViews = cellViews;
    }

    /// <summary>
    /// Highlight danh sách các vị trí với màu sắc chỉ định
    /// </summary>
    public void HighlightPositions(List<Vector2Int> positions, Color color)
    {
        ClearHighlight();
        foreach (var pos in positions)
        {
            if (_cellViews.TryGetValue(pos, out GridCellView cellView))
            {
                cellView.SetColor(color);
                _activeHighlights.Add(cellView);
            }
        }
    }

    /// <summary>
    /// Xóa toàn bộ highlight hiện tại 
    /// </summary>
    public void ClearHighlight()
    {
        foreach (var cell in _activeHighlights)
        {
            cell.ResetColor();
        }
        _activeHighlights.Clear();
    }
}