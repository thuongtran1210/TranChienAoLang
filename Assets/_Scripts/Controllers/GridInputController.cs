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

    private List<IGridLogic> _managedGrids = new List<IGridLogic>();

    public event Action<Vector2Int, IGridLogic> OnGridCellClicked;
    public event Action<Vector3> OnPointerPositionChanged;
    public event Action<Vector2Int, IGridLogic> OnGridCellHovered;
    public event Action OnRightClick;

    private bool _isInitialized = false;

    private bool _isFireInputPending = false;
    private Vector2 _currentScreenPos;

    // --- INITIALIZATION  ---
    public void RegisterGrid(IGridLogic grid)
    {
        if (!_managedGrids.Contains(grid))
        {
            _managedGrids.Add(grid);
            
        }
    }

    public void Initialize(Camera cameraToUse)
    {
        _inputCamera = cameraToUse;

        if (_inputCamera == null)
        {
            _isInitialized = false;
            return;
        }

        if (inputReader == null)
        {
           
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
        _currentScreenPos = screenPos;
        Vector3 worldPos = GetMouseWorldPosition(screenPos);
        OnPointerPositionChanged?.Invoke(worldPos);

        // --- ADDED: Check Hover Grid ---
        foreach (IGridLogic grid in _managedGrids)
        {
            // Kiểm tra xem chuột có đang nằm trên Grid này không
            if (grid.IsWorldPositionInside(worldPos, out Vector2Int gridPos))
            {
                // Bắn event kèm theo Grid đang được hover
                OnGridCellHovered?.Invoke(gridPos, grid);
                return;
            }
        }
        // Nếu không hover grid nào
        OnGridCellHovered?.Invoke(new Vector2Int(-999, -999), null);
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
            return;
        }

        // 2. Lấy vị trí chuột trong World
        Vector3 worldPos = GetMouseWorldPosition(_currentScreenPos);

        // 3. Duyệt qua danh sách các Grid
        foreach (IGridLogic grid in _managedGrids)
        {
            if (grid.IsWorldPositionInside(worldPos, out Vector2Int gridPos))
            {
                Debug.Log($"Click vào {grid.GridOwner} tại {gridPos}");
                OnGridCellClicked?.Invoke(gridPos, grid);
                return;
            }
        }
    }

    // --- UTILS  ---

    private Vector3 GetMouseWorldPosition(Vector2 screenPos)
    {
        if (_inputCamera == null) return Vector3.zero;
       
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, -_inputCamera.transform.position.z);
        return _inputCamera.ScreenToWorldPoint(screenPosWithZ);
    }

    public Vector3 GetCurrentMouseWorldPosition()
    {
        return GetMouseWorldPosition(_currentScreenPos);
    }
}