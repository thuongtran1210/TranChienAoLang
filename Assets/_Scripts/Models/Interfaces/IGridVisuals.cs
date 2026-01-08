// 2. Interface chuyên về Hiển thị/Phản hồi (Nếu bạn vẫn muốn GridManager quản lý Visual)
// LƯU Ý: Theo cách Refactor ở câu trả lời trước, tốt nhất nên bỏ hẳn phần này khỏi GridManager.
// Nhưng nếu bạn muốn giữ, hãy tách nó ra.
using UnityEngine;

public interface IGridVisuals
{
    void ShowGhost(DuckDataSO data);
    void HideGhost();
    void UpdateGhostPosition(Vector3 worldPos);
    void SetGhostValidation(bool isValid);
    void ToggleGhostRotation();
}