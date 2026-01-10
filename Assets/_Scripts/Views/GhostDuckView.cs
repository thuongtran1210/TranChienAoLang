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

    // --- PRIVATE ---
    private List<GhostSegmentView> _segmentsPool = new List<GhostSegmentView>();
    private DuckDataSO _currentData;
    private bool _isCurrentPosValid = true;

    private bool _isHorizontal = true;
    public bool IsHorizontal => _isHorizontal;

    public void Show(DuckDataSO data)
    {
        if (data == null) return;

        transform.rotation = Quaternion.identity;
        _isHorizontal = true;

        UpdateVisual(data);
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

    public void Rotate()
    {
        Debug.Log($"[GhostDuckView] Rotating! Current: {_isHorizontal} -> New: {!_isHorizontal} | Frame: {Time.frameCount}");

        if (_currentData == null) return;

        _isHorizontal = !_isHorizontal;

        Debug.Log($"[GhostDuckView] Rotating ONCE! New State: {(_isHorizontal ? "Horizontal" : "Vertical")}");

        float targetAngle = _isHorizontal ? 0f : -90f;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);

    }

    private void UpdateVisual(DuckDataSO data)
    {
        _currentData = data;
        int size = data.size;

        // 1. Xử lý Pool (Object Pooling)
        while (_segmentsPool.Count < size)
        {
            CreateNewSegment();
        }

        // 2. Setup từng đốt
        for (int i = 0; i < _segmentsPool.Count; i++)
        {
            GhostSegmentView segment = _segmentsPool[i];

            if (i < size)
            {
                segment.gameObject.SetActive(true);
                Vector3 localPos = new Vector3(i * cellSize.x, 0, 0);
                segment.transform.localPosition = localPos;

                segment.Setup(data.icon, i * 2);
            }
            else
            {
                segment.gameObject.SetActive(false);
            }
        }

        // 3. Update Color
        UpdateColorState();

        
    }

    private void CreateNewSegment()
    {
        GameObject container = new GameObject($"Segment_{_segmentsPool.Count}");
        container.transform.SetParent(transform, false);

        GameObject leaf = Instantiate(lotusLeafPrefab, container.transform);
        GameObject duckSeg = Instantiate(duckSegmentPrefab, container.transform);

        GhostSegmentView view = container.AddComponent<GhostSegmentView>();
        var duckRend = duckSeg.GetComponentInChildren<SpriteRenderer>();
        var leafRend = leaf.GetComponentInChildren<SpriteRenderer>();

        view.ManualInit(duckRend, leafRend);
        _segmentsPool.Add(view);
    }

    public void SetValidationState(bool isValid)
    {
        if (_isCurrentPosValid != isValid)
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
}