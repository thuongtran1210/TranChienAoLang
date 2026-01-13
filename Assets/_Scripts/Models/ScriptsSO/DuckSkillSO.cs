using System.Collections.Generic;
using UnityEngine;
public enum SkillTargetType
{
    Self,   
    Enemy,  
    Any     
}
public abstract class DuckSkillSO : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    public int energyCost;
    public Sprite icon;
    public Color skillColor = Color.yellow; 

    [Header("Targeting Rules")]
    public SkillTargetType targetType;

    [Header("Visual Config")]
    public Color validColor = Color.green;  
    public Color invalidColor = Color.red;

    public Color executionColor = new Color(1f, 0.5f, 0f, 0.8f); // Màu cam/đậm hơn mặc định
    public float impactDuration = 0.3f; // Thời gian nháy

    [TextArea] public string description;

    [Header("VISUAL SETTINGS")]

    [Tooltip("Prefab hiển thị vùng ảnh hưởng (Ghost) khi di chuột")]
    public GameObject ghostPrefab;
    [Tooltip("Hình ảnh hiển thị dưới dạng Ghost trên bàn cờ")]
    public Sprite ghostSprite;

    [Tooltip("Kích thước vùng Skill (Ví dụ: 1x1, 3x3). Dùng để tính Scale.")]
    public Vector2Int areaSize = Vector2Int.one; // Mặc định 1x1


    public abstract List<Vector2Int> GetAffectedPositions(Vector2Int pivotPos, IGridSystem targetGrid);

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

        eventChannel.RaiseSkillDeselected();
        return true;
    }

    protected virtual void ApplyVisualFeedback(List<Vector2Int> area, BattleEventChannelSO channel, Owner target)
    {
        channel.RaiseSkillImpactVisual(target, area, executionColor, impactDuration);

    }
}