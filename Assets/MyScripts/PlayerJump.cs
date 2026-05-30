using UnityEngine;

namespace MyScripts {
    public class PlayerJump : MonoBehaviour {
        [SerializeField] private float jumpForce = 5f;

        private Rigidbody2D _rb;
        private Collider2D _collider;
        private PlayerInputActions _inputActions;

        private int _groundLayerMask;
        private bool _isGrounded;
        private bool _jumpRequested;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();

            _inputActions = new PlayerInputActions();
            _groundLayerMask = LayerMask.GetMask("Ground");
        }

        private void OnEnable() => _inputActions.Player.Jump.Enable();
        private void OnDisable() => _inputActions.Player.Jump.Disable();

        private void Update() {
            if (_inputActions.Player.Jump.WasPressedThisFrame()) {
                _jumpRequested = true;
            }
        }

        private void FixedUpdate() {
            CheckGroundedOverlap();

            if (_jumpRequested) {
                ExecuteJump();
                _jumpRequested = false;
            }
        }

        private void CheckGroundedOverlap() {
            Bounds bounds = _collider.bounds;
            Vector2 overlapCenter = new Vector2(bounds.center.x, bounds.center.y - bounds.extents.y - 0.02f);
            Vector2 overlapSize = new Vector2(bounds.size.x * 0.9f, 0.05f);

            Collider2D hit = Physics2D.OverlapBox(overlapCenter, overlapSize, 0f, _groundLayerMask);
            _isGrounded = hit;
        }

        private void ExecuteJump() {
            if (_isGrounded) {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0);
                _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                _isGrounded = false;
            }
        }

        public bool IsGrounded => _isGrounded;
    }
}
