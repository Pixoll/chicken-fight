using System.Collections;
using UnityEngine;

namespace MyScripts
{
    public class HitReceiver : MonoBehaviour
    {
        [Header("Configuración de Impacto")]
        [SerializeField] private float knockbackForce = 15f;
        [SerializeField] private float resetDelay = 3f;

        private Rigidbody2D _rb;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Coroutine _resetCoroutine;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }


        public void ReceiveHit(Vector2 attackerPosition)
        {
            if (_rb == null) return;

            Vector2 pushDirection = ((Vector2)transform.position - attackerPosition).normalized;

            if (Mathf.Abs(pushDirection.y) < 0.2f)
            {
                pushDirection.y = 0.5f;
                pushDirection = pushDirection.normalized;
            }

            if (_resetCoroutine != null)
            {
                StopCoroutine(_resetCoroutine);
            }

            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            _rb.AddForce(pushDirection * knockbackForce, ForceMode2D.Impulse);

            _resetCoroutine = StartCoroutine(ResetPositionRoutine());
        }

        private IEnumerator ResetPositionRoutine()
        {
            yield return new WaitForSeconds(resetDelay);

            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }

            transform.position = _initialPosition;
            transform.rotation = _initialRotation;

            yield return new WaitForFixedUpdate();

            if (_rb != null)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
            }

            _resetCoroutine = null;
        }
    }
}