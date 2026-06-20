using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection.PlayerReceiverSection
{
    public class PlayerPunchReceiver : NetworkBehaviour 
    {
        private Rigidbody2D _rb;
        private PlayerMovement _playerMovement;
        private PlayerIdentity _playerIdentity;
        private MultiplayerScripts.MatchInformationManager _matchManager;

        private void Awake()
        {
            Transform root = transform.root;
            _rb = root.GetComponent<Rigidbody2D>();
            _playerIdentity = root.GetComponent<PlayerIdentity>();
            
            // Buscamos el movimiento en los hijos
            _playerMovement = root.GetComponentInChildren<PlayerMovement>();
            
            if (_rb == null)
            {
                Debug.LogError("[PlayerPunchReceiver] ¡CRÍTICO: No se encontró Rigidbody2D en la raíz del personaje!");
            }

            if (_playerIdentity == null)
            {
                Debug.LogWarning("[PlayerPunchReceiver] No se encontró PlayerIdentity en la raíz. El daño no se restará.");
            }
        }

        private void Start()
        {
            _matchManager = FindFirstObjectByType<MultiplayerScripts.MatchInformationManager>();
        }
        

        public void EnviarImpactoFisicoALaRed(
            float damage,
            float force, 
            HurtboxCharacteristics.InclinacionVertical inclinacion,
            HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun,
            Vector2 direccionDerechaEnemigo,
            Vector2 direccionArribaEnemigo)
        {

            if (_matchManager != null && _playerIdentity != null)
            {
                _matchManager.ModificarVidaJugador(_playerIdentity.NombreIdentificador, -damage);
            }

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
