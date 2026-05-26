using UnityEngine;

namespace MyScripts
{
    public class PlayerPush : MonoBehaviour
    {
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void ApplyPush(Vector2 direction, float force)
        {
            direction = direction.normalized;

            Debug.Log($"[PushComponent] Recibido - Dirección: {direction}, Fuerza: {force}");
            
            _rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
    }
}