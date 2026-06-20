using Unity.Netcode;
using UnityEngine;

namespace MultiplayerScripts
{
    public class MatchInformationManager : NetworkBehaviour
    {
        [Header("Reglas Generales de Partida")]
        [SerializeField] private float vidaInicialPredeterminada = 100f;

        [Header("Referencias Locales")]
        [SerializeField] private GlobalGameState.GameTimeSection timeSection;

        private NetworkVariable<float> _vidaInicialGlobal = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkList<PlayerData> _listaJugadores;

        public System.Action<NetworkListEvent<PlayerData>> AlModificarListaJugadores;

        private void Awake()
        {
            _listaJugadores = new NetworkList<PlayerData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            
            if (timeSection == null)
            {
                timeSection = GetComponentInChildren<GlobalGameState.GameTimeSection>();
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer == true)
            {
                _vidaInicialGlobal.Value = vidaInicialPredeterminada;
                
                if (timeSection != null)
                {
                    timeSection.IniciarCronometroMaestro();
                }

                RegistrarNuevoJugador("nigger");
                RegistrarNuevoJugador("Jugador_1");
            }

            _listaJugadores.OnListChanged += OnListaJugadoresModificada;
        }

        public void RegistrarNuevoJugador(string nombre)
        {
            if (IsServer == false) return;

            PlayerData nuevoJugador = new PlayerData();
            nuevoJugador.nombreJugador = nombre;
            nuevoJugador.puntosVictoria = 0;
            nuevoJugador.cooldownHabilidad = 0f;
            nuevoJugador.tieneObjeto = false;
            nuevoJugador.vidaActual = _vidaInicialGlobal.Value;

            _listaJugadores.Add(nuevoJugador);
        }

        public void ModificarVidaJugador(string nombreUnico, float cantidad)
        {
            ModificarVidaJugadorServerRpc(nombreUnico, cantidad);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ModificarVidaJugadorServerRpc(string nombreUnico, float cantidad)
        {
            for (int i = 0; i < _listaJugadores.Count; i++)
            {
                if (_listaJugadores[i].nombreJugador.ToString() == nombreUnico)
                {
                    PlayerData datosModificados = _listaJugadores[i];
                    datosModificados.vidaActual += cantidad;

                    if (datosModificados.vidaActual < 0f) datosModificados.vidaActual = 0f;
                    if (datosModificados.vidaActual > _vidaInicialGlobal.Value) datosModificados.vidaActual = _vidaInicialGlobal.Value;
                    _listaJugadores[i] = datosModificados; 
                    
                    break;
                }
            }
        }

        private void OnListaJugadoresModificada(NetworkListEvent<PlayerData> changeEvent)
        {
            AlModificarListaJugadores?.Invoke(changeEvent);
        }

        public override void OnNetworkDespawn()
        {
            _listaJugadores.OnListChanged -= OnListaJugadoresModificada;
        }
    }
}