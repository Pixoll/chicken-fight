using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts {
    public class PlayerPunch : NetworkBehaviour {
        [Header("Configuración del Golpe")] [SerializeField]
        private float punchCooldown = 4f;

        [SerializeField] private float hitboxDuration = 1f;
        [SerializeField] private GameObject punchHurtbox;

        private PlayerInputHandler _inputHandler;
        private float _nextPunchTime;
        private Coroutine _hitboxCoroutine;

        private void Awake() {
            _inputHandler = transform.root.GetComponentInChildren<PlayerInputHandler>();

            if (punchHurtbox != null) {
                punchHurtbox.SetActive(false);
            }
        }

        private void Update() {
            if (!IsOwner || !_inputHandler.IsPunchPressedThisFrame() || Time.time < _nextPunchTime) return;

            ExecutePunch();
            _nextPunchTime = Time.time + punchCooldown;
        }

        private void ExecutePunch() {
            if (_hitboxCoroutine != null) {
                StopCoroutine(_hitboxCoroutine);
            }

            _hitboxCoroutine = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine() {
            ActivePunchHurtbox();

            yield return new WaitForSeconds(hitboxDuration);

            InactivePunchHurtbox();

            _hitboxCoroutine = null;
        }

        public void ActivePunchHurtbox() {
            punchHurtbox.SetActive(true);
        }

        public void InactivePunchHurtbox() {
            punchHurtbox.SetActive(false);
        }
    }
}
