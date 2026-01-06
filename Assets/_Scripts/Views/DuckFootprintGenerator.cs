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
    /// Tuân thủ Open/Closed Principle: Mở rộng visual (prefab khác nhau) nhưng đóng logic xếp grid.
    /// </summary>
    /// <param name="data">Dữ liệu tàu</param>
    /// <param name="prefab">Prefab cần sinh ra (Lá cho Ghost, Vịt cho View)</param>
    /// <param name="sortingOrder">Thứ tự render</param>
    /// <returns>Trả về List để Controller bên ngoài có thể đổi màu hoặc thao tác thêm</returns>
    public List<GameObject> Generate(DuckDataSO data, GameObject prefab, int sortingOrder)
    {
        if (data == null || prefab == null)
        {
            Debug.LogWarning("[DuckFootprintGenerator] Missing Data or Prefab!");
            return new List<GameObject>();
        }

        // 1. Recycle / Hide old objects (Smart Pooling)
        ReturnToPool();

        // 2. Spawn new objects based on data
        foreach (Vector2Int gridOffset in data.structure)
        {
            GameObject segment = GetFromPool(prefab);

            // Setup Position
            float halfCell = cellSize * 0.5f;
            segment.transform.localPosition = new Vector3(
                (gridOffset.x * cellSize) + halfCell,
                (gridOffset.y * cellSize) + halfCell,
                0
            );

            // Setup Rotation (Random nhẹ cho tự nhiên nếu muốn, hoặc reset)
            segment.transform.localRotation = Quaternion.identity;

            // Setup Sorting Order
            SetSortingOrder(segment, sortingOrder);

            _activeSegments.Add(segment);
        }

        return _activeSegments;
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        GameObject instance;
        if (_pool.Count > 0)
        {
            instance = _pool.Dequeue();
            instance.SetActive(true);
        }
        else
        {
            instance = Instantiate(prefab, transform);
        }
        return instance;
    }

    private void ReturnToPool()
    {
        foreach (var item in _activeSegments)
        {
            item.SetActive(false);
            _pool.Enqueue(item);
        }
        _activeSegments.Clear();
    }

    private void SetSortingOrder(GameObject obj, int order)
    {
        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            sr.sortingOrder = order;
        }
    }
}