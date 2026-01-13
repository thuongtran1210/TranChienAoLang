using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Duck Battle/Skills/Sonar Skill")]
public class SonarSkillSO : DuckSkillSO
{
    [SerializeField] private int _radius = 1;
    [Tooltip("Màu hiển thị khi PHÁT HIỆN mục tiêu (Ping trúng địch)")]
    [SerializeField] private Color _detectedColor = Color.red;

    [Tooltip("Màu hiển thị khi KHÔNG tìm thấy gì (Ping vào vùng nước trống)")]
    [SerializeField] private Color _nothingFoundColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);


    public override List<Vector2Int> GetAffectedPositions(Vector2Int pivotPos, IGridSystem targetGrid)
    {
        List<Vector2Int> area = new List<Vector2Int>();

        for (int x = -_radius; x <= _radius; x++)
        {
            for (int y = -_radius; y <= _radius; y++)
            {
                Vector2Int checkPos = pivotPos + new Vector2Int(x, y);

                // Chỉ thêm vào list nếu nằm trong Grid
                if (targetGrid.IsValidPosition(checkPos))
                {
                    area.Add(checkPos);
                }
            }
        }
        return area;
    }


    public override bool Execute(IGridSystem targetGrid, Vector2Int pivotPos, BattleEventChannelSO eventChannel, Owner targetOwner)
    {
        // 1. Validate Input
        if (!targetGrid.IsValidPosition(pivotPos)) return false;

        // 2. Lấy vùng ảnh hưởng
        List<Vector2Int> scanArea = GetAffectedPositions(pivotPos, targetGrid);

        // 3. Xử lý Logic tìm kiếm (Core Logic)
        List<Vector2Int> detectedPositions = new List<Vector2Int>();
        int foundParts = 0;

        foreach (var pos in scanArea)
        {
            var cell = targetGrid.GetCell(pos);
            if (cell != null && cell.OccupiedUnit != null && !cell.IsHit)
            {
                foundParts++;
                detectedPositions.Add(pos);
            }
        }

        // 4. Xử lý Visual Feedback (Context-Aware)
        if (foundParts > 0)
        {
            // CASE A: Tìm thấy địch -> Chỉ hiển thị vị trí của địch (Ping!)
            // Bạn có thể đổi thành hiển thị cả vùng scanArea với màu cảnh báo nếu muốn giấu vị trí chính xác.
            // Ở đây tôi chọn hiển thị vị trí chính xác để đúng chất "Sonar".
            eventChannel.RaiseSkillImpactVisual(targetOwner, detectedPositions, _detectedColor, impactDuration);

            string msg = $"Sonar detected {foundParts} signals!";
            eventChannel.RaiseSkillFeedback(msg, pivotPos);
        }
        else
        {
            // CASE B: Không thấy gì -> Hiển thị vùng đã quét để người chơi biết skill đã hoạt động
            eventChannel.RaiseSkillImpactVisual(targetOwner, scanArea, _nothingFoundColor, impactDuration);

            eventChannel.RaiseSkillFeedback("No signals.", pivotPos);
        }

        // 5. Cleanup
        eventChannel.RaiseSkillDeselected();

        return true;
    }
}