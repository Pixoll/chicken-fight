using MultiPlayerSection.NetworkScripts;
using MultiPlayerSection.PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions.PlayerReceivers
{
    public class PlayerEnvironmentalReceiver : NetworkBehaviour
    {
        private Rigidbody2D _rb;
        private PlayerMovement _playerMovement;
        private MatchInformationManager _matchManager;

        private void Awake()
        {
            Transform root = transform.root;
            _rb = root.GetComponent<Rigidbody2D>();
            _playerMovement = root.GetComponentInChildren<PlayerMovement>();
        }

        private void Start()
        {
            _matchManager = FindFirstObjectByType<MatchInformationManager>();
        }
        
        public void EnviarImpactoAmbientalALaRed(
            float damage, float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun, Vector2 direccionDerechaEntorno, Vector2 direccionArribaEntorno, string nombreAfectado,
            float heal, bool appliesSlow, float slowIntensity, float slowDuration)
        {
            string miInstanciaDePantallaID = NetworkManager.Singleton.LocalClientId.ToString();

            if (miInstanciaDePantallaID != nombreAfectado) return;

            AplicarAturdimientoLocal(durationStun);
            AplicarFuerzaDeEmpujeLocal(force, inclinacion, direccion, direccionDerechaEntorno, direccionArribaEntorno);

            if (appliesSlow && _playerMovement != null)
            {
                _playerMovement.AplicarRalentizacionLocal(slowIntensity, slowDuration);
            }

            NotificarCambiosSaludAmbientalServerRpc(damage, heal, nombreAfectado);
        }

        [ServerRpc(RequireOwnership = false)]
        private void NotificarCambiosSaludAmbientalServerRpc(float damage, float heal, string nombreVictima)
        {
            if (_matchManager != null)
            {
                if (damage > 0f) _matchManager.ModificarVidaJugador(nombreVictima, -damage);

                if (heal > 0f) _matchManager.ModificarVidaJugador(nombreVictima, heal);
            }
        }

        private void AplicarAturdimientoLocal(float duracion)
        {
            if (duracion <= 0f || _playerMovement == null) return;
            _playerMovement.StunningTime(duracion);
        }

        private void AplicarFuerzaDeEmpujeLocal(float fuerza, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion, Vector2 dirDerecha, Vector2 dirArriba)
        {
            if (_rb == null || fuerza <= 0f) return;

            Vector2 vectorResultado = Vector2.zero;
            switch (direccion)
            {
                case HurtboxCharacteristics.DireccionHorizontal.Forward:   vectorResultado = dirDerecha; break;
                case HurtboxCharacteristics.DireccionHorizontal.Backward:  vectorResultado = -dirDerecha; break;
                case HurtboxCharacteristics.DireccionHorizontal.Up:        vectorResultado = dirArriba; break;
                case HurtboxCharacteristics.DireccionHorizontal.Down:      vectorResultado = -dirArriba; break;
            }

            if (inclinacion == HurtboxCharacteristics.InclinacionVertical.Top) vectorResultado += Vector2.up;
            else if (inclinacion == HurtboxCharacteristics.InclinacionVertical.Bottom) vectorResultado += Vector2.down;

            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(vectorResultado.normalized * fuerza, ForceMode2D.Impulse);
        }
    }
}
