using UnityEngine;
using System; // Để dùng Action

public class GridManager : MonoBehaviour, IGridContext
{
    [Header("Dependencies")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private GridInputController inputController;
    [SerializeField] private GridView gridView;
    [SerializeField] private GhostDuck ghostDuck;

    [Header("Settings")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;

    [SerializeField] private LayerMask gridLayer;

    // --- CORE DATA ---
    private IGridSystem _gridSystem;

    // --- EVENTS  ---
    public event Action<IGridContext, Vector2Int> OnGridClicked;

    // --- INTERFACE IMPLEMENTATION ---
    public IGridSystem GridSystem => _gridSystem;

    public DuckDataSO SelectedDuck => null; 

    public GridInputController InputController => inputController;
    public bool IsGhostHorizontal => ghostDuck != null && ghostDuck.IsHorizontal;
    public Owner GridOwner { get; private set; }

    // --- INITIALIZATION ---
    public void Initialize(IGridSystem gridSystem, Owner owner)
    {
        _gridSystem = gridSystem;
        GridOwner = owner;

        cameraController.SetupCamera(width, height);

        // Inject Camera dependency
        inputController.Initialize(cameraController.GetCamera());

        // 3. Truyền Owner xuống View để View setup từng Cell (như đã bàn ở bước View)
        // Lưu ý: Bạn cần cập nhật hàm InitializeBoard bên GridView để nhận thêm tham số 'owner'
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

        // 2. Factory: Tạo instance DuckUnit (Pure C#)
        DuckUnit newDuck = new DuckUnit(data, isHorizontal);

        // 3. Cập nhật Data Model (GridSystem)
        _gridSystem.PlaceUnit(newDuck, gridPos, isHorizontal);

        // 4. Cập nhật View (GridView)
        gridView.SpawnDuck(gridPos, isHorizontal, data, newDuck);

        Debug.Log($"[GridManager] Đã đặt {data.duckName} tại {gridPos}");
        return true;
    }
    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        // Gọi xuống View để lấy World Pos chuẩn
        return gridView.GridToWorldPosition(gridPos);
    }

    // --- INPUT HANDLING ---
    private void HandleCellClicked(Vector2Int gridPos, Owner clickedOwner)
    {
        if (clickedOwner != this.GridOwner)
        {
            return; // Click nhầm sang bảng khác -> Bỏ qua ngay lập tức
        }
        Debug.Log($"[GridManager] Valid click on {GridOwner} board at {gridPos}");
        OnGridClicked?.Invoke(this, gridPos);
    }

    // --- INTERFACE VISUAL METHODS  ---

    public void OnDuckPlacedSuccess(DuckUnit unit, Vector2Int pos)
    {
        gridView.SpawnDuck(pos, unit.IsHorizontal, unit.Data, unit);
        HideGhost();
    }

    public void OnSetupPhaseCompleted()
    {
        HideGhost();
    }

    public Vector2Int GetGridPosition(Vector3 worldPos) => gridView.WorldToGridPosition(worldPos);

    public void UpdateGhostPosition(Vector3 worldPos)
    {
        if (ghostDuck != null) ghostDuck.SetPosition(worldPos);
    }

    public void SetGhostValidation(bool isValid)
    {
        if (ghostDuck != null) ghostDuck.SetValidationState(isValid);
    }

    public void ToggleGhostRotation()
    {
        if (ghostDuck != null) ghostDuck.Rotate();
    }

    public void HideGhost()
    {
        if (ghostDuck != null) ghostDuck.Hide();
    }
    public void ShowGhost(DuckDataSO data)
    {
        if (ghostDuck != null)
        {
            ghostDuck.Show(data);
            ghostDuck.gameObject.SetActive(true);
        }
    }

}