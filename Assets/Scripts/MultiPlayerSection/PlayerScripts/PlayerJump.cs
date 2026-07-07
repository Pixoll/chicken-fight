using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.PlayerScripts {
    [RequireComponent(typeof(AudioSource))]
    public class PlayerJump : NetworkBehaviour {
        [Header("Configuración del Salto")] 
        [SerializeField] private float jumpForce = 5f;

        [Header("Físicas de Gravedad Dinámica")]
        [SerializeField] private float gravedadBase = 1f;
        [SerializeField] private float multiplicadorCaida = 3f;

        [Header("Referencias De Colisión")] 
        [SerializeField] private Collider2D groundCheckCollider;

        [Header("Audio del Salto (🌟 NUEVO)")]
        [SerializeField] private AudioClip sonidoSalto;
        [Range(0f, 1f)] [SerializeField] private float volumenSalto = 0.7f;

        private Rigidbody2D _rb;
        private PlayerInputHandler _inputHandler;
        private AudioSource _audioSource;

        private int _groundLayerMask;
        private bool _isGrounded;
        private bool _jumpRequested;

        private void Awake() {
            _rb = GetComponentInParent<Rigidbody2D>();
            _inputHandler = transform.root.GetComponentInChildren<PlayerInputHandler>();
            _audioSource = GetComponent<AudioSource>();
            _groundLayerMask = LayerMask.GetMask("Ground");
        }

        private void Update() {
            if (!IsOwner || _inputHandler == null) return;

            if (_inputHandler.IsJumpPressedThisFrame()) {
                _jumpRequested = true;
            }
        }

        private void FixedUpdate() {
            if (!IsOwner) return;

            CheckGroundedOverlap();
            AjustarGravedadDeCaida();

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

            ReproducirAudioSaltoLocal();
            SolicitarAudioSaltoServerRpc();
        }

        private void ReproducirAudioSaltoLocal()
        {
            if (sonidoSalto != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(sonidoSalto, volumenSalto);
            }
        }

        [ServerRpc]
        private void SolicitarAudioSaltoServerRpc()
        {
            SincronizarAudioSaltoRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void SincronizarAudioSaltoRpc()
        {
            if (IsOwner) return;
            ReproducirAudioSaltoLocal();
        }

        private void AjustarGravedadDeCaida() {
            if (_rb == null) return;

            if (_rb.linearVelocity.y < -0.1f && !_isGrounded) {
                _rb.gravityScale = multiplicadorCaida;
            }
            else {
                _rb.gravityScale = gravedadBase;
            }
        }

        public bool IsGrounded => _isGrounded;
        public float VerticalVelocity => _rb != null ? _rb.linearVelocity.y : 0f;
    }
}
