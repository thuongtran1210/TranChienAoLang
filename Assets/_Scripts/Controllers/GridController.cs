using UnityEngine;
using System; 

public class GridController : MonoBehaviour, IGridContext
{
    [Header("Dependencies")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GridInputController inputController;
    [SerializeField] private GridView gridView;
    [SerializeField] private GhostDuck ghostDuck;

    [Header("Settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 1f;

    [SerializeField] private LayerMask gridLayer;

    // --- CORE DATA ---
    private IGridSystem _gridSystem;

    // --- EVENTS  ---
    public event Action<IGridLogic, Vector2Int> OnGridClicked;

    // --- INTERFACE IMPLEMENTATION ---
    public IGridSystem GridSystem => _gridSystem;

    public DuckDataSO SelectedDuck => null; 

    public GridInputController InputController => inputController;
    public Owner GridOwner { get; private set; }



    // --- INITIALIZATION ---
    public void Initialize(IGridSystem gridSystem, Owner owner)
    {
        _gridSystem = gridSystem;
        GridOwner = owner;
        cameraController.SetupCamera(width, height);
        inputController.Initialize(cameraController.GetCamera());
        gridView.InitializeBoard(width, height, (GridSystem)_gridSystem, owner);
    }

    private void OnEnable()
    {
        inputController.OnGridCellClicked += HandleCellClicked;
    }

    private void OnDisable()
    {
        if (inputController != null)
        {
            inputController.OnGridCellClicked -= HandleCellClicked;
        }
        if (_gridSystem != null) _gridSystem.OnGridStateChanged -= gridView.UpdateCellState;
    }

    private void BindGridEvents()
    {
        if (_gridSystem != null)
        {
            _gridSystem.OnGridStateChanged -= gridView.UpdateCellState;
            _gridSystem.OnGridStateChanged += gridView.UpdateCellState;
        }
    }

    // ========================================================================
    // LOGIC ĐẶT TÀU (Placement Logic)
    // ========================================================================

    /// <summary>
    /// Kiểm tra xem vị trí và hướng xoay có hợp lệ không.
    /// </summary>
    public bool IsPlacementValid(Vector3 worldPos, DuckDataSO data, bool isHorizontal)
    {
        if (_gridSystem == null || data == null) return false;
        Vector2Int gridPos = gridView.WorldToGridPosition(worldPos);
        return _gridSystem.CanPlaceUnit(data, gridPos, isHorizontal);
    }
    /// <summary>
    /// Thực hiện hành động đặt tàu.
    /// Trả về true nếu thành công.
    /// </summary>
    public bool TryPlaceShip(Vector3 worldPos, DuckDataSO data, bool isHorizontal)
    {
        // 1. Validate lại lần cuối (Double-check pattern)
        if (!IsPlacementValid(worldPos, data, isHorizontal))
        {
            Debug.LogWarning("[GridManager] Vị trí đặt tàu không hợp lệ!");
            return false;
        }

        Vector2Int gridPos = gridView.WorldToGridPosition(worldPos);

        // 2. Factory: Tạo instance DuckUnit
        DuckUnit newDuck = new DuckUnit(data, isHorizontal);

        // 3. Cập nhật Data Model (GridSystem)
        _gridSystem.PlaceUnit(newDuck, gridPos, isHorizontal);

        // 4. Cập nhật View (GridView)
        gridView.SpawnDuck(gridPos, isHorizontal, data, newDuck);

        Debug.Log($"[GridManager] Đã đặt {data.duckName} tại {gridPos}");
        return true;
    }

    // --- HELPER METHODS ---
    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        // Tính toán vị trí World dựa trên Grid Index
        float offset = cellSize * 0.5f;

        Vector3 localPos = new Vector3(
            (gridPos.x * cellSize) + offset,
            (gridPos.y * cellSize) + offset,
            0
        );

        return transform.TransformPoint(localPos);
    }
    public bool IsWorldPositionInside(Vector3 worldPos, out Vector2Int gridPos)
    {
        gridPos = Vector2Int.zero;

        // 1. Chuyển World -> Local
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // 2.Tính toán dựa trên cellSize 
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);

        // 3. Kiểm tra biên
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            gridPos = new Vector2Int(x, y);
            return true;
        }

        return false;
    }

    // --- INPUT HANDLING ---
    private void HandleCellClicked(Vector2Int gridPos, Owner clickedOwner)
    {
        if (clickedOwner != this.GridOwner) return;


        OnGridClicked?.Invoke(this, gridPos);
    }

    // --- INTERFACE VISUAL METHODS  ---

    public Vector2Int GetGridPosition(Vector3 worldPos)
    {
    
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);

        return new Vector2Int(x, y);
    }


    #region IGridGhostHandler Implementation

    // Property: Trả về trạng thái xoay của Ghost
    public bool IsGhostHorizontal
    {
        get
        {
            // Senior Tip: Đừng return true cứng, hãy hỏi thẳng object GhostDuck
            if (ghostDuck != null) return ghostDuck.IsHorizontal;
            return true; // Default safety
        }
    }

    public void ShowGhost(DuckDataSO data)
    {
        if (ghostDuck != null)
        {
            ghostDuck.gameObject.SetActive(true);
            ghostDuck.Show(data);
        }
    }

    public void HideGhost()
    {
        // Dùng ?. để code gọn hơn: "Nếu ghostDuck không null thì gọi Hide()"
        ghostDuck?.Hide();
    }

    public void UpdateGhostPosition(Vector3 worldPos)
    {
        ghostDuck?.SetPosition(worldPos);
    }

    public void SetGhostValidation(bool isValid)
    {
        ghostDuck?.SetValidationState(isValid);
    }

    public void ToggleGhostRotation()
    {
        ghostDuck?.Rotate();
    }

    // Hàm này được gọi khi đặt vịt thành công (SetupState gọi) -> Cập nhật View thật
    public void OnDuckPlacedSuccess(DuckUnit unit, Vector2Int pos)
    {
        // Gọi GridView để spawn con vịt thật
        gridView.SpawnDuck(pos, unit.IsHorizontal, unit.Data, unit);

        // Ẩn ghost đi để chuẩn bị cho con tiếp theo (nếu cần)
        HideGhost();
    }

    public void OnSetupPhaseCompleted()
    {
        // Khi xong phase Setup, chắc chắn phải ẩn Ghost
        HideGhost();
        Debug.Log($"[{GridOwner}] Setup Phase Completed.");
    }

    #endregion


}