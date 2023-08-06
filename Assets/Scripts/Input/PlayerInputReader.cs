using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInput _playerInput;
    private PlayerInput.GameplayActions _gameplay;
    private Vector3 _currentTouchPosition;
    [SerializeField] private Game _game;

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _gameplay = _playerInput.Gameplay;

        _gameplay.TouchPosition.performed += ctx => ProcessTouchPosition(ctx);
        _gameplay.Tap.performed += ctx => ProcessTap();
    }

    private void ProcessTouchPosition(InputAction.CallbackContext ctx)
    {
        _currentTouchPosition = ctx.ReadValue<Vector2>();
    }

    private void ProcessTap()
    {
        _game.Click(_currentTouchPosition);
    }

    private void OnEnable()
    {
        _gameplay.Enable();
    }

    private void OnDisable()
    {
        _gameplay.Disable();
    }

}
