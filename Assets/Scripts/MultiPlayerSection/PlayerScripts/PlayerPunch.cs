using System.Collections;
using Unity.Netcode;
using UnityEngine;
using MultiPlayerSection.GameplayScripts.Objects; 

namespace MultiPlayerSection.PlayerScripts 
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerPunch : NetworkBehaviour 
    {
        private static readonly int IsPunch = Animator.StringToHash("IsPunch");

        [Header("Referencias de Gestión")]
        [SerializeField] private PlayerObjectAttackManager objectAttackManager;

        [Header("Configuración del Ataque")] 
        [SerializeField] private float punchCooldown = 4f;
        [SerializeField] private float hitboxDuration = 1f;

        [Header("Audio del Ataque (🌟 NUEVO)")]
        [SerializeField] private AudioClip sonidoGolpe;
        [Range(0f, 1f)] [SerializeField] private float volumenGolpe = 0.8f;

        private PlayerInputHandler _inputHandler;
        private PlayerMovement _playerMovement;
        private AudioSource _audioSource;
        private float _nextPunchTime;
        private Coroutine _hitboxCoroutine;
        private Coroutine _visualCoroutine; 
        
        private Animator _animator;
        private ObjectBoxCharacteristics[] _objetosDisponibles;

        private void Awake() 
        {
            _inputHandler = transform.root.GetComponentInChildren<PlayerInputHandler>();
            _animator = transform.root.GetComponentInChildren<Animator>();
            _playerMovement = transform.root.GetComponentInChildren<PlayerMovement>();
            _audioSource = GetComponent<AudioSource>();

            if (objectAttackManager == null)
            {
                objectAttackManager = GetComponentInChildren<PlayerObjectAttackManager>();
                if (objectAttackManager == null) objectAttackManager = transform.root.GetComponentInChildren<PlayerObjectAttackManager>();
            }

            _objetosDisponibles = GetComponentsInChildren<ObjectBoxCharacteristics>(true);
            foreach (var objeto in _objetosDisponibles)
            {
                if (objeto != null && objeto.gameObject != null) objeto.gameObject.SetActive(false);
            }
        }

        private void Update() 
        {
            if (!IsOwner || _inputHandler == null) return;
            if (_playerMovement != null && _playerMovement.IsPushedActive) return;
            if (!_inputHandler.IsPunchPressedThisFrame() || Time.time < _nextPunchTime) return;

            ExecutePunch();
            _nextPunchTime = Time.time + punchCooldown;
        }

        private void ExecutePunch() 
        {
            if (_hitboxCoroutine != null) StopCoroutine(_hitboxCoroutine);

            if (sonidoGolpe != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(sonidoGolpe, volumenGolpe);
            }

            SincronizarSpriteGolpeEnClientesRpc();
            _hitboxCoroutine = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine() 
        {
            GameObject hurtboxActiva = ObtenerHurtboxPorIDActual();
            if (_animator != null) _animator.SetBool(IsPunch, true);
            if (hurtboxActiva != null) hurtboxActiva.SetActive(true);

            yield return new WaitForSeconds(hitboxDuration);

            if (hurtboxActiva != null) hurtboxActiva.SetActive(false);
            if (_animator != null) _animator.SetBool(IsPunch, false);
            _hitboxCoroutine = null;
        }

        [Rpc(SendTo.Everyone)]
        private void SincronizarSpriteGolpeEnClientesRpc()
        {
            if (IsOwner) return;

            if (sonidoGolpe != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(sonidoGolpe, volumenGolpe);
            }

            if (_visualCoroutine != null) StopCoroutine(_visualCoroutine);
            _visualCoroutine = StartCoroutine(VisualPunchRoutineRemoto());
        }

        private IEnumerator VisualPunchRoutineRemoto()
        {
            GameObject spriteActivo = ObtenerHurtboxPorIDActual();
            if (spriteActivo != null) spriteActivo.SetActive(true);

            yield return new WaitForSeconds(hitboxDuration);

            if (spriteActivo != null) spriteActivo.SetActive(false);
            _visualCoroutine = null;
        }

        private GameObject ObtenerHurtboxPorIDActual()
        {
            if (objectAttackManager == null) return null;
            int idBuscado = objectAttackManager.IdGolpeActivoActual;

            foreach (var objeto in _objetosDisponibles)
            {
                if (objeto != null && objeto.ObjetoID == idBuscado) return objeto.gameObject;
            }
            return null;
        }
    }
}

