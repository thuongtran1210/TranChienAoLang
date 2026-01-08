using UnityEngine;

[CreateAssetMenu(menuName = "Duck Battle/Skills/Sonar Skill")]
public class SonarSkillSO : DuckSkillSO
{
    public override bool Execute(IGridSystem targetGrid, Vector2Int centerPos, BattleEventChannelSO eventChannel)
    {
        int foundShips = 0;

        // Quét vùng 3x3 (từ x-1 đến x+1, y-1 đến y+1)
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int checkPos = centerPos + new Vector2Int(x, y);

                // Kiểm tra ô hợp lệ
                if (targetGrid.IsValidPosition(checkPos))
                {
                    // Lấy thông tin ô (Bạn cần đảm bảo IGridSystem có API GetCell hoặc tương tự)
                    // Ở đây tôi giả định logic check tàu. 
                    // Lưu ý: Cần truy cập dữ liệu Unit mà không làm lộ sương mù.
                    var cell = targetGrid.GetCell(checkPos);
                    if (cell != null && cell.OccupiedUnit != null)
                    {
                        foundShips++;
                    }
                }
            }
        }

        // Gửi thông báo kết quả (Cần thêm Event vào Channel ở Bước 3)
        Debug.Log($"[SONAR] Phát hiện {foundShips} phần tàu địch trong vùng 3x3!");
        eventChannel.RaiseSkillFeedback($"{foundShips} found!", centerPos);

        // Return true vì skill đã thực hiện xong
        return true;
    }
}