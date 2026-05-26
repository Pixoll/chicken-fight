using System.Collections;
using UnityEngine;

namespace MyScripts
{
    public class PlayerAttack : MonoBehaviour
    {
        [Header("Configuración del Ataque")]
        [SerializeField] private GameObject attackArea; 
        
        [SerializeField] private float attackDuration = 0.3f; 

        private PlayerInputActions _inputActions;
        private Coroutine _attackCoroutine;
        private bool _isAttacking;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable() => _inputActions.Player.Attack.Enable();
        private void OnDisable() => _inputActions.Player.Attack.Disable();

        private void Update()
        {
            if (_inputActions.Player.Attack.WasPressedThisFrame() && !_isAttacking)
            {
                TriggerAttack();
            }
        }

        private void TriggerAttack()
        {
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
            }

            _attackCoroutine = StartCoroutine(AttackRoutine());
        }

        private IEnumerator AttackRoutine()
        {
            _isAttacking = true;

            if (attackArea != null)
            {
                attackArea.SetActive(true);
            }

            yield return new WaitForSeconds(attackDuration);

            if (attackArea != null)
            {
                attackArea.SetActive(false);
            }

            _isAttacking = false;
            _attackCoroutine = null;
        }
    }
}