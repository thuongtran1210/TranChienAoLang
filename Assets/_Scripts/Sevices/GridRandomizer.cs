// Assets/_Scripts/Services/GridRandomizer.cs
using UnityEngine;
using System.Collections.Generic;

public class GridRandomizer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxAttempts = 100; // Tránh vòng lặp vô tận

    /// <summary>
    /// Xếp ngẫu nhiên danh sách vịt lên GridManager mục tiêu.
    /// </summary>
    public void RandomizePlacement(GridManager targetGrid, List<DuckDataSO> ducksToPlace)
    {
        if (targetGrid == null || ducksToPlace == null || ducksToPlace.Count == 0)
        {
            Debug.LogWarning("[GridRandomizer] Invalid parameters!");
            return;
        }

        // 1. Clear bàn cờ trước khi xếp
        targetGrid.GridSystem.Clear();

        // 2. Duyệt qua từng con vịt cần xếp
        foreach (var duckData in ducksToPlace)
        {
            bool placed = false;
            int attempts = 0;

            // 3. Thử tìm vị trí hợp lệ
            while (!placed && attempts < maxAttempts)
            {
                // Random tọa độ
                // Giả sử GridManager có thể cung cấp Width/Height, hoặc ta hardcode theo Setting
                // Tốt nhất: targetGrid.Width lấy từ GridSystem
                int randX = Random.Range(0, 10);
                int randY = Random.Range(0, 10);

                // Random hướng (Horizontal / Vertical)
                bool isHorizontal = Random.value > 0.5f;

                // Chuyển đổi sang World Position để gọi hàm của GridManager
                // Lưu ý: GridManager cần hàm lấy WorldPos từ GridPos, hoặc ta sửa TryPlaceShip nhận GridPos.
                // Ở đây tôi dùng cách gọi hiện tại của bạn: WorldPos.
                Vector3 tryPos = targetGrid.GetWorldPosition(new Vector2Int(randX, randY));

                // 4. Gọi TryPlaceShip của GridManager (Tận dụng logic validate có sẵn)
                if (targetGrid.TryPlaceShip(tryPos, duckData, isHorizontal))
                {
                    placed = true;
                }

                attempts++;
            }

            if (!placed)
            {
                Debug.LogError($"[GridRandomizer] Could not place duck {duckData.duckName} after {maxAttempts} attempts!");
                // Có thể cân nhắc Clear() và thử lại từ đầu (Backtracking) nếu cần thuật toán chặt chẽ hơn.
            }
        }
    }
}