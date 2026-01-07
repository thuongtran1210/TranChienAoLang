using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class GridView : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 originPosition; // Vị trí bắt đầu của lưới (góc dưới trái)

    [Header("References")]
    [SerializeField] private GridCellView cellPrefab;
    [SerializeField] private Transform gridContainer;

    private GridCellView[,] _cellViews;

    public void InitializeBoard(int width, int height, GridSystem gridSystem, Owner owner)
    {
        _cellViews = new GridCellView[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 localPos = GetLocalPosition(x, y);

                GridCellView cellView = Instantiate(cellPrefab, gridContainer);
                cellView.transform.localPosition = localPos;

                var cellLogic = gridSystem.GetCell(new Vector2Int(x, y));

                cellView.Setup(cellLogic, owner);

                _cellViews[x, y] = cellView;
            }
        }
    }

    // --- SPAWN DUCK ---

    public void SpawnDuck(Vector2Int gridPos, bool isHorizontal, DuckDataSO data, DuckUnit logicUnit)
    {
        if (data == null || data.unitPrefab == null) return;

        Vector3 localSpawnPos = CalculateLocalCenterPosition(gridPos, data.size, isHorizontal);

      
        Quaternion rotation = isHorizontal ? Quaternion.identity : Quaternion.Euler(0, 0, 90);


        GameObject duckObj = Instantiate(data.unitPrefab, transform);

        duckObj.transform.localPosition = localSpawnPos;
        duckObj.transform.localRotation = rotation;

        // --- Setup Logic ---
        DuckView view = duckObj.GetComponent<DuckView>();
        if (view != null)
        {
            view.Bind(logicUnit, data);
        }
    }
    public Vector3 GetWorldPositionOfCell(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x , gridPos.y , 0);
    }

    public void UpdateCellState(Vector2Int pos, ShotResult result)
    {
        if (IsValidPosition(pos))
        {
            _cellViews[pos.x, pos.y].UpdateVisual(result);
        }
    }

    // --- Helpers ---

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 && pos.x < _cellViews.GetLength(0) && pos.y < _cellViews.GetLength(1);
    }
    // Chuyển đổi từ tọa độ thế giới (nơi chuột trỏ) về tọa độ Grid (x, y)
    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        localPos -= originPosition;

        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);
        return new Vector2Int(x, y);
    }
    public Vector3 GetLocalPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    // Lấy góc dưới trái của ô
    public Vector3 GetWorldPosition(int x, int y)
    {
        return transform.TransformPoint(GetLocalPosition(x, y));
    }

    // Lấy tâm ô 
    public Vector3 GetWorldCenterPosition(int x, int y)
    {
        Vector3 halfCell = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);
        Vector3 localCenter = GetLocalPosition(x, y) + halfCell;
        return transform.TransformPoint(localCenter);
    }
    /// <summary>
    /// Tính toán vị trí LOCAL trên Board   
    /// </summary>
    public Vector3 CalculateLocalCenterPosition(Vector2Int startGridPos, int size, bool isHorizontal)
    {
        // 1. Lấy vị trí góc dưới trái của ô bắt đầu
        Vector3 cellLocalPos = GetLocalPosition(startGridPos.x, startGridPos.y);

        // 2. Lấy tâm của ô 
        Vector3 halfCell = new Vector3(cellSize , cellSize , 0);


        return cellLocalPos + halfCell;
    }
    /// <summary>
    /// Chuyển đổi tọa độ Grid (Vector2Int) sang World Position (Vector3).
    /// </summary>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        
        return GetWorldPosition(gridPos.x, gridPos.y);
    }
}