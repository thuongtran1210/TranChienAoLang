using UnityEngine;
using UnityEngine.Rendering; // Dùng cho SortingGroup

public class DuckView : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private SortingGroup sortingGroup;

    [Header("Visual Prefabs")]
    [Tooltip("Prefab của 1 khúc thân vịt (hoặc 1 con vịt nhỏ)")]
    [SerializeField] private GameObject realDuckSegmentPrefab;

    [Tooltip("Prefab của chiếc lá sen")]
    [SerializeField] private GameObject lotusLeafPrefab;

    [Header("Containers")]
    [SerializeField] private Transform visualsContainer; // Chứa cả Vịt và Lá

    [Header("Sorting Orders")]
    [SerializeField] private int duckOrder = 10;
    [SerializeField] private int leafOrder = 0;

    private DuckUnit _linkedDuck;

    public void Bind(DuckUnit duckModel, DuckDataSO data)
    {
        _linkedDuck = duckModel;

        // 1. Xóa sạch visual cũ (để tránh chồng chéo khi pool)
        ClearOldVisuals();

        // 2. SINH RA CẶP "LÁ + VỊT" CHO TỪNG Ô
 
        foreach (var offset in data.structure)
        {
            GenerateSegment(offset);
        }

        // 3. Xoay toàn bộ DuckView theo hướng data
        UpdateRotation(duckModel.IsHorizontal);

        // 4. Các logic sự kiện (Health, Sunk...) giữ nguyên
        SubscribeEvents();
    }

    private void GenerateSegment(Vector2Int offset)
    {
        // --- A. TẠO LÁ SEN ---
        if (lotusLeafPrefab != null)
        {
            GameObject leaf = Instantiate(lotusLeafPrefab, visualsContainer);
            leaf.transform.localPosition = new Vector3(offset.x, offset.y, 0);

            // Đẩy lá xuống dưới
            if (leaf.TryGetComponent<SpriteRenderer>(out var srLeaf))
            {
                srLeaf.sortingOrder = leafOrder;
            }
        }

        // --- B. TẠO KHÚC VỊT (SEGMENT) ---
        if (realDuckSegmentPrefab != null)
        {
            GameObject duckSeg = Instantiate(realDuckSegmentPrefab, visualsContainer);
            // Đặt cùng vị trí với lá
            duckSeg.transform.localPosition = new Vector3(offset.x, offset.y, 0);

            // Đẩy vịt lên trên lá
            if (duckSeg.TryGetComponent<SpriteRenderer>(out var srDuck))
            {
                srDuck.sortingOrder = duckOrder;
            }
        }
    }

    private void ClearOldVisuals()
    {
        if (visualsContainer == null) return;
        foreach (Transform child in visualsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateRotation(bool isHorizontal)
    {
        float zRot = isHorizontal ? 0f : 90f;
        transform.localRotation = Quaternion.Euler(0, 0, zRot);

    }


    private void SubscribeEvents()
    {
        if (_linkedDuck != null)
        {
            _linkedDuck.OnHealthChanged += HandleHitVisual;
            _linkedDuck.OnSunk += HandleSunkVisual;
        }
    }

    private void HandleHitVisual(int currentHits, int maxHits)
    {
        Debug.Log($"[DuckView] {gameObject.name} bị bắn! HP: {currentHits}/{maxHits}");
    }

    private void HandleSunkVisual()
    {
        Debug.Log($"[DuckView] {gameObject.name} ĐÃ CHÌM!");
  
    }

    private void OnDestroy()
    {
        if (_linkedDuck != null)
        {
            _linkedDuck.OnHealthChanged -= HandleHitVisual;
            _linkedDuck.OnSunk -= HandleSunkVisual;
        }
    }
}