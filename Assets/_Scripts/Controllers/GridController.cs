using UnityEngine;
using System;
using System.Collections.Generic;
/// <summary>
/// Controller điều phối cho một Grid cụ thể.
/// </summary>

public class GridController : MonoBehaviour, IGridContext
{
    [Header("Dependencies")]
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private GridInputController _inputController;

    [SerializeField] private UnitVisualManager _unitVisualManager;
    [SerializeField] private GhostDuckView _ghostDuck;
    [SerializeField] private TilemapGridView _tilemapGridView;

    [Header("Settings")]
    [SerializeField] Owner _gridOwner;
    [SerializeField] private int _width = 10;
    [SerializeField] private int _height = 10;

    [Header("EVENTS CHANEL")]
    [SerializeField] private BattleEventChannelSO _battleChannel;
    [SerializeField] private GridInputChannelSO _gridInputChannel;

    // --- CORE DATA ---
    private IGridSystem _gridSystem;
    public Owner GridOwner => _gridOwner;
    public IGridSystem GridSystem => _gridSystem;

    // --- EVENTS  ---
    public event Action<IGridLogic, Vector2Int> OnGridClicked;

    // --- INTERFACE IMPLEMENTATION ---
    public DuckDataSO SelectedDuck => null; 

    public GridInputController InputController => _inputController;
    public bool IsGhostHorizontal => _ghostDuck != null && _ghostDuck.IsHorizontal;




    // --- INITIALIZATION ---
    public void Initialize(IGridSystem gridSystem, Owner owner)
    {
        _gridSystem = gridSystem;
        _gridOwner = owner;

        // DI Setup
        _cameraController.SetupCamera(_width, _height);
        _inputController.Initialize(_cameraController.GetCamera());

        // Init View Systems
        _tilemapGridView.InitializeBoard(_width, _height, (GridSystem)_gridSystem, owner);
        _unitVisualManager.Initialize(_tilemapGridView); 

    }

    private void OnEnable()
    {
        if (_inputController != null) _inputController.RegisterGrid(this);
        _gridInputChannel.OnGridCellClicked += HandleCellClicked;

        if (_battleChannel != null)
        {
            _battleChannel.OnGridHighlightRequested += HandleHighlightRequest;
            _battleChannel.OnGridHighlightClearRequested += _tilemapGridView.ClearHighlights; // Đổi sang Tilemap
        }
        // GridSystem event đã được TilemapGridView tự đăng ký trong InitializeBoard
    }

    private void OnDisable()
    {
        if (_inputController != null) _inputController.UnregisterGrid(this);
        _gridInputChannel.OnGridCellClicked -= HandleCellClicked;

        if (_battleChannel != null)
        {
            _battleChannel.OnGridHighlightRequested -= HandleHighlightRequest;
            _battleChannel.OnGridHighlightClearRequested -= _tilemapGridView.ClearHighlights;
        }
    }

    // --- INPUT HANDLING ---
    private void HandleCellClicked(Vector2Int gridPos, Owner clickedOwner)
    {
        if (clickedOwner != this.GridOwner) return;
        OnGridClicked?.Invoke(this, gridPos);
    }

    // --- LOGIC: PLACEMENT & CONVERSION ---

    /// <summary>
    /// Thực hiện hành động đặt tàu.
    /// Trả về true nếu thành công.
    /// </summary>
    public bool TryPlaceShip(Vector3 worldPos, DuckDataSO data, bool isHorizontal)
    {
        if (!IsPlacementValid(worldPos, data, isHorizontal)) return false;

        // Sử dụng Tilemap để tính toán tọa độ Grid
        Vector2Int gridPos = _tilemapGridView.WorldToGridPosition(worldPos);

        DuckUnit newDuck = new DuckUnit(data, gridPos, isHorizontal);
        _gridSystem.PlaceUnit(newDuck, gridPos, isHorizontal);

        // Gọi UnitVisualManager để spawn GameObject con vịt
        _unitVisualManager.SpawnDuck(gridPos, isHorizontal, data);

        return true;
    }
    /// <summary>
    /// Kiểm tra xem vị trí và hướng xoay có hợp lệ không.
    /// </summary>
    public bool IsPlacementValid(Vector3 worldPos, DuckDataSO data, bool isHorizontal)
    {
        if (_gridSystem == null || data == null) return false;
        Vector2Int gridPos = _tilemapGridView.WorldToGridPosition(worldPos);
        return _gridSystem.CanPlaceUnit(data, gridPos, isHorizontal);
    }
    private void HandleHighlightRequest(Owner target, List<Vector2Int> positions, Color color)
    {
        if (target != this.GridOwner) return;

        // Gọi TilemapGridView để highlight
        _tilemapGridView.HighlightCells(positions, color);

        StopAllCoroutines();
        StartCoroutine(AutoClearHighlightDelay(2f));
    }
    private System.Collections.IEnumerator AutoClearHighlightDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _tilemapGridView.ClearHighlights();
    }

    // --- HELPER METHODS ---
    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        return _tilemapGridView.GetWorldCenterPosition(gridPos);
    }
    public bool IsWorldPositionInside(Vector3 worldPos, out Vector2Int gridPos)
    {
        gridPos = _tilemapGridView.WorldToGridPosition(worldPos);
        return _gridSystem.IsValidPosition(gridPos);
    }



    // --- INTERFACE VISUAL METHODS  ---

    public Vector2Int GetGridPosition(Vector3 worldPos)
    {
        return _tilemapGridView.WorldToGridPosition(worldPos);
    }


    #region IGridGhostHandler Implementation

    // Property: Trả về trạng thái xoay của Ghost

    public void ShowGhost(DuckDataSO data)
    {
        if (_ghostDuck != null)
        {
            _ghostDuck.gameObject.SetActive(true);
            _ghostDuck.Show(data);
        }
    }

    public void HideGhost()
    {
        _ghostDuck?.Hide();
    }

    public void UpdateGhostPosition(Vector3 worldPos)
    {
        _ghostDuck?.SetPosition(worldPos);
    }

    public void SetGhostValidation(bool isValid)
    {
        _ghostDuck?.SetValidationState(isValid);
    }

    public void ToggleGhostRotation()
    {
        _ghostDuck?.Rotate();
    }

    public void OnDuckPlacedSuccess(DuckUnit unit, Vector2Int pos)
    {
        _unitVisualManager.SpawnDuck(pos, unit.IsHorizontal, unit.Data);
        HideGhost();
    }

    public void OnSetupPhaseCompleted()
    {
        HideGhost();
    }

    #endregion


}