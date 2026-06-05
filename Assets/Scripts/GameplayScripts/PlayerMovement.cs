using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts
{
    public class PlayerMovement : NetworkBehaviour {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");

        public enum StartingDirection {
            Left,
            Right
        }

        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private StartingDirection startingDirection = StartingDirection.Left;
        [SerializeField] private Animator animator;

        private Rigidbody2D _rb;
        private PlayerInputActions _inputActions;
        private Vector2 _moveInput;
        private Vector3 _originalScale;
        private Joystick _joystick;
        private PlayerJump _playerJump;

        private NetworkVariable<bool> _isFacingRight = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _inputActions = new PlayerInputActions();
            _originalScale = transform.localScale;
            _joystick = FindFirstObjectByType<Joystick>();
            _playerJump = GetComponent<PlayerJump>();
        }

        public override void OnNetworkSpawn() {
            SetInitialOrientation();
            _isFacingRight.OnValueChanged += UpdateOrientation;
        }

        private void OnEnable() => _inputActions.Player.Enable();

        private void OnDisable() => _inputActions.Player.Disable();

        private void Update() {
            // Si somos el dueño, leemos el control local
            if (IsOwner) {
                _moveInput = _inputActions.Player.Move.ReadValue<Vector2>().normalized;
                if (_moveInput == Vector2.zero) {
                    _moveInput = _joystick.Direction;
                }

                FlipCharacter();
            }

            // ESTO LO EJECUTAN TODOS: Así el maniquí o clones pueden apagar su animación
            UpdateAnimationState();
        }

        private void FixedUpdate() {
            if (!IsOwner) return;

            _rb.linearVelocity = new Vector2(_moveInput.x * moveSpeed, _rb.linearVelocity.y);
        }

        private void SetInitialOrientation() {
            // if (startingDirection == StartingDirection.Right) {
            //     transform.localScale = new Vector3(-_originalScale.x, _originalScale.y, _originalScale.z);
            // } else {
            //     transform.localScale = _originalScale;
            // }
            bool facingRight = startingDirection == StartingDirection.Right;
            _isFacingRight.Value = facingRight;
            UpdateOrientation(false, facingRight);
        }

        private void FlipCharacter() {
            // if (_moveInput.x > 0.1f) {
            //     transform.localScale = new Vector3(-_originalScale.x, _originalScale.y, _originalScale.z);
            // } else if (_moveInput.x < -0.1f) {
            //     transform.localScale = _originalScale;
            // }
            bool shouldFaceRight = _moveInput.x > 0.1f;
            bool shouldFaceLeft = _moveInput.x < -0.1f;

            if (shouldFaceRight && !_isFacingRight.Value) {
                _isFacingRight.Value = true;
            } else if (shouldFaceLeft && _isFacingRight.Value) {
                _isFacingRight.Value = false;
            }
        }

        private void UpdateOrientation(bool previousValue, bool newValue) {
            transform.localScale = newValue
                ? new Vector3(-_originalScale.x, _originalScale.y, _originalScale.z)
                : _originalScale;
        }

        private void UpdateAnimationState() {
            bool isMovingHorizontally;

            if (IsOwner) {
                // Si es nuestro propio personaje, dependemos ÚNICAMENTE del input del teclado/joystick.
                // Si no tocamos nada (_moveInput.x == 0), se detiene de inmediato.
                isMovingHorizontally = Mathf.Abs(_moveInput.x) > 0.01f;
            } else {
                // Si es el clon/maniquí visto desde otra pantalla, dependemos de su velocidad en red.
                // Subimos el umbral a 0.2f para absorber cualquier imprecisión física o retraso de red.
                isMovingHorizontally = Mathf.Abs(_rb.linearVelocity.x) > 0.2f;
            }

            // Verificamos si está tocando el suelo
            bool isGrounded = _playerJump && _playerJump.IsGrounded;

            if (animator != null) {
                // Solo corre si hay movimiento real Y está en el suelo
                animator.SetBool(IsRunning, isMovingHorizontally && isGrounded);
            }
        }
    }
}
