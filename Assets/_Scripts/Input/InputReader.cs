using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Duck Battle/Input Reader")]
public class InputReader : ScriptableObject, InputSystem_Actions.IGameInputActions
{
    private InputSystem_Actions _inputActions;

    public event Action<Vector2> MoveEvent;
    public event Action FireEvent;
    public event Action RotateEvent;
    public event Action OnClickEvent;

    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new InputSystem_Actions();
            _inputActions.GameInput.SetCallbacks(this);
        }
        _inputActions.GameInput.Enable();
    }
    private void OnDisable()
    {
        _inputActions.GameInput.Disable();
    }


    // Interface Implementation
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed) FireEvent?.Invoke();
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            RotateEvent?.Invoke();
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            OnClickEvent?.Invoke();
    }
}