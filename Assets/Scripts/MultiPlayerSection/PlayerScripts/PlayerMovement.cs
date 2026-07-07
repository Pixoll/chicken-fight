using MultiPlayerSection.GameplayScripts;
using MultiPlayerSection.NetworkScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.PlayerScripts
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerMovement : NetworkBehaviour
    {
        private static readonly int IsRunning = Animator.StringToHash("IsRunning");
        private static readonly int IsJump = Animator.StringToHash("IsJump");
        private static readonly int IsFall = Animator.StringToHash("IsFall");
        private static readonly int IsPushed = Animator.StringToHash("IsPushed");
        private static readonly int IsStill = Animator.StringToHash("IsStill");

        [Header("Configuración de Movimiento")] 
        [SerializeField] private float moveSpeed = 5f;

        [Header("Audio de Pasos")]
        [SerializeField] private AudioClip sonidoPasoCarrera;
        [Range(0f, 1f)] [SerializeField] private float volumenPasos = 0.5f;
        [Tooltip("Tiempo en segundos que tarda en repetirse cada pisada al correr")]
        [SerializeField] private float intervaloEntrePasos = 0.35f;

        [Header("Audio de Impacto / Aturdimiento (NUEVO)")]
        [SerializeField] private AudioClip sonidoStun;
        [Range(0f, 1f)] [SerializeField] private float volumenStun = 0.8f;

        private Rigidbody2D _rb;
        private PlayerInputHandler _inputHandler;
        private PlayerJump _playerJump; 
        private AudioSource _audioSource;
        private float _horizontalMove;
        private Animator _animator;
        private float _knockbackEndTime; 
        private float _slowEndTime;
        private float _currentSlowIntensity = 1f;
        private float _nextFootstepTime;

        private readonly NetworkVariable<bool> _isFacingRight = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private void Awake()
        {
            _rb = GetComponentInParent<Rigidbody2D>();
            _inputHandler = transform.root.GetComponentInChildren<PlayerInputHandler>();
            _animator = transform.root.GetComponentInChildren<Animator>();
            _playerJump = transform.root.GetComponentInChildren<PlayerJump>();
            _audioSource = GetComponent<AudioSource>();
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
            if (manager != null && !manager.EstaLaRondaActiva())
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

        private void Update()
        {
            ManejarAudioDePasosLocal();

            if (!IsOwner || _inputHandler == null) return;

            if (Time.time >= _slowEndTime) _currentSlowIntensity = 1f;

            if (Time.time < _knockbackEndTime)
            {
                _horizontalMove = 0f;
                UpdateAnimation();
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

        private void ManejarAudioDePasosLocal()
        {
            if (_animator == null || sonidoPasoCarrera == null || _audioSource == null) return;

            bool estaCorriendo = _animator.GetBool(IsRunning);

            if (estaCorriendo && Time.time >= _nextFootstepTime)
            {
                _audioSource.PlayOneShot(sonidoPasoCarrera, volumenPasos);
                _nextFootstepTime = Time.time + intervaloEntrePasos;
            }
        }

        public void StunningTime(float duration) 
        { 
            _knockbackEndTime = Time.time + duration; 

            if (sonidoStun != null && _audioSource != null && duration > 0f)
            {
                _audioSource.PlayOneShot(sonidoStun, volumenStun);
            }
        }

        public void AplicarRalentizacionLocal(float intensidad, float duracion)
        {
            if (duracion <= 0f) return;
            float factorVelocidad = Mathf.Clamp01(1f - intensidad);

            if (Time.time < _slowEndTime)
            {
                if (factorVelocidad < _currentSlowIntensity) _currentSlowIntensity = factorVelocidad;
            }
            else _currentSlowIntensity = factorVelocidad;

            _slowEndTime = Time.time + duracion;
        }

        private void HandleFlip() { _isFacingRight.Value = _horizontalMove switch { > 0f when !_isFacingRight.Value => true, < 0f when _isFacingRight.Value => false, var _ => _isFacingRight.Value }; }

        private void UpdateAnimation() 
        { 
            if (!_animator) return;

            bool running = false;
            bool jump = false;
            bool fall = false;
            bool pushed = false;
            bool still = false;

            if (Time.time < _knockbackEndTime) pushed = true;
            else if (_playerJump != null)
            {
                bool tocandoSuelo = _playerJump.IsGrounded;
                float velocidadY = _playerJump.VerticalVelocity;
                bool moviendoJoystick = Mathf.Abs(_horizontalMove) > 0.05f;

                if (tocandoSuelo)
                {
                    if (moviendoJoystick) running = true;
                    else still = true;
                }
                else 
                {
                    if (velocidadY < -0.1f) fall = true;
                    else jump = true;
                }
            }
            else still = true;

            _animator.SetBool(IsPushed, pushed);
            _animator.SetBool(IsRunning, running);
            _animator.SetBool(IsJump, jump);
            _animator.SetBool(IsFall, fall);
            _animator.SetBool(IsStill, still);
        }

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

            if (_animator) 
            {
                _animator.SetBool(IsRunning, false);
                _animator.SetBool(IsStill, true);
            }

            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }

            if (transform.root != null) transform.root.position = nuevaPosicion;
        }
        public bool IsPushedActive => Time.time < _knockbackEndTime;
    }
}
