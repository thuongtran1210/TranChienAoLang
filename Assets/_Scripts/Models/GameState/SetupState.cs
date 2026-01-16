using UnityEngine;
using static UnityEditor.Rendering.ShadowCascadeGUI;

public class SetupState : GameStateBase
{
    // Dependencies 
    private IGridContext _playerGrid; 
    private FleetManager _fleetManager;
    private GridInputChannelSO _gridInputChannel;
    private GridInputController _inputController;
    private DuckDataSO _selectedDuckData;

    public SetupState(IGameContext gameContext,
                          IGridContext playerGrid,
                          FleetManager fleetManager,
                          GridInputController inputController, 
                          GridInputChannelSO gridInputChannel) 
                : base(gameContext)
    {
        _playerGrid = playerGrid;
        _fleetManager = fleetManager;
        _inputController = inputController;
        _gridInputChannel = gridInputChannel;
    }

    public override void EnterState()
    {
        Debug.Log("--- ENTER SETUP STATE ---");
        CleanupEvents();

        _fleetManager.OnDuckSelected += HandleDuckSelected;
        _fleetManager.OnFleetEmpty += HandleFleetEmpty;
        _gridInputChannel.OnPointerPositionChanged += HandlePointerPositionChanged;
        _gridInputChannel.OnRotateAction += HandleRotate;
        _gridInputChannel.OnGridCellClicked += HandleGridInput;
    }

    public override void ExitState()
    {
        CleanupEvents();

        _fleetManager.OnDuckSelected -= HandleDuckSelected;
        _fleetManager.OnFleetEmpty -= HandleFleetEmpty;

        if (_gridInputChannel != null)
        {
            _gridInputChannel.OnPointerPositionChanged -= HandlePointerPositionChanged;
            _gridInputChannel.OnRotateAction -= HandleRotate;
            _gridInputChannel.OnGridCellClicked -= HandleGridInput;
        }

        _playerGrid.HideGhost();
    }
    private void CleanupEvents()
    {
        _fleetManager.OnDuckSelected -= HandleDuckSelected;
        _fleetManager.OnFleetEmpty -= HandleFleetEmpty;

        if (_gridInputChannel != null)
        {
            _gridInputChannel.OnPointerPositionChanged -= HandlePointerPositionChanged;
            _gridInputChannel.OnRotateAction -= HandleRotate;
            _gridInputChannel.OnGridCellClicked -= HandleGridInput;
        }
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
        // 1. Kiểm tra xem _playerGrid có phải là GridController không để dùng tính năng cao cấp
        if (_playerGrid is GridController gridController)
        {
            // 2. Lấy vị trí World để truyền vào TryPlaceShip (Vì TryPlaceShip nhận Vector3)
            // Senior Tip: Controller nên lo việc validate, State chỉ gửi yêu cầu.
            Vector3 worldPos = gridController.GetWorldPosition(gridPos);
            bool isHorizontal = _playerGrid.IsGhostHorizontal;
            // 3. GỌI HÀM VÀ HỨNG KẾT QUẢ (QUAN TRỌNG NHẤT)
            bool isSuccess = gridController.TryPlaceDuck(worldPos, _selectedDuckData, isHorizontal);

            // 4. Kiểm tra kết quả
            if (isSuccess)
            {
                // Chỉ trừ tàu khi Controller xác nhận đặt thành công
                _fleetManager.OnDuckPlacedSuccess();

                Debug.Log($"[SetupState] Đặt thành công {_selectedDuckData.duckName}");

                // Reset trạng thái chọn
                _selectedDuckData = null;
                _playerGrid.HideGhost();
            }
            else
            {
                // Logic thất bại: Controller đã log warning rồi, ở đây ta có thể play sound
                Debug.Log("[SetupState] Đặt thất bại - Controller trả về False");
                // audioManager.PlayErrorSound(); 
            }
        }
        else
        {
            Debug.LogError("[SetupState] _playerGrid không phải là GridController! Kiểm tra lại DI.");
        }
    }

    // --- EVENT HANDLERS ---
    private void HandleDuckSelected(DuckDataSO data)
    {
        _selectedDuckData = data;

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
        if (_selectedDuckData == null)
        {
            Debug.LogWarning("Không thể xoay Ghost khi chưa chọn tàu!");
            return;
        }

        _playerGrid.ToggleGhostRotation(); 
        Vector3 currentWorldPos = _inputController.GetCurrentMouseWorldPosition(); 
        HandlePointerPositionChanged(currentWorldPos);
    }
}