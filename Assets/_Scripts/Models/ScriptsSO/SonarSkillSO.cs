using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Duck Battle/Skills/Sonar Skill")]
public class SonarSkillSO : DuckSkillSO
{
    [SerializeField] private int _radius = 1;
    [Header("Visual Feedback")]
    [Tooltip("Tile hiển thị khi PHÁT HIỆN mục tiêu")]
    [SerializeField] private TileBase _detectedIndicatorTile; // [MOD] Thay Color bằng TileBase

    [Tooltip("Màu hiển thị vùng quét (khi không thấy gì hoặc nền)")]
    [SerializeField] private Color _scanAreaColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);


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
        // 1. Validate & 2. Get Area 
        if (!targetGrid.IsValidPosition(pivotPos)) return false;
        Owner actualTargetOwner = targetOwner;

        // Kiểm tra xem GridSystem có cung cấp thông tin Owner không (Dựa trên context SkillInteractionController có dùng GridOwner)
        if (targetGrid is IGridLogic gridLogic)
        {
            actualTargetOwner = gridLogic.GridOwner;
        }

        // 2. Get Area
        List<Vector2Int> scanArea = GetAffectedPositions(pivotPos, targetGrid);

        // 3. Logic tìm kiếm
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

        // 4. Xử lý Visual Feedback
        if (foundParts > 0)
        {
            // CASE A: Tìm thấy địch
            eventChannel.RaiseTileIndicator(detectedPositions, _detectedIndicatorTile, impactDuration);

            eventChannel.RaiseSkillImpactVisual(actualTargetOwner, scanArea, _scanAreaColor, impactDuration);

            string msg = $"Sonar detected {foundParts} signals!";
            eventChannel.RaiseSkillFeedback(msg, pivotPos);
        }
        else
        {
            // CASE B: Không thấy gì
            eventChannel.RaiseSkillImpactVisual(actualTargetOwner, scanArea, _scanAreaColor, impactDuration);
            eventChannel.RaiseSkillFeedback("No signals.", pivotPos);
        }

        return true;
    }
}