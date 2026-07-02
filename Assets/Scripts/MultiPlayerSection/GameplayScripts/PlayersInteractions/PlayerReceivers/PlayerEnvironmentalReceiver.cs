using MultiPlayerSection.NetworkScripts;
using MultiPlayerSection.PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions.PlayerReceivers
{
    // Hereda de NetworkBehaviour para conocer la identidad local (IsOwner)
    public class PlayerEnvironmentalReceiver : NetworkBehaviour 
    {
        private Rigidbody2D _rb;
        private PlayerMovement _playerMovement;
        private PlayerIdentity _playerIdentity;
        private MatchInformationManager _matchManager;

        private void Awake()
        {
            Transform root = transform.root;
            _rb = root.GetComponent<Rigidbody2D>();
            _playerIdentity = root.GetComponent<PlayerIdentity>();
            _playerMovement = root.GetComponentInChildren<PlayerMovement>();
        }

        private void Start()
        {
            _matchManager = FindFirstObjectByType<MatchInformationManager>();
        }

        /// <summary>
        /// Procesa impactos de fuentes globales (Escenario) sin un dueño jugador.
        /// </summary>
        public void EnviarImpactoAmbientalALaRed(
            float damage,
            float force, 
            HurtboxCharacteristics.InclinacionVertical inclinacion,
            HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun,
            Vector2 direccionDerechaHurtbox,
            Vector2 direccionArribaHurtbox)
        {
            // 🛡️ FILTRO CRÍTICO DE ESCENARIO: 
            // Como la lava está en todas las pantallas, solo permito que este script actúe 
            // si se está ejecutando en MI GALLINA PROPIA en este celular.
            if (!IsOwner) return;

            Debug.Log($"<color=orange>[ENVIRONMENTAL RECEIVER]</color> Mi gallina ({_playerIdentity.NombreIdentificador}) pisó un peligro ambiental. Daño: {damage}");

            // 1. Informamos al servidor para aplicar la pérdida de vida de forma legítima
            SolicitarAplicarDanoAmbientalServerRpc(damage, _playerIdentity.NombreIdentificador.ToString());

            // 2. Aplicamos la reacción física local e inmediatamente la sincronizamos a los demás
            EjecutarFisicaAmbiental(force, inclinacion, direccion, durationStun, direccionDerechaHurtbox, direccionArribaHurtbox);
            PropagarFisicasAmbientalesRpc(force, inclinacion, direccion, durationStun, direccionDerechaHurtbox, direccionArribaHurtbox);
        }

        [ServerRpc]
        private void SolicitarAplicarDanoAmbientalServerRpc(float damage, string nombreIdentificador)
        {
            if (_matchManager != null)
            {
                _matchManager.ModificarVidaJugador(nombreIdentificador, -damage);
            }
        }

        [Rpc(SendTo.NotMe)]
        private void PropagarFisicasAmbientalesRpc(float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion, float durationStun, Vector2 dirDerecha, Vector2 dirArriba)
        {
            EjecutarFisicaAmbiental(force, inclinacion, direccion, durationStun, dirDerecha, dirArriba);
        }

        private void EjecutarFisicaAmbiental(float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion, float durationStun, Vector2 dirDerecha, Vector2 dirArriba)
        {
            if (_rb == null) return;

            if (durationStun > 0f && _playerMovement != null)
            {
                _playerMovement.StunningTime(durationStun);
            }

            Vector2 vectorBase = Vector2.zero;
            switch (direccion)
            {
                case HurtboxCharacteristics.DireccionHorizontal.Forward:   vectorBase = dirDerecha; break;
                case HurtboxCharacteristics.DireccionHorizontal.Backward:  vectorBase = -dirDerecha; break;
                case HurtboxCharacteristics.DireccionHorizontal.Up:        vectorBase = dirArriba; break;
                case HurtboxCharacteristics.DireccionHorizontal.Down:      vectorBase = -dirArriba; break;
            }

            if (inclinacion == HurtboxCharacteristics.InclinacionVertical.Top) vectorBase += Vector2.up;
            else if (inclinacion == HurtboxCharacteristics.InclinacionVertical.Bottom) vectorBase += Vector2.down;

            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(vectorBase.normalized * force, ForceMode2D.Impulse);
        }
    }
}