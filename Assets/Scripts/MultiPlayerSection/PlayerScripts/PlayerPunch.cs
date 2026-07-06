using System.Collections;
using Unity.Netcode;
using UnityEngine;
using MultiPlayerSection.GameplayScripts.Objects; 

namespace MultiPlayerSection.PlayerScripts 
{
    public class PlayerPunch : NetworkBehaviour 
    {
        private static readonly int IsPunch = Animator.StringToHash("IsPunch");

        [Header("Referencias de Gestión")]
        [Tooltip("Asigna aquí el contenedor que tiene el PlayerObjectAttackManager")]
        [SerializeField] private PlayerObjectAttackManager objectAttackManager;

        [Header("Configuración del Ataque")] 
        [SerializeField] private float punchCooldown = 4f;
        [SerializeField] private float hitboxDuration = 1f;

        private PlayerInputHandler _inputHandler;
        private PlayerMovement _playerMovement;
        private float _nextPunchTime;
        private Coroutine _hitboxCoroutine;
        
        private Animator _animator;
        private ObjectBoxCharacteristics[] _objetosDisponibles;

        private void Awake() 
        {
            _inputHandler = transform.root.GetComponentInChildren<PlayerInputHandler>();
            _animator = transform.root.GetComponentInChildren<Animator>();
            
            _playerMovement = transform.root.GetComponentInChildren<PlayerMovement>();

            if (objectAttackManager == null)
            {
                objectAttackManager = GetComponentInChildren<PlayerObjectAttackManager>();
                if (objectAttackManager == null) objectAttackManager = transform.root.GetComponentInChildren<PlayerObjectAttackManager>();
            }

            _objetosDisponibles = GetComponentsInChildren<ObjectBoxCharacteristics>(true);
            foreach (var objeto in _objetosDisponibles)
            {
                if (objeto != null && objeto.gameObject != null)
                {
                    objeto.gameObject.SetActive(false);
                }
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
            if (_hitboxCoroutine != null) 
            {
                StopCoroutine(_hitboxCoroutine);
            }

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

        private GameObject ObtenerHurtboxPorIDActual()
        {
            if (objectAttackManager == null)
            {
                Debug.LogWarning("[PlayerPunch] -> No hay ningún PlayerObjectAttackManager asignado o vinculado.");
                return null;
            }

            int idBuscado = objectAttackManager.IdGolpeActivoActual;

            foreach (var objeto in _objetosDisponibles)
            {
                if (objeto != null && objeto.ObjetoID == idBuscado)
                {
                    return objeto.gameObject;
                }
            }

            Debug.LogWarning($"[PlayerPunch] -> No se encontró ningún GameObject hijo con el ObjetoID: {idBuscado}");
            return null;
        }
    }
}
