using UnityEngine;
using static UnityEditor.Rendering.ShadowCascadeGUI;

public class SetupState : GameStateBase
{
    // Dependencies 
    private IGridContext _playerGrid;
    private FleetManager _fleetManager;
    private GridInputController _inputController;
    private DuckDataSO _selectedDuckData;

    public SetupState(IGameContext context, IGridContext playerGrid, FleetManager fleetManager, GridInputController inputController)
                : base(context)
    {
        _playerGrid = playerGrid;
        _fleetManager = fleetManager;
        _inputController = inputController;
    }

    public override void EnterState()
    {
        Debug.Log("--- ENTER SETUP STATE ---");

        _fleetManager.OnShipSelected += HandleDuckSelected;
        _fleetManager.OnFleetEmpty += HandleFleetEmpty;
        _inputController.OnPointerPositionChanged += HandlePointerPositionChanged;
        _inputController.OnRightClick += HandleRotate;

        _inputController.OnGridCellClicked += HandleGridInput;
    }

    public override void ExitState()
    {
        _fleetManager.OnShipSelected -= HandleDuckSelected;
        _fleetManager.OnFleetEmpty -= HandleFleetEmpty;

        if (_inputController != null)
        {
            _inputController.OnPointerPositionChanged -= HandlePointerPositionChanged;
            _inputController.OnRightClick -= HandleRotate;
            _inputController.OnGridCellClicked -= HandleGridInput;
        }

        _playerGrid.HideGhost();
    }

    private void HandleGridInput(Vector2Int gridPos, Owner owner)
    {
        Debug.Log($"[DEBUG] SetupState Received Click: {gridPos}, Owner: {owner}");
        // 1. Validate Input
        if (owner != Owner.Player)
        {
            Debug.LogWarning("Chỉ được đặt tàu lên bảng của Player!");
            return;
        }

        // 2. Validate State Data
        if (_selectedDuckData == null)
        {
            // Có thể thêm logic: Nếu click vào tàu đã đặt -> Nhấc tàu lên (Edit mode)
            return;
        }

        // 3. Thực hiện hành động
        AttemptPlaceDuck(gridPos);
    }
    public override void OnGridInteraction(IGridContext source, Vector2Int gridPos)
    {
        if (source.GridOwner != Owner.Player) return;

        DuckDataSO selectedDuck = _fleetManager.GetSelectedDuck();

        if (selectedDuck != null)
        { 
            AttemptPlaceDuck(gridPos);
        }
    }
    private void AttemptPlaceDuck(Vector2Int gridPos)
    {
        bool isHorizontal = _playerGrid.IsGhostHorizontal;

        // Sử dụng GridSystem (Model) để check logic
        if (_playerGrid.GridSystem.CanPlaceUnit(_selectedDuckData, gridPos, isHorizontal))
        {
            // A. Cập nhật Model
            DuckUnit newUnit = new DuckUnit(_selectedDuckData, isHorizontal);
            _playerGrid.GridSystem.PlaceUnit(newUnit, gridPos, isHorizontal);

            // B. Cập nhật View
            _playerGrid.OnDuckPlacedSuccess(newUnit, gridPos);
            _fleetManager.OnShipPlacedSuccess();

            // C. Reset State cho lần đặt tiếp theo
            _selectedDuckData = null;

            // Quan trọng: Sau khi đặt xong, phải ẩn Ghost ngay hoặc reset input
            _playerGrid.HideGhost();
        }
        else
        {
            // Feedback Visual/Audio khi đặt lỗi
            Debug.Log("Vị trí không hợp lệ!");
            // _audioService.PlayErrorSound();
        }
    }

    // --- EVENT HANDLERS ---
    private void HandleDuckSelected(DuckDataSO data)
    {
        _selectedDuckData = data;
        Debug.Log($"SetupState: Picked duck {data.duckName}");

        // 1. Hiển thị GhostDuck
        _playerGrid.ShowGhost(data);


        Vector3 currentWorldPos = _inputController.GetCurrentMouseWorldPosition();

        _playerGrid.UpdateGhostPosition(currentWorldPos);

        Vector2Int gridPos = _playerGrid.GetGridPosition(currentWorldPos);
        bool isValid = _playerGrid.GridSystem.CanPlaceUnit(_selectedDuckData, gridPos, _playerGrid.IsGhostHorizontal);
        _playerGrid.SetGhostValidation(isValid);
    }

    private void HandleFleetEmpty()
    {
        Debug.Log("Setup Completed!");  
        _playerGrid.OnSetupPhaseCompleted();
        _gameContext.EndSetupPhase();
    }

    private void HandleClick()
    {
        if (_selectedDuckData == null) return;

    }
    private void HandlePointerPositionChanged(Vector3 worldPos)
    {
        if (_selectedDuckData == null) return;


        Vector2Int gridPos = _playerGrid.GetGridPosition(worldPos);


        Vector3 snapPos = _playerGrid.GetWorldPosition(gridPos);

  
        _playerGrid.UpdateGhostPosition(snapPos);


        bool isValid = _playerGrid.GridSystem.CanPlaceUnit(_selectedDuckData, gridPos, _playerGrid.IsGhostHorizontal);
        _playerGrid.SetGhostValidation(isValid);
    }
    private void HandleRotate()
    {
        if (_selectedDuckData == null) return;

        _playerGrid.ToggleGhostRotation(); 
        Vector3 currentWorldPos = _inputController.GetCurrentMouseWorldPosition(); 
        HandlePointerPositionChanged(currentWorldPos);
    }
}