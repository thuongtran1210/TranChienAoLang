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
        Vector3 halfCellOffset = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 1. Lấy vị trí góc (Bottom-Left)
                Vector3 bottomLeftPos = GetLocalPosition(x, y);

                // 2. Tính vị trí tâm (Center)
                Vector3 centerPos = bottomLeftPos + halfCellOffset;

                GridCellView cellView = Instantiate(cellPrefab, gridContainer);

                // 3. Đặt View vào tâm, thay vì đặt vào góc
                cellView.transform.localPosition = centerPos;

                // 4. Đồng bộ kích thước Visual 
         
                cellView.SetVisualSize(cellSize);

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
        // Chuyển từ World Space về Local Space của GridView
        // Điều này cực kỳ quan trọng nếu Grid của bạn không nằm ở (0,0) hoặc bị xoay/scale
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        // Giả sử cellSize là 1. Nếu khác 1, bạn cần chia cho cellSize.
        // Mathf.FloorToInt giúp làm tròn xuống để lấy chỉ số mảng chuẩn xác
        int x = Mathf.FloorToInt(localPos.x);
        int y = Mathf.FloorToInt(localPos.y);

        return new Vector2Int(x, y);
    }
    private Vector3 GetLocalCenterOfCell(int x, int y)
    {
        Vector3 origin = GetLocalPosition(x, y);
        Vector3 halfSize = new Vector3(cellSize * 0.5f, cellSize * 0.5f, 0);
        return origin + halfSize;
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
        return transform.TransformPoint(GetLocalCenterOfCell(x, y));
    }
    /// <summary>
    /// Tính toán vị trí LOCAL trên Board   
    /// </summary>
    public Vector3 CalculateLocalCenterPosition(Vector2Int startGridPos, int size, bool isHorizontal)
    {
       
        return GetLocalCenterOfCell(startGridPos.x, startGridPos.y);
    }
    /// <summary>
    /// Chuyển đổi tọa độ Grid (Vector2Int) sang World Position (Vector3).
    /// </summary>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        
        return GetWorldPosition(gridPos.x, gridPos.y);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() // Chỉ vẽ khi click vào GridView
    {
        if (!Application.isPlaying || _cellViews == null) return;

        Gizmos.color = new Color(0, 1, 1, 0.3f);
        // Thay vì vẽ từng ô, hãy vẽ WireCube bao quanh toàn bộ Grid cho nhẹ
        Vector3 center = GetLocalCenterOfCell(_cellViews.GetLength(0) / 2, _cellViews.GetLength(1) / 2);
        Vector3 size = new Vector3(_cellViews.GetLength(0) * cellSize, _cellViews.GetLength(1) * cellSize, 1);
        Gizmos.DrawWireCube(transform.TransformPoint(center), size);
    }
#endif

}