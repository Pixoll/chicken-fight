using MultiPlayerSection.NetworkScripts;
using MultiPlayerSection.PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions.PlayerReceivers
{
    public class PlayerWithOwnerReceiver : NetworkBehaviour 
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
        
        public void EnviarImpactoFisicoALaRed(
            float damage, float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun, Vector2 direccionDerechaEnemigo, Vector2 direccionArribaEnemigo, string nombreVictima, string nombreAtacante,
            float heal, bool appliesSlow, float slowIntensity, float slowDuration)
        {
            string miInstanciaDePantallaID = NetworkManager.Singleton.LocalClientId.ToString();

            if (miInstanciaDePantallaID != nombreAtacante) return; 

            SolicitarProcesarImpactoEnServidorServerRpc(
                damage, force, inclinacion, direccion, durationStun, direccionDerechaEnemigo, direccionArribaEnemigo, nombreVictima, nombreAtacante,
                heal, appliesSlow, slowIntensity, slowDuration
            );
        }

        [ServerRpc(RequireOwnership = false)]
        private void SolicitarProcesarImpactoEnServidorServerRpc(
            float damage, float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun, Vector2 dirDerecha, Vector2 dirArriba, string nombreVictima, string nombreAtacante,
            float heal, bool appliesSlow, float slowIntensity, float slowDuration)
        {
            if (_matchManager != null)
            {
                _matchManager.ModificarVidaJugador(nombreVictima, -damage);

                if (heal > 0f)
                {
                    _matchManager.ModificarVidaJugador(nombreAtacante, heal);
                    Debug.Log($"<color=green>[SERVER] -> Sanando al atacante ({nombreAtacante}) por +{heal} puntos.</color>");
                }
            }

            ProcesarFisicaDeGolpeEnClientesRpc(
                force, inclinacion, direccion, durationStun, dirDerecha, dirArriba, nombreVictima, nombreAtacante,
                appliesSlow, slowIntensity, slowDuration
            );
        }

        [Rpc(SendTo.Everyone)]
        private void ProcesarFisicaDeGolpeEnClientesRpc(
            float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun, Vector2 dirDerecha, Vector2 dirArriba, string nombreVictima, string nombreAtacante,
            bool appliesSlow, float slowIntensity, float slowDuration)
        {
            string miInstanciaDePantallaID = NetworkManager.Singleton.LocalClientId.ToString();

            if (miInstanciaDePantallaID == nombreVictima)
            {
                AplicarAturdimientoLocal(durationStun);
                AplicarFuerzaDeEmpujeLocal(force, inclinacion, direccion, dirDerecha, dirArriba);

                if (appliesSlow && _playerMovement != null)
                {
                    _playerMovement.AplicarRalentizacionLocal(slowIntensity, slowDuration);
                }
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
