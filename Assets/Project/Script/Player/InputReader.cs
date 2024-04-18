using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Player Input")]
public class InputReader : ScriptableObject, PlayerInputAction.IPlayerActions
{
    public event UnityAction<Vector2> moveEvent = delegate { };
    public event UnityAction<Vector2> lookEvent = delegate { };
    public event UnityAction<bool> runEvent = delegate { };
    public event UnityAction jumpEvent = delegate { };
    public event UnityAction attackEvent = delegate { };

    private PlayerInputAction _playerInputAction;

    public Vector2 Direction => _playerInputAction.Player.Move.ReadValue<Vector2>();
    public Vector3 lookDirection => (Vector3)_playerInputAction.Player.Look.ReadValue<Vector2>();

    private void OnEnable()
    {
        if(_playerInputAction == null)
        {
            _playerInputAction = new PlayerInputAction();
            _playerInputAction.Player.SetCallbacks(this);
        }

        _playerInputAction.Enable();
    }

    private void OnDisable()
    {
        if (_playerInputAction != null) _playerInputAction.Player.Disable();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            jumpEvent.Invoke();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                runEvent?.Invoke(true);
                break;
            case InputActionPhase.Canceled:
                runEvent?.Invoke(false);
                break;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        attackEvent?.Invoke();
    }
}
