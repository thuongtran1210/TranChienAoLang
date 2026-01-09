using System.Collections.Generic;
using UnityEngine;

public abstract class DuckSkillSO : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public int energyCost;
    public Sprite icon;
    public Color skillColor = Color.yellow; // Màu đặc trưng của skill

    // Config cho Validation
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;

    [TextArea] public string description;

    /// <summary>
    /// Hàm mới: Chỉ tính toán danh sách các ô sẽ bị tác động.
    /// Dùng cho cả Preview (Hover) và Execute (Click).
    /// </summary>
    public abstract List<Vector2Int> GetAffectedPositions(Vector2Int pivotPos, IGridSystem targetGrid);

    /// <summary>
    /// Hàm thực thi: Gọi lại GetAffectedPositions để xử lý logic game.
    /// </summary>
    public virtual bool Execute(IGridSystem targetGrid, Vector2Int pivotPos, BattleEventChannelSO eventChannel, Owner targetOwner)
    {
        // 1. Validate
        if (!targetGrid.IsValidPosition(pivotPos))
        {
            eventChannel.RaiseSkillFeedback("Invalid Target!", pivotPos);
            return false;
        }

        // 2. Get Positions
        List<Vector2Int> affectedArea = GetAffectedPositions(pivotPos, targetGrid);

        // 3. Apply Logic (Override ở lớp con nếu cần xử lý phức tạp hơn)
        int hitCount = 0;
        foreach (var pos in affectedArea)
        {
            // Logic xử lý chung (ví dụ: đếm unit, gây damge...)
            // Có thể delegate xuống lớp con thông qua Template Method pattern nếu cần
            var cell = targetGrid.GetCell(pos);
            if (cell != null && cell.OccupiedUnit != null && !cell.IsHit) hitCount++;
        }

        // 4. Feedback
        ApplyVisualFeedback(affectedArea, eventChannel, targetOwner);
        return true;
    }

    protected virtual void ApplyVisualFeedback(List<Vector2Int> area, BattleEventChannelSO channel, Owner target)
    {
        channel.RaiseGridHighlight(target, area, skillColor);
    }
}