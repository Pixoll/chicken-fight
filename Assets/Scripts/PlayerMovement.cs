using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour {
    public enum StartingDirection {
        Left,
        Right
    }

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private StartingDirection startingDirection = StartingDirection.Left;

    private Rigidbody2D _rb;
    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;
    private Vector3 _originalScale;
    private Joystick _joystick;
    private PlayerJump _playerJump;
    private SpriteAnimator _animator;

    // for later
    private NetworkVariable<int> _health = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _inputActions = new PlayerInputActions();
        _originalScale = transform.localScale;
        _joystick = FindFirstObjectByType<Joystick>();
        _playerJump = GetComponent<PlayerJump>();
        _animator = GetComponentInChildren<SpriteAnimator>();
    }

    public override void OnNetworkSpawn() {
        SetInitialOrientation();
    }

    private void OnEnable() => _inputActions.Player.Enable();

    private void OnDisable() => _inputActions.Player.Disable();

    private void Update() {
        if (!IsOwner) return;

        _moveInput = _inputActions.Player.Move.ReadValue<Vector2>().normalized;

        if (_moveInput == Vector2.zero) {
            _moveInput = _joystick.Direction;
        }

        FlipCharacter();

        UpdateAnimationState();
    }

    private void FixedUpdate() {
        if (!IsOwner) return;

        _rb.linearVelocity = new Vector2(_moveInput.x * moveSpeed, _rb.linearVelocity.y);
    }

    private void SetInitialOrientation() {
        if (startingDirection == StartingDirection.Right) {
            transform.localScale = new Vector3(-_originalScale.x, _originalScale.y, _originalScale.z);
        } else {
            transform.localScale = _originalScale;
        }
    }

    private void FlipCharacter() {
        if (_moveInput.x > 0.1f) {
            transform.localScale = new Vector3(-_originalScale.x, _originalScale.y, _originalScale.z);
        } else if (_moveInput.x < -0.1f) {
            transform.localScale = _originalScale;
        }
    }

    private void UpdateAnimationState() {
        if (!_animator) return;

        bool isMovingHorizontally = Mathf.Abs(_moveInput.x) > 0.1f;

        bool isGrounded = _playerJump && _playerJump.IsGrounded;

        if (isMovingHorizontally && isGrounded) {
            _animator.ChangeState(SpriteAnimator.AnimationState.Run);
        } else {
            _animator.ChangeState(SpriteAnimator.AnimationState.Idle);
        }
    }
}
