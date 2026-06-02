using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class HitReceiver : NetworkBehaviour {
    [Header("Configuración de Tipo de Objeto")] [SerializeField]
    private bool isPlayer;

    [Header("Configuración de Impacto")] [SerializeField]
    private float knockbackForceX = 18f;

    [SerializeField] private float knockbackForceY = 10f;
    [SerializeField] private float resetDelay = 3f;

    [Header("Estadísticas de Combate")]
    // La vida sincronizada automáticamente en red
    public NetworkVariable<int> currentHealth = new(100);

    private Rigidbody2D _rb;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private Coroutine _resetCoroutine;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    public void ReceiveHit(Vector2 attackerPosition) {
        // Bloquear golpes si la partida ya terminó
        if (GameplayManager.Instance != null && GameplayManager.Instance.isMatchOver.Value) return;

        ReceiveHitServerRpc(attackerPosition);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveHitServerRpc(Vector2 attackerPosition) {
        if (_rb == null) return;

        // 1. Descontar vida de forma segura en el servidor
        if (currentHealth.Value > 0) {
            currentHealth.Value -= 5;
            // Evaluamos si este golpe causó una muerte/victoria
            GameplayManager.Instance?.CheckWinConditions();
        }

        // Si ya murió, evitamos aplicar fuerzas físicas extras
        if (currentHealth.Value <= 0 && isPlayer) {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        // 2. Físicas del golpe (Tu código de empuje lateral)
        float horizontalDirection = transform.position.x - attackerPosition.x;
        horizontalDirection = Mathf.Abs(horizontalDirection) < 0.05f ? 1f : Mathf.Sign(horizontalDirection);

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        Vector2 finalKnockback = new Vector2(horizontalDirection * knockbackForceX, knockbackForceY);
        _rb.AddForce(finalKnockback, ForceMode2D.Impulse);

        if (!isPlayer) {
            if (_resetCoroutine != null) StopCoroutine(_resetCoroutine);
            _resetCoroutine = StartCoroutine(ResetPositionRoutine());
        }
    }

    private IEnumerator ResetPositionRoutine() {
        yield return new WaitForSeconds(resetDelay);

        if (_rb != null) {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
        yield return new WaitForFixedUpdate();

        if (_rb != null) _rb.bodyType = RigidbodyType2D.Dynamic;
        _resetCoroutine = null;
    }
}
