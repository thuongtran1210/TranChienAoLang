using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Cần thiết để gọi EventSystem
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

    // --- STATE MANAGEMENT ---
    // Thay vì xử lý ngay, chúng ta dùng biến này để đánh dấu sự kiện
    private bool _isFireInputPending = false;
    private Vector2 _currentScreenPos;

    // --- INITIALIZATION (Giữ nguyên) ---
    public void RegisterGrid(GridManager grid)
    {
        if (!_managedGrids.Contains(grid))
        {
            _managedGrids.Add(grid);
            Debug.Log($"[GridInputController] Đã đăng ký Grid: {grid.GridOwner}");
        }
    }

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
        inputReader.FireEvent += HandleFireInput; 
        inputReader.RotateEvent += HandleRotateInput;
    }

    private void OnDisable()
    {
        if (inputReader == null) return;
        inputReader.MoveEvent -= HandleMove;
        inputReader.FireEvent -= HandleFireInput;
        inputReader.RotateEvent -= HandleRotateInput;
    }

    private void Update()
    {
        if (!_isInitialized) return;

        if (_isFireInputPending)
        {
            ProcessFireLogic();
            _isFireInputPending = false; 
        }
    }

    // --- INPUT HANDLERS (SIGNAL ONLY) ---

    private void HandleMove(Vector2 screenPos)
    {
        if (!_isInitialized) return;

        // 1. Cập nhật vị trí màn hình (cho logic polling nếu cần)
        _currentScreenPos = screenPos;

        // 2. Chuyển đổi sang World Position
        Vector3 worldPos = GetMouseWorldPosition(screenPos);

        // 3. [QUAN TRỌNG] Bắn event để GhostDuck hoặc các hệ thống khác biết để cập nhật vị trí
        OnPointerPositionChanged?.Invoke(worldPos);
    }

    private void HandleFireInput()
    {
        if (!_isInitialized) return;
        _isFireInputPending = true;
    }

    private void HandleRotateInput() => OnRightClick?.Invoke();

    // --- GAME LOGIC (PROCESSING) ---

    private void ProcessFireLogic()
    {
        // 1. Chặn click xuyên UI

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Blocked by UI");
            return;
        }

        // 2. Lấy vị trí chuột trong World
        Vector3 worldPos = GetMouseWorldPosition(_currentScreenPos);

        // 3. Duyệt qua danh sách các Grid
        foreach (var grid in _managedGrids)
        {
            if (grid.IsWorldPositionInside(worldPos, out Vector2Int gridPos))
            {
                Debug.Log($"Click vào {grid.GridOwner} tại {gridPos}");
                OnGridCellClicked?.Invoke(gridPos, grid.GridOwner);
                return;
            }
        }
    }

    // --- UTILS (Giữ nguyên) ---

    private Vector3 GetMouseWorldPosition(Vector2 screenPos)
    {
        if (_inputCamera == null) return Vector3.zero;
        // Lưu ý: InputSystem trả về Vector2, cần đảm bảo Z distance phù hợp với Camera
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, -_inputCamera.transform.position.z);
        return _inputCamera.ScreenToWorldPoint(screenPosWithZ);
    }

    public Vector3 GetCurrentMouseWorldPosition()
    {
        return GetMouseWorldPosition(_currentScreenPos);
    }
}