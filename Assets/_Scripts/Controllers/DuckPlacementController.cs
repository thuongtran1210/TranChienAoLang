using UnityEngine;

public class DuckPlacementController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FleetManager fleetManager; // Model (Inventory)
    [SerializeField] private GridManager gridManager;   // Model (Logic)
    [SerializeField] private GhostDuck ghostDuck;       // View (Visual)
    [SerializeField] private InputReader inputReader;   // Input

    [Header("Camera")]
    [SerializeField] private Camera _mainCamera;

    // Internal State
    private bool _isPlacingShip = false;
    private DuckDataSO _currentDuckData;

    private void Awake()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        // 1. Lắng nghe FleetManager (Khi chọn tàu từ UI)
        if (fleetManager != null)
        {
            fleetManager.OnShipSelected += HandleShipSelected;
            fleetManager.OnFleetEmpty += StopPlacement;
            fleetManager.OnFleetChanged += HandleFleetChanged;
        }

        // 2. Lắng nghe Input
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

    // 1. BẮT ĐẦU: Khi chọn tàu từ UI
    private void HandleShipSelected(DuckDataSO shipData)
    {
        _isPlacingShip = true;
        _currentDuckData = shipData;

      
        ghostDuck.Show(shipData);
    }

    // 2. DI CHUYỂN: Cập nhật vị trí và màu sắc (Valid/Invalid)
    private void HandleMoveInput(Vector2 screenPosition)
    {
        if (!_isPlacingShip || _currentDuckData == null) return;

        // A. Lấy vị trí chuột trong World
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10));
        worldPos.z = 0;

        // B. (Optional) Snap vị trí Ghost vào giữa ô lưới cho đẹp
        // Lấy tọa độ Grid
        Vector2Int gridPos = gridManager.GetGridPosition(worldPos);
        // Lấy lại tọa độ World chuẩn tâm ô
        Vector3 snappedPos = gridManager.GetWorldPosition(gridPos);

        // C. Cập nhật vị trí Ghost
        ghostDuck.SetPosition(snappedPos);

        // D. Hỏi GridManager: "Chỗ này hợp lệ không?"
        bool isValid = gridManager.IsPlacementValid(snappedPos, _currentDuckData, ghostDuck.IsHorizontal);

        // E. Cập nhật màu xanh/đỏ
        ghostDuck.SetValidationState(isValid);
    }

    // 3. XOAY: Xoay Visual và check lại Valid
    private void HandleRotateInput()
    {
        if (!_isPlacingShip) return;


        ghostDuck.Rotate();


    }

    // 4. ĐẶT TÀU: Khi Click chuột trái
    private void HandleClickInput()
    {
        if (!_isPlacingShip || _currentDuckData == null) return;

        Vector3 currentPos = ghostDuck.transform.position;
        bool isHorizontal = ghostDuck.IsHorizontal;

        // Gọi GridManager thực hiện đặt tàu
        bool success = gridManager.TryPlaceShip(currentPos, _currentDuckData, isHorizontal);

        if (success)
        {
            // Nếu đặt thành công:
            // 1. Trừ số lượng tàu trong kho (FleetManager)
            fleetManager.OnShipPlacedSuccess();

            // 2. Logic FleetManager sẽ bắn event check xem còn tàu không để update trạng thái
            // (Đã xử lý ở HandleFleetChanged/HandleFleetEmpty)
        }
        else
        {
            // Feedback âm thanh hoặc rung camera báo lỗi (nếu cần)
            Debug.Log("Không thể đặt tàu tại đây!");
        }
    }

    private void HandleFleetChanged(System.Collections.Generic.List<DuckDataSO> list)
    {
        // Nếu con vịt đang cầm trên tay bị hết số lượng (ví dụ logic nào đó xóa nó), hủy đặt
        DuckDataSO selected = fleetManager.GetSelectedDuck();
        if (selected == null || selected != _currentDuckData)
        {
            StopPlacement();
        }
    }

    private void StopPlacement()
    {
        _isPlacingShip = false;
        _currentDuckData = null;
        ghostDuck.Hide();
    }

    private void HandleFleetEmpty()
    {
        StopPlacement();
        Debug.Log("Đã đặt hết tàu!");
    }
}