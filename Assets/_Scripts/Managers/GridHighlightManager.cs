using System.Collections.Generic;
using UnityEngine;

// Áp dụng Clean Code: Class này chỉ lo việc hiển thị Visual
public class GridHighlightManager : MonoBehaviour
{
    // Giả sử bạn có class GridCellView để quản lý view của từng ô
    // Dictionary để tra cứu nhanh từ Tọa độ (Vector2Int) ra View
    private Dictionary<Vector2Int, GridCellView> _cellViews = new Dictionary<Vector2Int, GridCellView>();

    // Cache lại các ô đang được highlight để clear cho nhanh (Optimization)
    private List<GridCellView> _activeHighlights = new List<GridCellView>();

    /// <summary>
    /// Hàm khởi tạo, cần được gọi từ GridSystem hoặc GameManager khi tạo map xong.
    /// </summary>
    public void Initialize(Dictionary<Vector2Int, GridCellView> cellViews)
    {
        _cellViews = cellViews;
    }

    /// <summary>
    /// Highlight danh sách các vị trí với màu sắc chỉ định
    /// </summary>
    public void HighlightPositions(List<Vector2Int> positions, Color color)
    {
        // 1. Clear cũ trước khi vẽ mới để tránh chồng chéo
        ClearHighlight();

        foreach (var pos in positions)
        {
            // Sử dụng TryGetValue để an toàn (tránh lỗi KeyNotFound)
            if (_cellViews.TryGetValue(pos, out GridCellView cellView))
            {
                cellView.SetColor(color); // Giả định GridCellView có hàm này
                _activeHighlights.Add(cellView);
            }
        }
    }

    /// <summary>
    /// Xóa toàn bộ highlight hiện tại (Đây là hàm bạn đang thiếu)
    /// </summary>
    public void ClearHighlight()
    {
        // Sử dụng Null-conditional operator (?.) của C# hiện đại nếu cần
        foreach (var cell in _activeHighlights)
        {
            cell.ResetColor(); // Giả định GridCellView có hàm Reset về mặc định
        }

        _activeHighlights.Clear();
    }
}