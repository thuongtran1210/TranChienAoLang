using UnityEngine;

[CreateAssetMenu(menuName = "Duck Battle/Skills/Sonar Skill")]
public class SonarSkillSO : DuckSkillSO
{
    // Cấu hình bán kính quét (1 = 3x3, 2 = 5x5)
    [SerializeField] private int _radius = 1;

    public override bool Execute(IGridSystem targetGrid, Vector2Int centerPos, BattleEventChannelSO eventChannel)
    {
        // 1. Validate: Không cho cast skill ra ngoài map quá xa (tùy game design)
        if (!targetGrid.IsValidPosition(centerPos))
        {
            eventChannel.RaiseSkillFeedback("Invalid Target!", centerPos);
            return false;
        }

        int foundParts = 0;

        // 2. Logic quét vùng (Clean Code loop)
        for (int x = -_radius; x <= _radius; x++)
        {
            for (int y = -_radius; y <= _radius; y++)
            {
                Vector2Int checkPos = centerPos + new Vector2Int(x, y);

                // Chỉ check ô hợp lệ
                if (targetGrid.IsValidPosition(checkPos))
                {
                    var cell = targetGrid.GetCell(checkPos);

                    // Logic Sonar: Chỉ phát hiện có Unit, không quan tâm Unit gì, và chưa bị bắn
                    // Lưu ý: Tùy game design, bạn có muốn Sonar phát hiện cả tàu đã chết không?
                    // Ở đây tôi giả định là đếm các phần tàu CHƯA bị bắn trúng.
                    if (cell.OccupiedUnit != null && !cell.IsHit)
                    {
                        foundParts++;
                    }

                    // TODO: Gửi sự kiện để Highlight ô này trên UI (Visual Feedback)
                    // eventChannel.RaiseGridHighlight(checkPos, Color.green);
                }
            }
        }

        // 3. Feedback kết quả
        string message = foundParts > 0
            ? $"Sonar detected {foundParts} signals!"
            : "No signals detected.";

        Debug.Log($"[Sonar] Center: {centerPos} | Result: {foundParts}");
        eventChannel.RaiseSkillFeedback(message, centerPos);

        return true; // Skill thực thi thành công -> Trừ mana
    }
}