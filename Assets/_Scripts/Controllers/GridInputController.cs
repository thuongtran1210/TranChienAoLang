using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Cần thiết để gọi EventSystem
using UnityEngine.InputSystem;


/// <summary>
/// <para><b>ROLE:</b></para>
/// <para>
/// Class này đóng vai trò là <b>Input Mediator (Trung gian xử lý Input)</b>. 
/// Nhiệm vụ duy nhất của nó là chuyển đổi các tín hiệu Input thô (Raw Input từ <see cref="InputReader"/>) 
/// thành các sự kiện có ý nghĩa trong ngữ cảnh Grid (Domain Events) như: Hover vào ô nào, Click vào ô nào.
/// </para>
/// <para><b>NGUYÊN TẮC ÁP DỤNG:</b></para>
/// <list type="bullet">
/// <item><b>Single Responsibility Principle (SRP):</b> Chỉ chịu trách nhiệm "phiên dịch" Input, không chứa logic game (như đặt tàu, tấn công).</item>
/// <item><b>Observer Pattern:</b> Lắng nghe sự kiện từ InputReader và phát lại qua GridInputChannelSO.</item>
/// </list>
/// </summary>
/// <remarks>
/// <para><b>CONSTRAINTS:</b></para>
/// <list type="number">
/// <item><b>Initialization Required:</b> BẮT BUỘC phải gọi hàm <see cref="Initialize(Camera)"/> trước khi sử dụng để tham chiếu Camera dùng cho Raycast.</item>
/// <item><b>Input System:</b> Phụ thuộc chặt chẽ vào <see cref="InputReader"/> (New Input System Wrapper).</item>
/// <item><b>UI Blocking:</b> Tự động chặn Raycast xuống Grid nếu chuột đang nằm trên UI (thông qua <see cref="EventSystem"/>).</item>
/// </list>
/// </remarks>

public class GridInputController : MonoBehaviour
{
    [Header("Broadcasting Channels")]
    [SerializeField] private GridInputChannelSO _gridInputChannel;

    [Header("References")]
    [SerializeField] private InputReader inputReader;
    private Camera _inputCamera;

    private List<IGridLogic> _managedGrids = new List<IGridLogic>();


    private bool _isInitialized = false;

    private bool _isFireInputPending = false;
    private Vector2 _currentScreenPos;

    private Vector2Int _lastHoveredPos = new Vector2Int(-999, -999);
    private IGridLogic _lastHoveredGrid = null;

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

    // --- INPUT HANDLERS (SIGNAL ONLY) ---

    private void HandleMove(Vector2 screenPos)
    {
        if (!_isInitialized) return;
        _currentScreenPos = screenPos;
        Vector3 worldPos = GetMouseWorldPosition(screenPos);

        _gridInputChannel.RaisePointerPositionChanged(worldPos);

        bool foundGrid = false;

        foreach (IGridLogic grid in _managedGrids)
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

        // Nếu không tìm thấy grid nào và trước đó đang hover (để tránh spam null liên tục)
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

        foreach (IGridLogic grid in _managedGrids)
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