using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.PlayerScripts 
{
    public class PlayerPunch : NetworkBehaviour 
    {
        public enum TipoObjetoEquipado { Ninguno, Espada }

        [Header("Estado del Inventario")]
        [SerializeField] private TipoObjetoEquipado objetoActual = TipoObjetoEquipado.Ninguno;

        [Header("Configuración del Golpe Base (Punch)")] 
        [SerializeField] private float punchCooldown = 4f;
        [SerializeField] private float hitboxDuration = 1f;
        [SerializeField] private GameObject punchHurtbox;

        [Header("Configuración de Objetos Equipables")]
        [SerializeField] private GameObject espadaHurtbox; 

        private PlayerInputHandler _inputHandler;
        private float _nextPunchTime;
        private Coroutine _hitboxCoroutine;

        public TipoObjetoEquipado ObjetoActual 
        {
            get => objetoActual;
            set => objetoActual = value;
        }

        private void Awake() 
        {
            _inputHandler = transform.root.GetComponentInChildren<PlayerInputHandler>();

            if (punchHurtbox != null) punchHurtbox.SetActive(false);
            if (espadaHurtbox != null) espadaHurtbox.SetActive(false);
        }

        private void Update() 
        {
            if (!IsOwner || !_inputHandler.IsPunchPressedThisFrame() || Time.time < _nextPunchTime) return;

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
            GameObject hurtboxActiva = ObtenerHurtboxActual();

            if (hurtboxActiva) hurtboxActiva.SetActive(true);

            yield return new WaitForSeconds(hitboxDuration);

            if (hurtboxActiva) hurtboxActiva.SetActive(false);

            _hitboxCoroutine = null;
        }

        private GameObject ObtenerHurtboxActual()
        {
            switch (objetoActual)
            {
                case TipoObjetoEquipado.Espada:
                    return espadaHurtbox;
                
                case TipoObjetoEquipado.Ninguno:
                default:
                    return punchHurtbox;
            }
        }
    }
}
