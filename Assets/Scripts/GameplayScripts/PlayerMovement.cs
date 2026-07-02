using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts
{
    public class PlayerMovement : NetworkBehaviour
    {
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");

        [Header("Configuración de Movimiento")] 
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D _rb;
        private PlayerInputHandler _inputHandler;
        private float _horizontalMove;
        private Animator _animator;
        private float _knockbackEndTime; 

        private readonly NetworkVariable<bool> _isFacingRight = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private void Awake()
        {
            _rb = GetComponentInParent<Rigidbody2D>();
            _inputHandler = transform.root.GetComponentInChildren<PlayerInputHandler>();
            _animator = transform.root.GetComponentInChildren<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            _isFacingRight.OnValueChanged += OnOrientationChanged;
            UpdateOrientation(_isFacingRight.Value);

            // 获取 NetworkObject 
            ulong miId = OwnerClientId;
            
            Debug.Log($"<color=white>[PLAYER SPAWN] Gallina de Red Spawneda en esta ventana. ID Cliente Dueño: {miId} | ¿Es dueña de esta pantalla (IsOwner)?: {IsOwner}</color>");

            if (IsOwner)
            {
                FightMenuSectionController uiController = FindFirstObjectByType<FightMenuSectionController>();
                if (uiController != null)
                {
                    Debug.Log($"<color=cyan>[PLAYER SPAWN] Gallina dueña (ID: {miId}) encontró el FightMenuSectionController con éxito. Enviando credenciales...</color>");
                    uiController.VincularGallinaLocal(_inputHandler);
                }
                else
                {
                    Debug.LogError($"<color=red>[PLAYER SPAWN ERROR] ¡Gallina dueña (ID: {miId}) NO encontró ningún FightMenuSectionController en la escena!</color>");
                }
            }
        }

        private void Update()
        {
            if (!IsOwner || _inputHandler == null) return;

            if (Time.time < _knockbackEndTime)
            {
                _horizontalMove = 0f;
                return;
            }

            _horizontalMove = _inputHandler.GetHorizontalInput();
            HandleFlip();
            UpdateAnimation();
        }

        private void FixedUpdate()
        {
            if (!IsOwner || Time.time < _knockbackEndTime) return;
            if (_rb) _rb.linearVelocity = new Vector2(_horizontalMove * moveSpeed, _rb.linearVelocity.y);
        }

        public void StunningTime(float duration) { _knockbackEndTime = Time.time + duration; }
        private void HandleFlip() { _isFacingRight.Value = _horizontalMove switch { > 0f when !_isFacingRight.Value => true, < 0f when _isFacingRight.Value => false, var _ => _isFacingRight.Value }; }
        private void UpdateAnimation() { if (!_animator) return; _animator.SetBool(IsRunning, Mathf.Abs(_horizontalMove) > 0.05f); }
        private void UpdateOrientation(bool faceRight) { if (transform.root != null) transform.root.rotation = faceRight ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 0f, 0f); }
        private void OnOrientationChanged(bool prev, bool newVal) { UpdateOrientation(newVal); }
        public override void OnNetworkDespawn() { _isFacingRight.OnValueChanged -= OnOrientationChanged; }
    }
}
