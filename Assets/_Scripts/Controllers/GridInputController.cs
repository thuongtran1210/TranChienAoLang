using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    private Camera _inputCamera;


    private List<GridManager> _managedGrids = new List<GridManager>();

    public event Action<Vector2Int, Owner> OnGridCellClicked;
    public event Action<Vector3> OnPointerPositionChanged;
    public event Action OnRightClick;

    private bool _isInitialized = false;
    bool _fireInputReceived = true;
    private Vector2 _currentScreenPos;

    // --- INITIALIZATION ---

    /// <summary>
    /// Hàm này được gọi từ GameManager để đăng ký PlayerGrid và EnemyGrid
    /// </summary>
    public void RegisterGrid(GridManager grid)
    {
        if (!_managedGrids.Contains(grid))
        {
            _managedGrids.Add(grid);
            Debug.Log($"[GridInputController] Đã đăng ký Grid: {grid.GridOwner}");
        }
    }
    /// <summary>
    /// Inject dependency Camera. 
    /// </summary>
    public void Initialize(Camera cameraToUse)
    {
        _inputCamera = cameraToUse;

        if (_inputCamera == null)
        {
            Debug.LogError("[GridInputController] Camera NULL!");
            _isInitialized = false;
            return;
        }

        if (inputReader == null)
        {
            Debug.LogError("[GridInputController] Thiếu InputReader!");
            _isInitialized = false;
            return;
        }

        _isInitialized = true;
    }

    // --- UNITY EVENTS ---

    private void OnEnable()
    {
        if (inputReader == null) return;
        inputReader.MoveEvent += HandleMove;
        inputReader.FireEvent += HandleFire;
        inputReader.RotateEvent += HandleRotateInput;
    }

    private void OnDisable()
    {
        if (inputReader == null) return;
        inputReader.MoveEvent -= HandleMove;
        inputReader.FireEvent -= HandleFire;
        inputReader.RotateEvent -= HandleRotateInput;
    }
    // --- INPUT HANDLERS (PURE MATH LOGIC) ---

    private void HandleMove(Vector2 screenPos)
    {
        if (!_isInitialized) return;
        _currentScreenPos = screenPos;

    }

    private void HandleFire()
    {
        if (!_isInitialized) return;

        // 1. Chặn click xuyên UI (nếu dùng EventSystem)
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 2. Lấy vị trí chuột trong World
        Vector3 worldPos = GetMouseWorldPosition(_currentScreenPos);

        // 3. Duyệt qua danh sách các Grid đã đăng ký (Player & Enemy)
        foreach (var grid in _managedGrids)
        {
            // Gọi hàm IsWorldPositionInside (Bạn cần đảm bảo đã thêm hàm này vào GridManager như hướng dẫn trước)
            if (grid.IsWorldPositionInside(worldPos, out Vector2Int gridPos))
            {
                // [FOUND IT] Tìm thấy Grid mà chuột đang trỏ vào!
                // grid.GridOwner sẽ cho biết đó là Player hay Enemy
                Debug.Log($"Click vào {grid.GridOwner} tại {gridPos}");

                OnGridCellClicked?.Invoke(gridPos, grid.GridOwner);
                return; // Đã tìm thấy thì thoát vòng lặp ngay
            }
        }

        // Nếu chạy hết vòng lặp mà không return -> Click vào khoảng không
        // Debug.Log("Click trượt ra ngoài các bảng đấu.");
    }

    private void HandleRotateInput() => OnRightClick?.Invoke();

    // --- UTILS ---

    private Vector3 GetMouseWorldPosition(Vector2 screenPos)
    {
        if (_inputCamera == null) return Vector3.zero;
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, -_inputCamera.transform.position.z);
        return _inputCamera.ScreenToWorldPoint(screenPosWithZ);
    }
    // --- PUBLIC API 

    /// <summary>
    /// Trả về vị trí World hiện tại của chuột (Z = 0 trên mặt phẳng Grid).
    /// Dùng cho các State cần poll vị trí chuột (như khi nhấn R để xoay tại chỗ).
    /// </summary>
    public Vector3 GetCurrentMouseWorldPosition()
    {
        // _currentScreenPos là biến private đã có trong code cũ, được update tại HandleMove
        return GetMouseWorldPosition(_currentScreenPos);
    }

}