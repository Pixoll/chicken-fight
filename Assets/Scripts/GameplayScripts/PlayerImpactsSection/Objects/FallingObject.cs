using System.Collections;
using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection.Objects
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
            
            // 🔍 CONFIGURACIÓN INICIAL: Nace flotando sin gravedad
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Start()
        {
            // Arrancamos el cronómetro de flotación apenas el GameTimeSection lo instancie en el mapa
            StartCoroutine(CronometroFlotacionRoutine());
        }

        private IEnumerator CronometroFlotacionRoutine()
        {
            yield return new WaitForSeconds(tiempoFlotando);

            // 🔥 SE ACABÓ EL TIEMPO: Activamos gravedad para que caiga de forma natural
            if (!_yaSuelo)
            {
                _rb.gravityScale = 1f;
            }
        }

        // 🔍 DETECCIÓN DEL SUELO: Funciona tanto si el suelo es Trigger como si es Sólido
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Si ya se detuvo en el suelo o no es la capa correcta, ignoramos
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
            
            // Frenamos por completo el objeto y lo dejamos estático en el suelo
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll; 
            
            Debug.Log($"[OBJETO] {gameObject.name} ha aterrizado de forma segura en el suelo.");
        }
    }
}