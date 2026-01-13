using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Duck Battle/Skills/Sonar Skill")]
public class SonarSkillSO : DuckSkillSO
{
    [SerializeField] private int _radius = 1;


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
        {
            // Gọi base để validate
            if (!targetGrid.IsValidPosition(pivotPos)) return false;

            List<Vector2Int> area = GetAffectedPositions(pivotPos, targetGrid);

            int foundParts = 0;
            foreach (var pos in area)
            {
                var cell = targetGrid.GetCell(pos);
                if (cell.OccupiedUnit != null && !cell.IsHit)
                {
                    foundParts++;
                }
            }

            // Visual
            base.ApplyVisualFeedback(area, eventChannel, targetOwner);

         
            string msg = foundParts > 0 ? $"Sonar detected {foundParts} signals!" : "No signals.";
            eventChannel.RaiseSkillFeedback(msg, pivotPos);

            return true;
        }
    }
}