using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class HitReceiver : NetworkBehaviour {
    [Header("Configuración de Tipo de Objeto")]
    [Tooltip("Si está marcado, no reseteará posición. Si está desmarcado (Maniquí), regresará a su sitio original.")]
    [SerializeField] private bool isPlayer = false;

    [Header("Configuración de Impacto")] 
    [SerializeField] private float knockbackForceX = 18f;
    [SerializeField] private float knockbackForceY = 10f;
    [SerializeField] private float resetDelay = 3f;

    private Rigidbody2D _rb;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Coroutine _resetCoroutine;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        
        // Guardamos la posición inicial de arranque local
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    public void ReceiveHit(Vector2 attackerPosition) {
        // Enviamos la orden al servidor
        ReceiveHitServerRpc(attackerPosition);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveHitServerRpc(Vector2 attackerPosition) {
        if (_rb == null) return;

        // 1. Calculamos la dirección horizontal del golpe
        float horizontalDirection = transform.position.x - attackerPosition.x;
        horizontalDirection = Mathf.Abs(horizontalDirection) < 0.05f ? 1f : Mathf.Sign(horizontalDirection);

        // 2. Nos aseguramos de que sea dinámico para procesar el impacto
        _rb.bodyType = RigidbodyType2D.Dynamic;
        
        // 3. Limpiamos velocidades para evitar que la inercia previa frene el golpe
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        // 4. Aplicamos el vector de fuerza masivo
        Vector2 finalKnockback = new Vector2(horizontalDirection * knockbackForceX, knockbackForceY);
        _rb.AddForce(finalKnockback, ForceMode2D.Impulse);

        // 5. SI NO ES JUGADOR (es el maniquí), iniciamos la rutina de reseteo en el Servidor
        if (!isPlayer) {
            if (_resetCoroutine != null) {
                StopCoroutine(_resetCoroutine);
            }
            _resetCoroutine = StartCoroutine(ResetPositionRoutine());
        }
    }

    private IEnumerator ResetPositionRoutine() {
        // Esperamos el tiempo configurado (ej. 3 segundos) en el servidor
        yield return new WaitForSeconds(resetDelay);

        if (_rb) {
            // Convertimos temporalmente a Kinematic para congelar el movimiento de red
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        // El servidor cambia la posición de forma autoritativa.
        // NetworkRigidbody2D / NetworkTransform forzarán a los clientes a teletransportar el objeto en sus pantallas.
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;

        // Esperamos a que termine el frame de físicas para asegurar la sincronización del teletransporte
        yield return new WaitForFixedUpdate();

        if (_rb) {
            // Devolvemos el cuerpo a Dinámico para que pueda recibir el siguiente golpe
            _rb.bodyType = RigidbodyType2D.Dynamic;
        }

        _resetCoroutine = null;
    }
}
