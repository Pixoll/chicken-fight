using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace GameplayScripts
{
    public class PlayerAttack : NetworkBehaviour {
        [Header("Configuración del Ataque")] [SerializeField]
        private GameObject attackArea;

        [SerializeField] private float attackDuration = 0.3f;

        private PlayerInputActions _inputActions;
        private RectTransform _inputAttackAreaRect;
        private Coroutine _attackCoroutine;
        private bool _isAttacking;
        private int _lastProcessedTouchId1 = -1;
        private int _lastProcessedTouchId2 = -1;

        private void Awake() {
            _inputActions = new PlayerInputActions();
        }

        public override void OnNetworkSpawn() {
            GameObject inputAttackArea = GameObject.FindWithTag("AttackArea");

            if (inputAttackArea != null) {
                _inputAttackAreaRect = inputAttackArea.GetComponent<RectTransform>();
            }
        }

        private void OnEnable() => _inputActions.Player.Attack.Enable();
        private void OnDisable() => _inputActions.Player.Attack.Disable();

        private void Update() {
            if (!IsOwner) return;

            if (WantsToAttack() && !_isAttacking) {
                TriggerAttack();
            }
        }

        private bool WantsToAttack() {
            if (_inputActions.Player.Attack.WasPressedThisFrame()) {
                return true;
            }

            int touchCount = Math.Min(Touchscreen.current.touches.Count, 2);

            if (_inputAttackAreaRect == null || Touchscreen.current == null || touchCount <= 0) {
                return false;
            }

            for (int index = 0; index < touchCount; index++) {
                TouchControl touch = Touchscreen.current.touches[index];

                if (
                    touch.touchId.value == _lastProcessedTouchId1
                    || touch.touchId.value == _lastProcessedTouchId2
                    || touch.phase.value != TouchPhase.Began
                ) {
                    continue;
                }

                if (index == 0) {
                    _lastProcessedTouchId1 = touch.touchId.value;
                } else {
                    _lastProcessedTouchId2 = touch.touchId.value;
                }

                if (RectTransformUtility.RectangleContainsScreenPoint(_inputAttackAreaRect, touch.position.value)) {
                    return true;
                }
            }

            return false;
        }

        private void TriggerAttack() {
            if (_attackCoroutine != null) {
                StopCoroutine(_attackCoroutine);
            }

            _attackCoroutine = StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine() {
            _isAttacking = true;

            if (attackArea != null) {
                attackArea.SetActive(true);
            }

            yield return new WaitForSeconds(attackDuration);

            if (attackArea != null) {
                attackArea.SetActive(false);
            }

            _isAttacking = false;
            _attackCoroutine = null;
        }
    }
}
