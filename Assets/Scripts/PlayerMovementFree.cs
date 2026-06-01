using UnityEngine;

public class PlayerMovementFree : MonoBehaviour {
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;

    private PlayerInputActions _inputActions;

    private Vector2 _moveInput;

    private void Awake() {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable() {
        _inputActions.Player.Enable();
    }

    private void OnDisable() {
        _inputActions.Player.Disable();
    }

    private void Update() {
        _moveInput = _inputActions.Player.Move.ReadValue<Vector2>().normalized;
    }

    private void FixedUpdate() {
        rb.linearVelocity = _moveInput * moveSpeed;
    }
}
