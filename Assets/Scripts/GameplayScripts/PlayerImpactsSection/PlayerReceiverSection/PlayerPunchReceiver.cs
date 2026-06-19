using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection.PlayerReceiverSection
{
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
        
        
        public void EnviarImpactoFisicoALaRed(float force, HurtboxCharacteristics.InclinacionVertical inclinacion,
            HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun,
            Vector2 direccionDerechaEnemigo,
            Vector2 direccionArribaEnemigo
            )
        {
            ProcesarFisicaDeGolpeRpc(force, inclinacion, direccion, durationStun, direccionDerechaEnemigo, direccionArribaEnemigo);
        }

        [Rpc(SendTo.Everyone)]
        private void ProcesarFisicaDeGolpeRpc(float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion, float durationStun, Vector2 dirDerecha, Vector2 dirArriba)
        {
            if (_rb == null) return;

            if (durationStun > 0f && _playerMovement != null)
            {
                _playerMovement.StunningTime(durationStun);
            }

            Vector2 vectorBase = Vector2.zero;

            if (direccion == HurtboxCharacteristics.DireccionHorizontal.Forward)   vectorBase = dirDerecha;
            if (direccion == HurtboxCharacteristics.DireccionHorizontal.Backward)  vectorBase = -dirDerecha;
            if (direccion == HurtboxCharacteristics.DireccionHorizontal.Up)        vectorBase = dirArriba;
            if (direccion == HurtboxCharacteristics.DireccionHorizontal.Down)      vectorBase = -dirArriba;

            if (inclinacion == HurtboxCharacteristics.InclinacionVertical.Top)
            {
                vectorBase += Vector2.up;
            }
            else if (inclinacion == HurtboxCharacteristics.InclinacionVertical.Bottom)
            {
                vectorBase += Vector2.down;
            }

            Vector2 finalPushVector = vectorBase.normalized;

            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(finalPushVector * force, ForceMode2D.Impulse);
        }
    }
}