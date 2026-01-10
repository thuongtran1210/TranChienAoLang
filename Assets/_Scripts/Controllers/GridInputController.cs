using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Cần thiết để gọi EventSystem
using UnityEngine.InputSystem;


/// <summary>
/// Chịu trách nhiệm duy nhất: Phiên dịch Raw Input từ InputReader thành Grid Events.
/// Áp dụng: SRP, Mediator Pattern.
/// </summary>

public class GridInputController : MonoBehaviour
{
    [Header("Broadcasting Channels")]
    [SerializeField] private GridInputChannelSO _gridInputChannel;

    [Header("References")]
    [SerializeField] private InputReader inputReader;
    private Camera _inputCamera;

    private List<IGridLogic> _activeGrids = new List<IGridLogic>();


    private bool _isInitialized = false;

    private bool _isFireInputPending = false;
    private Vector2 _currentScreenPos;

    private Vector2Int _lastHoveredPos = new Vector2Int(-999, -999);
    private IGridLogic _lastHoveredGrid = null;

    // --- INITIALIZATION  ---
    public void RegisterGrid(IGridLogic grid)
    {
        if (!_activeGrids.Contains(grid)) _activeGrids.Add(grid);
    }

    public void UnregisterGrid(IGridLogic grid) 
    {
        if (_activeGrids.Contains(grid)) _activeGrids.Remove(grid);
    }

    public void Initialize(Camera cameraToUse)
    {
        _inputCamera = cameraToUse;
        _isInitialized = (_inputCamera != null && inputReader != null && _gridInputChannel != null);

        if (_gridInputChannel == null)
            Debug.LogError("GridInputChannelSO is missing in GridInputController!");
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

    // --- INPUT HANDLERS ---

    private void HandleMove(Vector2 screenPos)
    {
        if (!_isInitialized) return;
        _currentScreenPos = screenPos;
        Vector3 worldPos = GetMouseWorldPosition(screenPos);

        // 1. Báo cáo vị trí chuột (cho VFX, Ghost, etc.)
        _gridInputChannel.RaisePointerPositionChanged(worldPos);

        bool foundGrid = false;

        foreach (IGridLogic grid in _activeGrids)
        {
            if (grid.IsWorldPositionInside(worldPos, out Vector2Int gridPos))
            {
                // TỐI ƯU: Chỉ raise khi tọa độ hoặc Grid thay đổi
                if (gridPos != _lastHoveredPos || grid != _lastHoveredGrid)
                {
                    _lastHoveredPos = gridPos;
                    _lastHoveredGrid = grid;
                    _gridInputChannel.RaiseGridCellHovered(gridPos, grid);
                }
                foundGrid = true;
                return; // Tìm thấy rồi thì thoát luôn
            }
        }

        // Nếu không tìm thấy grid nào và trước đó đang hover 
        if (!foundGrid && _lastHoveredGrid != null)
        {
            _lastHoveredPos = new Vector2Int(-999, -999);
            _lastHoveredGrid = null;
            _gridInputChannel.RaiseGridCellHovered(_lastHoveredPos, null);
        }
    }

    private void HandleFireInput()
    {
        if (!_isInitialized) return;
        _isFireInputPending = true;
    }

    private void HandleRotateInput() => _gridInputChannel.RaiseRightClick();

    // --- GAME LOGIC (PROCESSING) ---

    private void ProcessFireLogic()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 worldPos = GetMouseWorldPosition(_currentScreenPos);

        foreach (IGridLogic grid in _activeGrids)
        {
            if (grid.IsWorldPositionInside(worldPos, out Vector2Int gridPos))
            {
                _gridInputChannel.RaiseGridCellClicked(gridPos, grid.GridOwner);
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