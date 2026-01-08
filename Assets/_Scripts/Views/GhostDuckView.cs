using System.Collections.Generic;
using UnityEngine;

public class GhostDuckView : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject lotusLeafPrefab;
    [SerializeField] private GameObject duckSegmentPrefab; 

    [Header("Settings")]
    [SerializeField] private Vector2 cellSize = new Vector2(1f, 1f);
    [SerializeField] private float transparency = 0.6f;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.red;

    // --- CÁC BIẾN PRIVATE ---
    private List<GhostSegmentView> _segmentsPool = new List<GhostSegmentView>();
    private DuckDataSO _currentData;
    private bool _isCurrentPosValid = true;


    private bool _isHorizontal = true;
    public bool IsHorizontal => _isHorizontal;


    public void Show(DuckDataSO data)
    {
        if (data == null) return;
        UpdateVisual(data); // Gọi lại logic dựng hình cũ
        gameObject.SetActive(true);
    }


    public void Hide()
    {
        gameObject.SetActive(false);
    }


    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }


    public void RotateVisual()
    {
        if (_currentData == null) return;

        // Xoay 90 độ (logic UI/2D thường là -90)
        transform.Rotate(0, 0, -90);

        // Đảo ngược trạng thái xoay
        _isHorizontal = !_isHorizontal;
    }

    private void UpdateVisual(DuckDataSO data)
    {
        _currentData = data;

        // Reset trạng thái xoay về mặc định khi hiển thị vịt mới
        _isHorizontal = true;
        transform.localRotation = Quaternion.identity;

        int size = data.size;

        // --- BƯỚC 1: Xử lý Pool ---
        while (_segmentsPool.Count < size)
        {
            CreateNewSegment();
        }

        // --- BƯỚC 2: Setup từng đốt ---
        for (int i = 0; i < _segmentsPool.Count; i++)
        {
            GhostSegmentView segment = _segmentsPool[i];

            if (i < size)
            {
                segment.gameObject.SetActive(true);

                // Tính toán vị trí local (luôn tính theo chiều ngang ban đầu)
                Vector3 localPos = new Vector3(i * cellSize.x, 0, 0);
                segment.transform.localPosition = localPos;

                segment.Setup(data.icon, i * 2);
            }
            else
            {
                segment.gameObject.SetActive(false);
            }
        }

        // --- BƯỚC 3: Căn giữa ---
        CenterGhost(size);

        UpdateColorState();
    }

    private void CreateNewSegment()
    {
        // Tạo Container
        GameObject container = new GameObject($"Segment_{_segmentsPool.Count}");
        container.transform.SetParent(transform, false);

        // Tạo Lá & Vịt
        GameObject leaf = Instantiate(lotusLeafPrefab, container.transform);
        GameObject duckSeg = Instantiate(duckSegmentPrefab, container.transform);

        // Gắn Script quản lý view
        GhostSegmentView view = container.AddComponent<GhostSegmentView>();

        // Tự động tìm Renderer
        var duckRend = duckSeg.GetComponentInChildren<SpriteRenderer>();
        var leafRend = leaf.GetComponentInChildren<SpriteRenderer>();

        view.ManualInit(duckRend, leafRend);

        _segmentsPool.Add(view);
    }

    private void CenterGhost(int size)
    {
        float totalWidth = (size - 1) * cellSize.x;
        Vector3 centerOffset = new Vector3(-totalWidth / 2f, 0, 0);
        foreach (var seg in _segmentsPool)
        {
            if (seg.gameObject.activeSelf)
                seg.transform.localPosition += centerOffset;
        }
    }

    public void SetValidationState(bool isValid)
    {
        if (_isCurrentPosValid != isValid) // Chỉ update khi trạng thái thay đổi để tối ưu
        {
            _isCurrentPosValid = isValid;
            UpdateColorState();
        }
    }

    private void UpdateColorState()
    {
        foreach (var seg in _segmentsPool)
        {
            if (seg.gameObject.activeSelf)
            {
                seg.SetTransparency(transparency, validColor, invalidColor, _isCurrentPosValid);
            }
        }
    }
    public void Rotate()
    {
        if (_currentData == null) return;

        transform.Rotate(0, 0, -90);

        _isHorizontal = !_isHorizontal;
    }
}