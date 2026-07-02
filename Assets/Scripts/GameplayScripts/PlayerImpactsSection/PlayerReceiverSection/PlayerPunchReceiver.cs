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
            _playerMovement = root.GetComponentInChildren<PlayerMovement>();
            
            if (_rb == null) Debug.LogError("[PlayerPunchReceiver] ¡CRÍTICO: No se encontró Rigidbody2D en la raíz!");
        }

        private void Start()
        {
            _matchManager = FindFirstObjectByType<MultiplayerScripts.MatchInformationManager>();
        }

        /// <summary>
        /// Método llamado por el ImpactManager local.
        /// </summary>
        public void EnviarImpactoFisicoALaRed(
            float damage,
            float force, 
            HurtboxCharacteristics.InclinacionVertical inclinacion,
            HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun,
            Vector2 direccionDerechaEnemigo,
            Vector2 direccionArribaEnemigo)
        {
            // 🔍 LOG DE DIAGNÓSTICO CRÍTICO
            string dueñoObjeto = IsOwner ? "SÍ SOY EL DUEÑO" : "NO SOY EL DUEÑO";
            Debug.Log($"<color=orange>[HITBOX DETECTED]</color> Objeto: {gameObject.name} | Identificador: {_playerIdentity?.NombreIdentificador} | {dueñoObjeto} | IsServer: {IsServer}");

            // Si este filtro está abortando siempre, significa que la colisión se está leyendo en la instancia equivocada
            if (!IsOwner) return;

            Debug.Log($"<color=green>[RECEIVER PROCESANDO]</color> ¡Filtro aprobado! Aplicando daño y físicas.");

            // 1. Gestionamos la reducción de vida
            SolicitarAplicarDanoServerRpc(damage, _playerIdentity.NombreIdentificador.ToString());

            // 2. Aplico la física en mi pantalla
            EjecutarFisicaEfectiva(force, inclinacion, direccion, durationStun, direccionDerechaEnemigo, direccionArribaEnemigo);

            // 3. Sincronización al resto
            PropagarFisicasAlRestoDeClientesRpc(force, inclinacion, direccion, durationStun, direccionDerechaEnemigo, direccionArribaEnemigo);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SolicitarAplicarDanoServerRpc(float damage, string nombreIdentificador)
        {
            if (_matchManager != null)
            {
                _matchManager.ModificarVidaJugador(nombreIdentificador, -damage);
            }
        }

        // Enviamos el vector exacto calculado por el dueño a todos los clones remotos
        [Rpc(SendTo.NotMe)]
        private void PropagarFisicasAlRestoDeClientesRpc(float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion, float durationStun, Vector2 dirDerecha, Vector2 dirArriba)
        {
            // Las otras pantallas simplemente imitan la fuerza que el dueño ya experimentó
            EjecutarFisicaEfectiva(force, inclinacion, direccion, durationStun, dirDerecha, dirArriba);
        }

        private void EjecutarFisicaEfectiva(float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion, float durationStun, Vector2 dirDerecha, Vector2 dirArriba)
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
