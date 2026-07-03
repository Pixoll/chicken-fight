using System.Collections;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.Objects
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingObject : MonoBehaviour
    {
        [Header("Configuración de Caída")]
        [Tooltip("Tiempo en segundos que el objeto se quedará flotando en el aire antes de caer")]
        [SerializeField] private float tiempoFlotando = 3f;
        [Tooltip("Capa que representa el suelo del mapa (ej: Ground o Default)")]
        [SerializeField] private LayerMask capaSuelo;
        private Rigidbody2D _rb;
        private bool _yaSuelo = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Start()
        {
            StartCoroutine(CronometroFlotacionRoutine());
        }

        private IEnumerator CronometroFlotacionRoutine()
        {
            yield return new WaitForSeconds(tiempoFlotando);
            if (!_yaSuelo)
            {
                _rb.gravityScale = 1f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_yaSuelo || ((1 << collision.gameObject.layer) & capaSuelo) == 0) return;
            FrenarEnSuelo();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_yaSuelo || ((1 << collision.gameObject.layer) & capaSuelo) == 0) return;
            FrenarEnSuelo();
        }

        private void FrenarEnSuelo()
        {
            _yaSuelo = true;
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll; 
        }
    }
}
