using System.Collections.Generic;
using UnityEngine;

// Biến dữ liệu DataSO thành các GameObjects trên Scene theo Grid.
public class DuckFootprintGenerator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cellSize = 1f;

    private readonly List<GameObject> _activeSegments = new List<GameObject>();
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();

    /// <summary>
    /// Hàm Generic để tạo hình.
    /// </summary>
    /// <param name="data">Dữ liệu tàu</param>
    /// <param name="prefab">Prefab cần sinh ra (Lá cho Ghost, Vịt cho View)</param>
    /// <param name="sortingOrder">Thứ tự render</param>
    /// <returns>Trả về List để Controller bên ngoài có thể đổi màu hoặc thao tác thêm</returns>
    /// <summary>
    /// Tạo hình dáng tàu.
    /// </summary>
    /// <param name="clearPrevious">Nếu true: Ẩn các object cũ (dùng cho lần vẽ đầu tiên). Nếu false: Vẽ thêm vào (dùng cho layer thứ 2).</param>
    public List<GameObject> Generate(DuckDataSO data, GameObject prefab, int sortingOrder, bool clearPrevious = true)
    {
        if (data == null || prefab == null) return new List<GameObject>();

        if (clearPrevious)
        {
            ReturnToPool();
        }

        List<GameObject> newlySpawned = new List<GameObject>();

    
        foreach (Vector2Int gridOffset in data.structure)
        {
            GameObject segment = GetFromPool(prefab);

            float halfCell = cellSize * 0.5f;
            segment.transform.localPosition = new Vector3(
                (gridOffset.x * cellSize) + halfCell,
                (gridOffset.y * cellSize) + halfCell,
                0
            );

            segment.transform.localRotation = Quaternion.identity;
            SetSortingOrder(segment, sortingOrder);

            _activeSegments.Add(segment);
            newlySpawned.Add(segment);
        }

        return newlySpawned;
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        // Logic Pool đơn giản: Nếu có hàng tồn thì lấy, không thì mua mới
        // Lưu ý: Nếu prefab khác nhau (Lá vs Vịt), pool này có thể bị trộn lẫn visual.
        // Giải pháp nhanh: Nếu object trong pool KHÁC prefab đang cần -> Destroy nó đi tạo mới.

        while (_pool.Count > 0)
        {
            GameObject item = _pool.Dequeue();

            // [FIX CRASH] Kiểm tra nếu object đã bị xóa từ bên ngoài
            if (item == null) continue;

            // Kiểm tra xem item này có đúng loại prefab mình cần không? (Check tên hoặc component)
            // Cách đơn giản nhất: Destroy nếu muốn an toàn tuyệt đối, hoặc tái sử dụng mù quáng (cẩn thận)
            // Ở đây tôi chọn giải pháp an toàn cho Fresher: Tạo mới nếu nghi ngờ, nhưng vẫn tái sử dụng GameObject

            // Reset lại SpriteRenderer nếu cần (vì pool dùng chung cho cả Lá và Vịt)
            if (item.TryGetComponent<SpriteRenderer>(out var sr) && prefab.TryGetComponent<SpriteRenderer>(out var prefabSr))
            {
                sr.sprite = prefabSr.sprite;
                sr.color = prefabSr.color;
            }

            item.SetActive(true);
            return item;
        }

        return Instantiate(prefab, transform);
    }

    private void ReturnToPool()
    {
   
        for (int i = _activeSegments.Count - 1; i >= 0; i--)
        {
            var item = _activeSegments[i];
            if (item != null)
            {
                item.SetActive(false);
                _pool.Enqueue(item);
            }
        }
        _activeSegments.Clear();
    }

    private void SetSortingOrder(GameObject obj, int order)
    {
        if (obj == null) return;
        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.sortingOrder = order;
        }
    }
}