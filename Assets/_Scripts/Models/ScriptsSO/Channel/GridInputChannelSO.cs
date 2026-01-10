using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Duck Battle/Events/Grid Input Channel")]
public class GridInputChannelSO : ScriptableObject
{
    // Sự kiện khi Click vào một ô
    // Param 1: Tọa độ Grid
    // Param 2: Grid Owner (Player/Enemy)
    public UnityAction<Vector2Int, Owner> OnGridCellClicked;

    // Sự kiện khi Hover chuột qua một ô (Dùng cho UI tooltip hoặc highlight nhẹ)
    // Param 1: Tọa độ Grid (Nếu (-999, -999) là không hover gì cả)
    // Param 2: Grid Logic (để lấy thêm thông tin nếu cần)
    public UnityAction<Vector2Int, IGridLogic> OnGridCellHovered;

    // Sự kiện chuột di chuyển trong world (Dùng cho VFX con trỏ, v.v.)
    public UnityAction<Vector3> OnPointerPositionChanged;

    // Sự kiện chuột phải (Hủy/Quay lại)
    public UnityAction OnRolateClick;

    // --- RAISERS ---
    public void RaiseGridCellClicked(Vector2Int gridPos, Owner owner)
        => OnGridCellClicked?.Invoke(gridPos, owner);

    public void RaiseGridCellHovered(Vector2Int gridPos, IGridLogic grid)
        => OnGridCellHovered?.Invoke(gridPos, grid);

    public void RaisePointerPositionChanged(Vector3 worldPos)
        => OnPointerPositionChanged?.Invoke(worldPos);

    public void RaiseRightClick()
        => OnRolateClick?.Invoke();
}