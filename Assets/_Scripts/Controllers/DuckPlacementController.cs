using UnityEngine;

public class DuckPlacementController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FleetManager fleetManager;
    [SerializeField] private GhostDuck ghostDuck;
    [SerializeField] private InputReader inputReader; 

    // State
    private bool _isPlacingShip = false;

    private void OnEnable()
    {
        // 1. Đăng ký lắng nghe sự kiện từ FleetManager
        if (fleetManager != null)
        {
            fleetManager.OnShipSelected += HandleShipSelected;
            fleetManager.OnFleetEmpty += HandleFleetEmpty;
            fleetManager.OnFleetChanged += HandleFleetChanged;
        }

        // 2. Đăng ký lắng nghe Input (Xoay tàu)
        if (inputReader != null)
        {
            inputReader.RotateEvent += HandleRotateInput;
            inputReader.MoveEvent += HandleMoveInput;
            // inputReader.OnClickEvent += HandleClick; // Xử lý đặt tàu sau
        }
    }

    private void OnDisable()
    { 
        if (fleetManager != null)
        {
            fleetManager.OnShipSelected -= HandleShipSelected;
            fleetManager.OnFleetEmpty -= HandleFleetEmpty;
            fleetManager.OnFleetChanged -= HandleFleetChanged;
        }

        if (inputReader != null)
        {
            inputReader.RotateEvent -= HandleRotateInput;
            inputReader.MoveEvent -= HandleMoveInput;
        }
    }

    // --- EVENT HANDLERS ---

    private void HandleShipSelected(DuckDataSO shipData)
    {
       
        _isPlacingShip = true;
        ghostDuck.Show(shipData);
        Debug.Log($"PlacementController: Bắt đầu đặt tàu {shipData.duckName}");
    }

    private void HandleRotateInput()
    {
        
        if (_isPlacingShip)
        {
            ghostDuck.Rotate();
        }
    }

    private void HandleMoveInput(Vector2 screenPosition)
    {
        if (_isPlacingShip)
        {

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10));
            worldPos.z = 0; 

     
            ghostDuck.SetPosition(worldPos);
        }
    }

    private void HandleFleetEmpty()
    {
        _isPlacingShip = false;
        ghostDuck.Hide();
    }

    private void HandleFleetChanged(System.Collections.Generic.List<DuckDataSO> list)
    {
      
        if (fleetManager.GetSelectedDuck() == null)
        {
            _isPlacingShip = false;
            ghostDuck.Hide();
        }
    }
}