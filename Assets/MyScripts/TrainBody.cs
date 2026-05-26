using System.Collections;
using UnityEngine;

namespace MyScripts
{
    public class TrainBody : MonoBehaviour
    {
        [SerializeField] private float resetDelay = 3f;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private Rigidbody2D _rb;
        private Coroutine _resetCoroutine;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (_resetCoroutine != null)
            {
                StopCoroutine(_resetCoroutine);
            }

            _resetCoroutine = StartCoroutine(ResetPositionRoutine());
        }

        private IEnumerator ResetPositionRoutine()
        {
            yield return new WaitForSeconds(resetDelay);

            if (_rb)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
                
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
            }

            transform.position = _initialPosition;
            transform.rotation = _initialRotation;

            yield return new WaitForFixedUpdate();

            if (_rb)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
            }

            _resetCoroutine = null;
        }
    }
}