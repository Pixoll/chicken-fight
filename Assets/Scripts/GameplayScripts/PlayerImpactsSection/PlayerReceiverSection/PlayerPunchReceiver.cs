using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection.PlayerReceiverSection
{
    public class PlayerPunchReceiver : MonoBehaviour
    {
        public enum PunchInclination
        {
            Top,
            Mid,
            Bottom
        }

        private PunchInclination _punchDirection;
        private PlayerMovement _playerMovement;
        
        private Rigidbody2D _rb;
        
        private void Awake()
        {
            _rb = transform.root.GetComponent<Rigidbody2D>();
            _playerMovement = transform.root.GetComponentInChildren<PlayerMovement>();
            
            if (_rb == null)
            {
                Debug.LogError("[PlayerPunchReceiver] ¡CRÍTICO: No se encontró Rigidbody2D en la raíz del personaje!");
            }
            
            if (_playerMovement == null)
            {
                Debug.LogError("[PlayerPunchReceiver] ¡CRÍTICO: No se encontró PlayerMovement en algun hijo!");
            }
            if (_playerMovement )
            {
                Debug.Log("[PlayerPunchReceiver] ¡CRÍTICO: Si se encontró PlayerMovement en algun hijo!");
            }
        }

        public void ApplyPunchKnockback(Vector3 attackerPosition, float force, PunchInclination inclination)
        {
            Vector2 pushVector = default;
            
            bool direction = _playerMovement.IsFacingRight;
            Debug.Log("Esta mirando derecha?" + direction);
            
            switch (inclination)
            {
                case PunchInclination.Top:
                    pushVector = Vector2.up;
                    break;

                case PunchInclination.Mid:
                    pushVector = Vector2.up;
                    break;

                case PunchInclination.Bottom:
                    if (direction)
                    {
                        pushVector = Vector2.left;
                        break;
                    }
                    pushVector = Vector2.right;
                    break;
            }

            _rb.linearVelocity = Vector2.zero;

            _rb.AddForce(pushVector * force, ForceMode2D.Impulse);

            Debug.Log($"<color=cyan>[PlayerPunchReceiver] Knockback aplicado. Dirección Enum: {inclination} | Vector Final: {pushVector} | Fuerza: {force}</color>");
        }
        
        public void ApplyPunchEffect()
        {
        }
    }
}
