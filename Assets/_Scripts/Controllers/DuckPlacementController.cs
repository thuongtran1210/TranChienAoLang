using UnityEngine;
using System.Collections.Generic; // Để dùng List<>

public class DuckPlacementController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FleetManager fleetManager;
    [SerializeField] private GameObject gridManagerObject; 
    private IGridLogic _gridLogic;
    [SerializeField] private GhostDuck ghostDuck;
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Camera _mainCamera;

    private bool _isPlacingShip = false;
    private DuckDataSO _currentDuckData;

    private void Awake()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (gridManagerObject != null)
            _gridLogic = gridManagerObject.GetComponent<IGridLogic>();
    }

    private void OnEnable()
    {
        if (fleetManager != null)
        {
            fleetManager.OnShipSelected += HandleShipSelected;
            fleetManager.OnFleetEmpty += StopPlacement;
            fleetManager.OnFleetChanged += HandleFleetChanged;
        }
        if (inputReader != null)
        {
            inputReader.RotateEvent += HandleRotateInput;
            inputReader.MoveEvent += HandleMoveInput;
            inputReader.OnClickEvent += HandleClickInput;
        }
    }

    private void OnDisable()
    {
        if (fleetManager != null)
        {
            fleetManager.OnShipSelected -= HandleShipSelected;
            fleetManager.OnFleetEmpty -= StopPlacement;
            fleetManager.OnFleetChanged -= HandleFleetChanged;
        }
        if (inputReader != null)
        {
            inputReader.RotateEvent -= HandleRotateInput;
            inputReader.MoveEvent -= HandleMoveInput;
            inputReader.OnClickEvent -= HandleClickInput;
        }
    }

    // --- LOGIC FLOW ---

    private void HandleShipSelected(DuckDataSO shipData)
    {
        _isPlacingShip = true;
        _currentDuckData = shipData;
        ghostDuck.Show(shipData); // Controller trực tiếp gọi Ghost
    }

    private void HandleMoveInput(Vector2 screenPosition)
    {
        if (!_isPlacingShip || _currentDuckData == null) return;

        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10));
        worldPos.z = 0;

        // Snap logic: Vẫn dùng GridManager để tính toán tọa độ
        Vector2Int gridPos = _gridLogic.GetGridPosition(worldPos);
        Vector3 snappedPos = _gridLogic.GetWorldPosition(gridPos);

        // Update Visual
        ghostDuck.SetPosition(snappedPos);

        // Check luật
        bool isValid = _gridLogic.IsPlacementValid(snappedPos, _currentDuckData, ghostDuck.IsHorizontal);
        ghostDuck.SetValidationState(isValid);
    }

    private void HandleRotateInput()
    {
        if (!_isPlacingShip) return;

        ghostDuck.Rotate(); // Controller tự xoay Ghost

        // Re-validate ngay lập tức sau khi xoay (UX tốt hơn)
        // Chúng ta giả lập lại sự kiện di chuyển chuột tại chỗ để cập nhật màu sắc
        Vector3 currentWorldPos = ghostDuck.transform.position;
        bool isValid = _gridLogic.IsPlacementValid(currentWorldPos, _currentDuckData, ghostDuck.IsHorizontal);
        ghostDuck.SetValidationState(isValid);
    }

    private void HandleClickInput()
    {
        if (!_isPlacingShip || _currentDuckData == null) return;

        Vector3 currentPos = ghostDuck.transform.position;
        bool isHorizontal = ghostDuck.IsHorizontal; // Lấy dữ liệu từ Ghost

        // Gửi lệnh xuống Model (GridManager)
        bool success = _gridLogic.TryPlaceShip(currentPos, _currentDuckData, isHorizontal);

        if (success)
        {
            fleetManager.OnShipPlacedSuccess();

            // [MỚI] Controller chịu trách nhiệm ẩn Ghost sau khi đặt xong
            // (Nếu game cho phép đặt liên tiếp, logic ở HandleFleetChanged sẽ quyết định có hiện lại hay không)
            ghostDuck.Hide();
        }
        else
        {
            Debug.Log("Không thể đặt tàu tại đây!");
        }
    }

    private void HandleFleetChanged(List<DuckDataSO> list)
    {
        DuckDataSO selected = fleetManager.GetSelectedDuck();
        // Nếu không còn vịt đang chọn trong kho, dừng đặt
        if (selected == null || selected != _currentDuckData)
        {
            StopPlacement();
        }
        // Nếu vẫn còn, Ghost sẽ tiếp tục hiển thị (vì chúng ta chưa gọi StopPlacement)
        // hoặc logic FleetManager sẽ kích hoạt lại HandleShipSelected.
    }

    private void StopPlacement()
    {
        _isPlacingShip = false;
        _currentDuckData = null;
        ghostDuck.Hide(); // Controller ẩn Ghost
    }
}