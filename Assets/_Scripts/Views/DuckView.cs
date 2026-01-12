using System.Collections.Generic;
using UnityEngine;

public class DuckView : MonoBehaviour
{
    [Header("Prefabs References")]
    [SerializeField] private GameObject lotusLeafPrefab;   // Prefab Visual Lá sen
    [SerializeField] private GameObject duckSegmentPrefab; // Prefab Visual Vịt con

    [Header("Visual Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int baseSortingOrder = 5; // Thấp hơn Ghost (thường là 10) để Ghost đè lên

    // Lưu trữ các renderer để sau này làm hiệu ứng (chớp nháy khi trúng đạn, chìm...)
    private List<SpriteRenderer> _duckRenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> _leafRenderers = new List<SpriteRenderer>();

    /// <summary>
    /// Hàm nhận dữ liệu từ GridView để hiển thị (Thay thế tên Initialize cũ)
    /// </summary>
    /// <param name="data">Dữ liệu cấu hình con vịt (SO)</param>
    /// <param name="isHorizontal">Trạng thái xoay</param>
    public void Bind(DuckDataSO data, bool isHorizontal)
    {
        // 1. Dọn dẹp visual cũ (Clean up)
        ClearOldVisuals();

        if (data == null) return;

        // 2. Xử lý xoay (Rotation Logic)
        transform.localRotation = Quaternion.identity; // Reset trước
        if (!isHorizontal)
        {
            // Xoay -90 độ nếu là chiều dọc
            transform.Rotate(0, 0, -90);
        }

        // 3. Spawn từng đốt (Segment Instantiation)
        for (int i = 0; i < data.size; i++)
        {
            CreateSegment(i, data);
        }
    }
    public void Bind(DuckUnit unit)
    {
        if (unit == null)
        {
            Debug.LogError("[DuckView] Cannot Bind null DuckUnit!");
            return;
        }

        Bind(unit.Data, unit.IsHorizontal);
        unit.OnHealthChanged += HandleHealthChanged;
        unit.OnSunk += HandleSunk;
    }
    private void HandleHealthChanged(int current, int max) { /* TODO: Cập nhật visual */ }
    private void HandleSunk() { /*TODO:  Play animation chìm */ }

    /// <summary>
    /// Hàm này được gọi ngay sau khi Instantiate DuckUnit_Base
    /// </summary>
    /// <param name="data">Dữ liệu con vịt</param>
    /// <param name="isHorizontal">Hướng đặt</param>
    private void CreateSegment(int index, DuckDataSO data)
    {
        // Tạo container rỗng
        GameObject segmentContainer = new GameObject($"Segment_{index}");
        segmentContainer.transform.SetParent(transform, false);

        // Tính vị trí local theo trục X (vì đã xoay cả object cha rồi)
        segmentContainer.transform.localPosition = new Vector3(index * cellSize, 0, 0);

        // --- A. Spawn Lá Sen ---
        if (lotusLeafPrefab != null)
        {
            GameObject leaf = Instantiate(lotusLeafPrefab, segmentContainer.transform);
            leaf.transform.localPosition = Vector3.zero;

            // Xử lý Sorting Order cho Lá
            var leafRend = leaf.GetComponentInChildren<SpriteRenderer>();
            if (leafRend)
            {
                leafRend.sortingOrder = baseSortingOrder;
                _leafRenderers.Add(leafRend);
            }
        }

        // --- B. Spawn Vịt ---
        if (duckSegmentPrefab != null)
        {
            GameObject duck = Instantiate(duckSegmentPrefab, segmentContainer.transform);
            duck.transform.localPosition = Vector3.zero;

            // Xử lý Sorting Order cho Vịt
            var duckRend = duck.GetComponentInChildren<SpriteRenderer>();
            if (duckRend)
            {
                // Logic lấy Sprite: Ưu tiên lấy theo list nếu có, không thì lấy icon chung
                // Nếu DuckDataSO của bạn có List<Sprite> segments, hãy dùng: data.segments[index]
                if (data.icon != null)
                {
                    duckRend.sprite = data.icon;
                }

                duckRend.sortingOrder = baseSortingOrder + 1; // Đè lên lá
                _duckRenderers.Add(duckRend);
            }
        }
    }

    private void ClearOldVisuals()
    {
        _duckRenderers.Clear();
        _leafRenderers.Clear();

        // Destroy toàn bộ con (children) để tái tạo lại từ đầu
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    // --- CÁC HÀM VISUAL EFFECTS (Để dùng sau này) ---

    public void OnHit()
    {
        // Ví dụ: Đổi màu đỏ nháy lên
        foreach (var rend in _duckRenderers) rend.color = Color.red;
    }

    public void OnDie()
    {
        // Ví dụ: Fade mờ đi và chìm xuống
        foreach (var rend in _duckRenderers)
        {
            rend.color = Color.gray;
            // Logic animation chìm...
        }
    }
}