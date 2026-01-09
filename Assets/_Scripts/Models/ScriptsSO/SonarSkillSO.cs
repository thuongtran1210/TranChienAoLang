using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Duck Battle/Skills/Sonar Skill")]
public class SonarSkillSO : DuckSkillSO
{
    // Cấu hình bán kính quét (1 = 3x3, 2 = 5x5)
    [SerializeField] private int _radius = 1;
    [SerializeField] private Color _highlightColor = Color.yellow;

    public override bool Execute(IGridSystem targetGrid, Vector2Int centerPos, BattleEventChannelSO eventChannel)
    {
        if (!targetGrid.IsValidPosition(centerPos))
        {
            eventChannel.RaiseSkillFeedback("Invalid Target!", centerPos);
            return false;
        }

        int foundParts = 0;

        // Tạo danh sách các ô cần highlight
        List<Vector2Int> highlightArea = new List<Vector2Int>();

        for (int x = -_radius; x <= _radius; x++)
        {
            for (int y = -_radius; y <= _radius; y++)
            {
                Vector2Int checkPos = centerPos + new Vector2Int(x, y);

                if (targetGrid.IsValidPosition(checkPos))
                {
                    // Add vào danh sách visual
                    highlightArea.Add(checkPos);

                    var cell = targetGrid.GetCell(checkPos);
                    // Logic check tàu (Giữ nguyên logic của bạn)
                    if (cell.OccupiedUnit != null && !cell.IsHit)
                    {
                        foundParts++;
                    }
                }
            }
        }

        // BẮN EVENT VISUAL: Gửi 1 lần duy nhất danh sách các ô cần tô màu
        eventChannel.RaiseGridHighlight(highlightArea, _highlightColor);

        // Feedback Text
        string message = foundParts > 0 ? $"Sonar detected {foundParts} signals!" : "No signals.";
        eventChannel.RaiseSkillFeedback(message, centerPos);

        return true;
    }
}