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
            float damage,
            float force, 
            HurtboxCharacteristics.InclinacionVertical inclinacion,
            HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun,
            Vector2 direccionDerechaEnemigo,
            Vector2 direccionArribaEnemigo,
            string nombreVictima,
            string nombreAtacante)
        {
            string miInstanciaDePantallaID = NetworkManager.Singleton.LocalClientId.ToString();

            if (miInstanciaDePantallaID != nombreAtacante) 
            {
                return; 
            }

            Debug.Log($"<color=orange>[OWNER RECEIVER] -> Fase 1 Aprobada en Pantalla ({miInstanciaDePantallaID}). Esta copia de objeto envía ServerRpc porque coincide con el atacante real ({nombreAtacante}).</color>");

            SolicitarProcesarImpactoEnServidorServerRpc(
                damage, force, inclinacion, direccion, durationStun, direccionDerechaEnemigo, direccionArribaEnemigo, nombreVictima, nombreAtacante
            );
        }

        [ServerRpc(RequireOwnership = false)]
        private void SolicitarProcesarImpactoEnServidorServerRpc(
            float damage, float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion, float durationStun, Vector2 dirDerecha, Vector2 dirArriba, string nombreVictima, string nombreAtacante)
        {
            if (_matchManager != null)
            {
                _matchManager.ModificarVidaJugador(nombreVictima, -damage);
            }

            ProcesarFisicaDeGolpeEnClientesRpc(
                force, inclinacion, direccion, durationStun, dirDerecha, dirArriba, nombreVictima, nombreAtacante
            );
        }

        [Rpc(SendTo.Everyone)]
        private void ProcesarFisicaDeGolpeEnClientesRpc(
            float force, HurtboxCharacteristics.InclinacionVertical inclinacion, HurtboxCharacteristics.DireccionHorizontal direccion, float durationStun, Vector2 dirDerecha, Vector2 dirArriba, string nombreVictima, string nombreAtacante)
        {
            string miInstanciaDePantallaID = NetworkManager.Singleton.LocalClientId.ToString();

            if (miInstanciaDePantallaID == nombreVictima)
            {
                Debug.Log($"<color=red>[RECEIVER] -> Validación exitosa en pantalla ({miInstanciaDePantallaID}). Derivando a labores modulares.</color>");

                AplicarAturdimientoLocal(durationStun);

                AplicarFuerzaDeEmpujeLocal(force, inclinacion, direccion, dirDerecha, dirArriba);
            }
        }

        private void AplicarAturdimientoLocal(float duracion)
        {
            if (duracion <= 0f || _playerMovement == null) return;

            _playerMovement.StunningTime(duracion);
            
            Debug.Log($"<color=yellow>[STUN MODULAR] -> Joystick inhabilitado por {duracion}s. El oponente no puede moverse voluntariamente.</color>");
        }

        private void AplicarFuerzaDeEmpujeLocal(
            float fuerza, 
            HurtboxCharacteristics.InclinacionVertical inclinacion, 
            HurtboxCharacteristics.DireccionHorizontal direccion, 
            Vector2 dirDerecha, 
            Vector2 dirArriba)
        {
            if (_rb == null) return;

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
    
            Debug.Log($"<color=magenta>[FÍSICA MODULAR] -> Empujando a la gallina afectada con fuerza: {fuerza} | Dirección: {vectorResultado.normalized}</color>");
        }
    }
}
