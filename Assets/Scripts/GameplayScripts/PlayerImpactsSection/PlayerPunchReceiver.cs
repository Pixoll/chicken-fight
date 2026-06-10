using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection {
    public class PlayerPunchReceiver : MonoBehaviour {
        [Header("Configuración del Empuje (Knockback)")] [SerializeField]
        private float defaultPunchForce = 15f;

        private Rigidbody2D _rb;

        private void Awake() {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void ApplyPunchKnockback(Vector3 attackerPosition, float force) {
            if (_rb == null) return;

            Vector2 pushDirection = (transform.position - attackerPosition).normalized;

            pushDirection.y += 0.3f;
            pushDirection = pushDirection.normalized;

            _rb.linearVelocity = Vector2.zero;

            _rb.AddForce(pushDirection * force, ForceMode2D.Impulse);

            Debug.Log($"<color=cyan>[PlayerPunchReceiver] ¡Gallina lanzada con fuerza dinámica de: {force}!</color>");
        }
    }
}
