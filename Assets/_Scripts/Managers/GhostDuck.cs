using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GhostDuck : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private DuckFootprintGenerator footprintGenerator;

    [Header("Settings")]
    [SerializeField] private GameObject leafPrefab;         
    [SerializeField] private GameObject duckSegmentPrefab; 

    [SerializeField] private Color validColor = new Color(1, 1, 1, 0.6f);
    [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.6f);

    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int duckSortingOrder = 10;
    [SerializeField] private int leafSortingOrder = 5;


    private List<GameObject> _currentVisuals = new List<GameObject>();

    public DuckDataSO CurrentData { get; private set; }
    public bool IsHorizontal { get; private set; } = true;

    private void Awake()
    {
        if (footprintGenerator == null)
            footprintGenerator = GetComponent<DuckFootprintGenerator>();
    }

    public void Show(DuckDataSO data)
    {
        CurrentData = data;
        IsHorizontal = true;

        // 1. Sinh ra toàn bộ Visual (Lá + Vịt)
        GenerateVisuals(data);

        // 2. Reset Transform
        transform.localRotation = Quaternion.identity;
        SetValidationState(true);
        gameObject.SetActive(true);
    }

    private void GenerateVisuals(DuckDataSO data)
    {
        _currentVisuals.Clear(); 

        if (footprintGenerator != null)
        {
            // Lần 1: Vẽ LÁ 
            var leaves = footprintGenerator.Generate(data, leafPrefab, leafSortingOrder, clearPrevious: true);
            _currentVisuals.AddRange(leaves);

            // Lần 2: Vẽ VỊT 
            if (duckSegmentPrefab != null)
            {
                var ducks = footprintGenerator.Generate(data, duckSegmentPrefab, duckSortingOrder, clearPrevious: false);
                _currentVisuals.AddRange(ducks);
            }
        }
    }

    public void Rotate()
    {
        IsHorizontal = !IsHorizontal;
        float zRot = IsHorizontal ? 0 : 90;
        transform.localRotation = Quaternion.Euler(0, 0, zRot);


    }

    public void SetPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    public void SetValidationState(bool isValid)
    {
        Color targetColor = isValid ? validColor : invalidColor;

        
        foreach (var obj in _currentVisuals)
        {
            if (obj && obj.TryGetComponent<SpriteRenderer>(out var sr))
            {
                sr.color = targetColor;
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        CurrentData = null;
    }
}