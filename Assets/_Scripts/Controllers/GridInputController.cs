using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private LayerMask gridLayer;
    private Camera _inputCamera;

    public event Action<Vector2Int, Owner> OnGridCellClicked;
    public event Action<Vector3> OnPointerPositionChanged;
    public event Action OnRightClick;

    private bool _isInitialized = false;
    private Vector2 _currentScreenPos;

    [SerializeField]
    private bool _drawDebugRay = true;

    /// <summary>
    /// Inject dependency Camera. 
    /// </summary>
    public void Initialize(Camera cameraToUse)
    {
        _inputCamera = cameraToUse;

        if (_inputCamera == null)
        {
            Debug.LogError("[GridInputController] Camera được inject là NULL!");
            _isInitialized = false;
            return;
        }

        if (inputReader == null)
        {
            Debug.LogError("[GridInputController] Missing InputReader Reference!");
            _isInitialized = false;
            return;
        }

        _isInitialized = true;
    }

    private void OnEnable()
    {
        if (inputReader == null) return;
        inputReader.MoveEvent += HandleMove;     
        inputReader.FireEvent += HandleFire;
        inputReader.RotateEvent += HandleRotateInput;
    }

    private void OnDisable()
    {
        if (inputReader == null) return;
        inputReader.MoveEvent -= HandleMove;
        inputReader.FireEvent -= HandleFire;
        inputReader.RotateEvent -= HandleRotateInput;
    }
    public Vector3 GetCurrentMouseWorldPosition()
    {
        if (!_isInitialized) return Vector3.zero;
        return GetMouseWorldPosition(_currentScreenPos);
    }

    // --- EVENT HANDLERS ---

    private void HandleMove(Vector2 screenPos)
    {
        if (!_isInitialized) return;

        _currentScreenPos = screenPos; 

        Vector3 worldPos = GetMouseWorldPosition(screenPos);
        OnPointerPositionChanged?.Invoke(worldPos);

    }
    private void HandleFire()
    {
        if (!_isInitialized) return;

      
        if (TryGetInteractable(_currentScreenPos, out IGridInteractable interactable))
        {
            OnGridCellClicked?.Invoke(interactable.GridPosition, interactable.CellOwner);
        }
    }
    private bool TryGetInteractable(Vector2 screenPos, out IGridInteractable interactable)
    {
        interactable = null;
        if (_inputCamera == null) return false;

        Vector3 worldPos = GetMouseWorldPosition(screenPos);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPos, gridLayer);

        if (hitCollider != null)
        {
          
            return hitCollider.TryGetComponent(out interactable);
        }
        return false;
    }

    private void HandleRotateInput() => OnRightClick?.Invoke();

    // --- PHYSICS / LOGIC ---

    private bool TryGetGridInfo(Vector2 screenPos, out Vector2Int gridPos, out Owner owner)
    {
        gridPos = Vector2Int.zero;
        owner = default;

        if (_inputCamera == null) return false;

        Vector3 worldPos = GetMouseWorldPosition(screenPos);


        Collider2D hitCollider = Physics2D.OverlapPoint(worldPos, gridLayer);

        if (hitCollider != null && hitCollider.TryGetComponent(out GridCellView cellView))
        {

            if (cellView._cellLogic != null)
            {
                gridPos = cellView._cellLogic.GridPosition;
                owner = cellView.CellOwner;
                return true;
            }
        }
        return false;
    }

    private Vector3 GetMouseWorldPosition(Vector2 screenPos)
    {
        Vector3 screenPosWithZ = new Vector3(screenPos.x, screenPos.y, -_inputCamera.transform.position.z);
        return _inputCamera.ScreenToWorldPoint(screenPosWithZ);
    }
    private void OnDrawGizmos()
    {
        if (_inputCamera != null)
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(GetMouseWorldPosition(_currentScreenPos), 0.05f);
            }
        }
    }

}