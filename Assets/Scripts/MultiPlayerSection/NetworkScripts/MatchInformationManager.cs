using MultiPlayerSection.GameplayScripts.GlobalGameState;
using MultiPlayerSection.PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.NetworkScripts
{
    public class MatchInformationManager : NetworkBehaviour
    {
        [Header("Reglas Generales de Partida")]
        [SerializeField] private float vidaInicialPredeterminada = 100f;

        [Header("Configuración de Spawn Manual")]
        [SerializeField] private GameObject chickenPrefab;
        [SerializeField] private Transform puntoSpawnJugador0;
        [SerializeField] private Transform puntoSpawnJugador1;

        [Header("Referencias Locales")]
        [SerializeField] private GameTimeSection timeSection;

        private NetworkVariable<float> _vidaInicialGlobal = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkList<PlayerData> _listaJugadores;

        public System.Action<NetworkListEvent<PlayerData>> AlModificarListaJugadores;

        private void Awake()
        {
            _listaJugadores = new NetworkList<PlayerData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
            if (timeSection == null) timeSection = GetComponentInChildren<GameTimeSection>();
        }

        public override void OnNetworkSpawn()
        {
            _listaJugadores.OnListChanged += OnListaJugadoresModificada;

            if (IsServer)
            {
                _vidaInicialGlobal.Value = vidaInicialPredeterminada;
                if (timeSection != null) timeSection.IniciarCronometroMaestro();

                Debug.Log("<color=orange>[MATCH MANAGER] OnNetworkSpawn ejecutado en Servidor. Suscribiendo evento de carga de escena...</color>");
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnTodasLasEscenasCargadas;
            }
        }

        private void OnTodasLasEscenasCargadas(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, System.Collections.Generic.List<ulong> clientsCompleted, System.Collections.Generic.List<ulong> clientsTimedOut)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnTodasLasEscenasCargadas;

            if (IsServer)
            {
                Debug.Log($"<color=white>[MATCH MANAGER] ¡Sincronización de escena Completa! Clientes que cargaron exitosamente: {clientsCompleted.Count}. Procediendo al Spawn...</color>");
                SpawnearJugadoresDeLaPartida();
            }
        }

        private void SpawnearJugadoresDeLaPartida()
        {
            if (!IsServer) return;

            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong idCliente = cliente.ClientId;
                Vector3 posicion = (idCliente == 0 && puntoSpawnJugador0 != null) ? puntoSpawnJugador0.position : 
                                   (puntoSpawnJugador1 != null) ? puntoSpawnJugador1.position : Vector3.zero;

                GameObject nuevoPollo = Instantiate(chickenPrefab, posicion, Quaternion.identity);

                string nombreOficial = "Jugador_" + idCliente;
                PlayerIdentity identidad = nuevoPollo.GetComponent<PlayerIdentity>();
                if (identidad != null)
                {
                    identidad.NombreIdentificador = nombreOficial;
                }

                RegistrarNuevoJugador(nombreOficial);

                Debug.Log($"<color=yellow>[MATCH MANAGER - SERVER] Instanciando {nombreOficial} (Client ID: {idCliente}) en posición {posicion}. Invocando SpawnWithOwnership...</color>");
                
                nuevoPollo.GetComponent<NetworkObject>().SpawnWithOwnership(idCliente);
            }
        }

        public void RegistrarNuevoJugador(string nombre)
        {
            if (!IsServer) return;
            PlayerData nuevoJugador = new PlayerData { nombreJugador = nombre, vidaActual = _vidaInicialGlobal.Value };
            _listaJugadores.Add(nuevoJugador);
        }
        
        // 🔍 DE VUELTA A LA VIDA: El método que necesita tu PlayerPunchReceiver para aplicar daño
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

        private void OnListaJugadoresModificada(NetworkListEvent<PlayerData> changeEvent) { AlModificarListaJugadores?.Invoke(changeEvent); }
        public override void OnNetworkDespawn() { _listaJugadores.OnListChanged -= OnListaJugadoresModificada; }
    }
}
