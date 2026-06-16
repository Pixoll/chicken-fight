using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection.PlayerReceiverSection
{
    // Script modular de red encargado EXCLUSIVAMENTE de las físicas de impacto
    public class PlayerPunchReceiver : NetworkBehaviour 
    {
        private Rigidbody2D _rb;
        private PlayerMovement _playerMovement;
        
        private void Awake()
        {
            _rb = transform.root.GetComponent<Rigidbody2D>();
            _playerMovement = transform.root.GetComponentInChildren<PlayerMovement>();
            
            if (_rb == null)
            {
                Debug.LogError("[PlayerPunchReceiver] ¡CRÍTICO: No se encontró Rigidbody2D en la raíz del personaje!");
            }
        }

        /// <summary>
        /// Punto de entrada local llamado por el manager.
        /// </summary>
        public void EnviarImpactoFisicoALaRed(float force, HurtboxCharacteristics.KnockbackDirection direction, float durationStun)
        {
            // Enviamos el paquete de red optimizado con los datos justos
            ProcesarFisicaDeGolpeRpc(force, direction, durationStun);
        }

        /// <summary>
        /// RPC especializado que se ejecuta en todas las simulaciones de esta gallina por red.
        /// </summary>
        [Rpc(SendTo.Everyone)]
        private void ProcesarFisicaDeGolpeRpc(float force, HurtboxCharacteristics.KnockbackDirection direction, float durationStun)
        {
            if (_rb == null) return;

            // 1. Si viene tiempo de aturdimiento, inhabilitamos el movimiento de la gallina dueña
            if (durationStun > 0f && _playerMovement != null)
            {
                _playerMovement.StunningTime(durationStun);
                Debug.Log($"<color=yellow>[STUN] Movimiento inhabilitado por {durationStun} segundos.</color>");
            }

            // 2. Traducimos el Enum de dirección a vectores matemáticos 2D
            Vector2 pushVector = GetVectorFromDirection(direction);
            
            // 3. Reseteamos la velocidad acumulada para que el golpe se sienta seco y potente
            _rb.linearVelocity = Vector2.zero; 

            // 4. Aplicamos el impulso físico
            _rb.AddForce(pushVector * force, ForceMode2D.Impulse);

            Debug.Log($"<color=cyan>[PlayerPunchReceiver RPC] ¡Impacto procesado! Fuerza: {force} | Dirección: {direction}</color>");
        }

        private Vector2 GetVectorFromDirection(HurtboxCharacteristics.KnockbackDirection direction)
        {
            return direction switch
            {
                HurtboxCharacteristics.KnockbackDirection.Top => Vector2.up,
                HurtboxCharacteristics.KnockbackDirection.Left => Vector2.left,
                HurtboxCharacteristics.KnockbackDirection.Right => Vector2.right,
                HurtboxCharacteristics.KnockbackDirection.TopLeft => new Vector2(-1f, 1f).normalized,
                HurtboxCharacteristics.KnockbackDirection.TopRight => new Vector2(1f, 1f).normalized,
                _ => Vector2.zero
            };
        }
    }
}