using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class GridView : MonoBehaviour
{
    [Header("Grid Config")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellSpacing = 0.1f;

    [Header("Visual Settings")]
    [SerializeField] private Vector3 originPosition; // Local x ,y của ô (0,0)

    [Header("References")]
    [SerializeField] private GridCellView cellPrefab;
    [SerializeField] private Transform gridContainer;

    private GridCellView[,] _cellViews;

    private List<Vector2Int> _currentHighlights = new List<Vector2Int>();
    private GridSystem _gridSystem;

    /// <summary>
    /// Khởi tạo bàn cờ và đăng ký sự kiện.
    /// </summary>
    public void InitializeBoard(int width, int height, GridSystem gridSystem, Owner owner)
    {
        ClearBoard();
        _gridSystem = gridSystem;

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

                // 3. Đặt View vào tâm
                cellView.transform.localPosition = centerPos;

                // 4. Đồng bộ kích thước Visual 
         
                cellView.SetVisualSize(cellSize);

                GridCell cellLogic = gridSystem.GetCell(new Vector2Int(x, y));

                cellView.Setup(cellLogic);

                _cellViews[x, y] = cellView;
            }
        }
    }
    /// <summary>
    /// Xử lý khi Model thông báo có thay đổi (Bắn trúng/trượt)
    /// </summary>
    private void HandleGridStateChanged(Vector2Int position, ShotResult result)
    {
        if (IsValidPosition(position))
        {
            // O(1) Access - Cực nhanh
            GridCellView view = _cellViews[position.x, position.y];
            view.UpdateVisual(result);
        }
    }
    /// <summary>
    /// Dọn dẹp sạch sẽ các object cũ và hủy đăng ký sự kiện
    /// </summary>
    public void ClearBoard()
    {
        // Unsubscribe Event để tránh lỗi MissingReferenceException
        if (_gridSystem != null)
        {
            _gridSystem.OnGridStateChanged -= HandleGridStateChanged;
            _gridSystem = null;
        }

        // Xóa visual cũ
        if (_cellViews != null)
        {
            foreach (var cellView in _cellViews)
            {
                if (cellView != null) Destroy(cellView.gameObject);
            }
            _cellViews = null;
        }

        // Nếu gridContainer có con lạ (do editor), xóa hết
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
    }

    // Bảo vệ Memory Leak khi GameObject này bị hủy
    private void OnDestroy()
    {
        ClearBoard();
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
            view.Bind(data, isHorizontal);
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
    /// <summary>
    /// Hàm làm sáng các ô dựa trên danh sách vị trí truyền vào
    /// </summary>
    public void HighlightCells(List<Vector2Int> positions, Color color)
    {
        // 1. Xóa highlight cũ trước khi vẽ mới
        ClearHighlights();

        // 2. Duyệt qua danh sách và bật màu
        foreach (var pos in positions)
        {
            // Kiểm tra xem tọa độ có nằm trong bảng không
            if (IsValidPosition(pos))
            {
                // Gọi hàm SetHighlightState vừa viết ở Bước 1
                _cellViews[pos.x, pos.y].SetHighlightState(true, color);

                // Lưu lại vào list để quản lý
                _currentHighlights.Add(pos);
            }
        }
    }
    /// <summary>
    /// Hàm tắt toàn bộ highlight
    /// </summary>
    public void ClearHighlights()
    {
        foreach (var pos in _currentHighlights)
        {
            if (IsValidPosition(pos))
            {
                // Trả về màu trắng bình thường
                _cellViews[pos.x, pos.y].SetHighlightState(false, Color.white);
            }
        }
        // Xóa danh sách lưu trữ
        _currentHighlights.Clear();
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