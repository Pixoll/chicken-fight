using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.PlayerScripts {
    public class PlayerJump : NetworkBehaviour {
        [Header("Configuración del Salto")] 
        [SerializeField] private float jumpForce = 5f;

        [Header("Referencias De Colisión")] 
        [SerializeField] private Collider2D groundCheckCollider;

        private Rigidbody2D _rb;
        private PlayerInputHandler _inputHandler;

        private int _groundLayerMask;
        private bool _isGrounded;
        private bool _jumpRequested;

        private void Awake() {
            _rb = GetComponentInParent<Rigidbody2D>();
            _inputHandler = transform.root.GetComponentInChildren<PlayerInputHandler>();
            _groundLayerMask = LayerMask.GetMask("Ground");
        }

        private void Update() {
            if (!IsOwner || _inputHandler == null) return;

            if (_inputHandler.IsJumpPressedThisFrame()) {
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
            if (groundCheckCollider == null) return;

            Bounds bounds = groundCheckCollider.bounds;
            Vector2 overlapCenter = new Vector2(bounds.center.x, bounds.center.y - bounds.extents.y - 0.02f);
            Vector2 overlapSize = new Vector2(bounds.size.x * 0.9f, 0.05f);

            Collider2D hit = Physics2D.OverlapBox(overlapCenter, overlapSize, 0f, _groundLayerMask);
            _isGrounded = hit;
        }

        private void ExecuteJump() {
            if (!_isGrounded || _rb == null) return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, 0);
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _isGrounded = false;
        }

        public bool IsGrounded => _isGrounded;
    }
}
