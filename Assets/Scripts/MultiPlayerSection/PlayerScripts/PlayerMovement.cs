using MultiPlayerSection.GameplayScripts;
using MultiPlayerSection.NetworkScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.PlayerScripts
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

        private float _slowEndTime;
        private float _currentSlowIntensity = 1f;

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

            if (IsOwner)
            {
                FightMenuSectionController uiController = FindFirstObjectByType<FightMenuSectionController>();
                PlayerInputHandler handler = transform.root.GetComponentInChildren<PlayerInputHandler>();
                if (uiController != null && handler != null)
                {
                    uiController.VincularGallinaLocal(handler);
                }
            }

            MatchInformationManager manager = FindFirstObjectByType<MatchInformationManager>();
            if (manager != null)
            {
                if (!manager.EstaLaRondaActiva())
                {
                    SpriteRenderer[] renderers = transform.root.GetComponentsInChildren<SpriteRenderer>(true);
                    foreach (var sr in renderers) sr.enabled = false;

                    Canvas[] uiLocales = transform.root.GetComponentsInChildren<Canvas>(true);
                    foreach (var canvas in uiLocales) canvas.enabled = false;

                    Rigidbody2D rb = transform.root.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.bodyType = RigidbodyType2D.Kinematic;
                        rb.linearVelocity = Vector2.zero;
                    }
                }
            }
        }

        private void Update()
        {
            if (!IsOwner || _inputHandler == null) return;

            if (Time.time >= _slowEndTime)
            {
                _currentSlowIntensity = 1f;
            }

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
            
            if (_rb) 
            {
                float velocidadCalculada = _horizontalMove * moveSpeed * _currentSlowIntensity;
                _rb.linearVelocity = new Vector2(velocidadCalculada, _rb.linearVelocity.y);
            }
        }

        public void StunningTime(float duration) { _knockbackEndTime = Time.time + duration; }


        public void AplicarRalentizacionLocal(float intensidad, float duracion)
        {
            if (duracion <= 0f) return;

            float factorVelocidad = Mathf.Clamp01(1f - intensidad);

            if (Time.time < _slowEndTime)
            {
                if (factorVelocidad < _currentSlowIntensity) _currentSlowIntensity = factorVelocidad;
            }
            else
            {
                _currentSlowIntensity = factorVelocidad;
            }

            _slowEndTime = Time.time + duracion;
            Debug.Log($"<color=yellow>[EFECTO LOCAL] -> Ralentizado al {factorVelocidad * 100}% por {duracion}s.</color>");
        }

        private void HandleFlip() { _isFacingRight.Value = _horizontalMove switch { > 0f when !_isFacingRight.Value => true, < 0f when _isFacingRight.Value => false, var _ => _isFacingRight.Value }; }
        private void UpdateAnimation() { if (!_animator) return; _animator.SetBool(IsRunning, Mathf.Abs(_horizontalMove) > 0.05f); }
        private void UpdateOrientation(bool faceRight) { if (transform.root != null) transform.root.rotation = faceRight ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.Euler(0f, 0f, 0f); }
        private void OnOrientationChanged(bool prev, bool newVal) { UpdateOrientation(newVal); }
        public override void OnNetworkDespawn() { _isFacingRight.OnValueChanged -= OnOrientationChanged; }

        public void TeletransportarGallina(Vector3 nuevaPosicion)
        {
            if (!IsServer) return;
            TeletransportarGallinaOwnerRpc(nuevaPosicion);
        }

        [Rpc(SendTo.Owner)]
        private void TeletransportarGallinaOwnerRpc(Vector3 nuevaPosicion)
        {
            _knockbackEndTime = 0f;
            _slowEndTime = 0f;
            _currentSlowIntensity = 1f;
            _horizontalMove = 0f;

            if (_animator) _animator.SetBool(IsRunning, false);

            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }

            if (transform.root != null)
            {
                transform.root.position = nuevaPosicion;
            }

            Debug.Log($"<color=yellow>[Movement] -> Gallina local teletransportada exitosamente a: {nuevaPosicion}</color>");
        }
    }
}
