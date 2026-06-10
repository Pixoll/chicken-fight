using GameplayScripts.PlayerImpactsSection;
using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts {
    public class PlayerMovement : NetworkBehaviour {
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");

        [Header("Movement settings")] [SerializeField]
        private float moveSpeed = 5f;

        private Rigidbody2D _rb;
        private PlayerInputHandler _inputHandler;
        private float _horizontalMove;

        private Vector3 _originalVisualScale;
        private Vector3 _originalCombatScale;

        private Animator _animator;
        private Transform _combatFolder;

        private readonly NetworkVariable<bool> _isFacingRight = new(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private void Awake() {
            _rb = GetComponentInParent<Rigidbody2D>();

            _inputHandler = transform.root.GetComponentInChildren<PlayerInputHandler>();
            _animator = transform.root.GetComponentInChildren<Animator>();

            var combatManager = transform.root.GetComponentInChildren<PlayerImpactManager>();
            _combatFolder = combatManager.transform;

            if (_animator != null) {
                _originalVisualScale = _animator.transform.localScale;
            }

            if (_combatFolder != null) {
                _originalCombatScale = _combatFolder.localScale;
            }
        }

        private void UpdateSpriteScale(bool faceRight) {
            if (_animator) {
                _animator.transform.localScale = faceRight
                    ? new Vector3(-_originalVisualScale.x, _originalVisualScale.y, _originalVisualScale.z)
                    : _originalVisualScale;
            }

            if (_combatFolder) {
                _combatFolder.localScale = faceRight
                    ? new Vector3(-_originalCombatScale.x, _originalCombatScale.y, _originalCombatScale.z)
                    : _originalCombatScale;
            }
        }

        public override void OnNetworkSpawn() {
            _isFacingRight.OnValueChanged += OnOrientationChanged;
            UpdateSpriteScale(_isFacingRight.Value);
        }

        private void Update() {
            if (!IsOwner) return;

            _horizontalMove = _inputHandler.GetHorizontalInput();

            HandleFlip();
            UpdateAnimation();
        }

        private void FixedUpdate() {
            if (!IsOwner) return;

            _rb.linearVelocity = new Vector2(_horizontalMove * moveSpeed, _rb.linearVelocity.y);
        }

        private void HandleFlip() {
            _isFacingRight.Value = _horizontalMove switch {
                > 0f when !_isFacingRight.Value => true,
                < 0f when _isFacingRight.Value => false,
                var _ => _isFacingRight.Value
            };
        }

        private void UpdateAnimation() {
            if (!_animator) return;

            bool isRunning = Mathf.Abs(_horizontalMove) > 0.05f;
            _animator.SetBool(IsRunning, isRunning);
        }

        private void OnOrientationChanged(bool previousValue, bool newValue) => UpdateSpriteScale(newValue);
    }
}
