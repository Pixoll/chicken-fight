using System.Collections;
using System.Collections.Generic;
using MultiPlayerSection.GameplayScripts;
using MultiPlayerSection.GameplayScripts.Objects;
using MultiPlayerSection.HUDScripts;
using MultiPlayerSection.PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.NetworkScripts
{
    public class MatchInformationManager : NetworkBehaviour
    {
        
        [Header("Sincronización de Fin de Partida")]
        private readonly NetworkVariable<bool> _partidaFinalizadaGlobal = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public bool PartidaFinalizada => _partidaFinalizadaGlobal.Value;
        public string IDJugadorGanador { get; private set; } = "Nadie";
        public int VidasPerdidasJugador0 { get; private set; } = 0;
        public int VidasPerdidasJugador1 { get; private set; } = 0;

        [Header("Referencias de Interfaz Final")]
        [SerializeField] private VentanaPantallaFinalUI ventanaFinalUI;
        
        [Header("Reglas Generales de Partida")]
        [SerializeField] private float vidaInicialPredeterminada = 100f;
        [SerializeField] private int maxRondasConfigurables = 5;
        [Tooltip("Puntos necesarios para ganar la partida completa (Al mejor de 5 = 3 puntos)")]
        [SerializeField] private int puntosParaGanarPartida = 3;

        [Header("Configuración de Spawn Manual")]
        [SerializeField] private GameObject chickenPrefab;
        [SerializeField] private Transform puntoSpawnJugador0;
        [SerializeField] private Transform puntoSpawnJugador1;

        private NetworkVariable<float> _vidaInicialGlobal = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkList<PlayerData> _listaJugadores;

        private int _rondaActual = 0;
        private Dictionary<ulong, int> _puntosDeVictoriaPorCliente = new Dictionary<ulong, int>();
        private Dictionary<ulong, GameObject> _referenciasGallinasInstanciadas = new Dictionary<ulong, GameObject>();
        
        private Dictionary<ulong, int> _vidasPerdidasPorCliente = new Dictionary<ulong, int>();

        public System.Action<NetworkListEvent<PlayerData>> AlModificarListaJugadores;
        private NetworkVariable<bool> _rondaComenzada = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake()
        {
            _listaJugadores = new NetworkList<PlayerData>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        }

        public override void OnNetworkSpawn()
        {
            _listaJugadores.OnListChanged += OnListaJugadoresModificada;

            if (IsServer)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnTodasLasEscenasCargadas;
            }
        }

        private void OnTodasLasEscenasCargadas(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnTodasLasEscenasCargadas;

            if (IsServer)
            {
                FuncionInicioPartida();
            }
        }

        private void FuncionInicioPartida()
        {
            if (!IsServer) return;

            Debug.Log("<color=green>[Match] -> Fase 1: FuncionInicioPartida. Seteando parámetros iniciales.</color>");
            
            _vidaInicialGlobal.Value = vidaInicialPredeterminada;
            _rondaComenzada.Value = false;
            _rondaActual = 0; 
            _puntosDeVictoriaPorCliente.Clear();
            _referenciasGallinasInstanciadas.Clear();
            _vidasPerdidasPorCliente.Clear();

            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong idCliente = cliente.ClientId;
                _puntosDeVictoriaPorCliente[idCliente] = 0; 
                _vidasPerdidasPorCliente[idCliente] = 0;
                RegistrarNuevoJugador(idCliente.ToString());
            }

            FuncionInicioRonda();
        }

        public void FuncionInicioRonda()
        {
            if (!IsServer) return;
            StartCoroutine(SecuenciaInicioRondaRoutine());
        }

        private IEnumerator SecuenciaInicioRondaRoutine()
        {
            _rondaActual++;
            Debug.Log($"<color=cyan>[Match] -> Fase 2: FuncionInicioRonda. Preparando ROUND {_rondaActual}.</color>");

            if (GlobalGameStateManager.Instance != null)
            {
                GlobalGameStateManager.Instance.LlamarCortinaCargaServer();
            }

            yield return new WaitForSeconds(0.2f);

            EjecutarRespawnerSeguro();

            yield return new WaitForSeconds(2.8f);

            if (GlobalGameStateManager.Instance != null)
            {
                GlobalGameStateManager.Instance.IniciarRondaServer($"ROUND {_rondaActual}");
            }
        }

        public void FuncionFinRonda(string nombreGanadorRound)
        {
            if (!IsServer) return;

            _rondaComenzada.Value = false; 

            Debug.Log($"<color=red>[Match] -> Fase 3: FuncionFinRonda. Deteniendo daño. Ganador del Round: {nombreGanadorRound}.</color>");

            if (GlobalGameStateManager.Instance != null)
            {
                GlobalGameStateManager.Instance.FinDeRondaServer($"GANADOR {nombreGanadorRound}");
            }

            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                RestablecerVidaSpecificaServidor(cliente.ClientId.ToString(), _vidaInicialGlobal.Value);
            }

            ulong idGanadorComprobacion = ulong.Parse(nombreGanadorRound);
            if (_puntosDeVictoriaPorCliente.ContainsKey(idGanadorComprobacion) && 
                _puntosDeVictoriaPorCliente[idGanadorComprobacion] >= puntosParaGanarPartida)
            {
                Debug.Log($"<color=gold>[Match Definitive] -> ¡PARTIDA FINALIZADA! El jugador {nombreGanadorRound} ha alcanzado los {puntosParaGanarPartida} puntos de victoria.</color>");
                
                FinalizarPartidaDefinitivamenteServer(nombreGanadorRound);
                return;
            }

            Invoke(nameof(FuncionInicioRonda), 3f);
        }


        private void FinalizarPartidaDefinitivamenteServer(string nombreGanadorFinal)
        {
            if (!IsServer) return;

            VidasPerdidasJugador0 = _vidasPerdidasPorCliente.ContainsKey(0) ? _vidasPerdidasPorCliente[0] : 0;
            VidasPerdidasJugador1 = _vidasPerdidasPorCliente.ContainsKey(1) ? _vidasPerdidasPorCliente[1] : 0;

            string nombreJ0 = "Jugador 0";
            string nombreJ1 = "Jugador 1";

            for (int i = 0; i < _listaJugadores.Count; i++)
            {
                if (_listaJugadores[i].nombreJugador.ToString() == "0") nombreJ0 = "Jugador 0";
                else if (_listaJugadores[i].nombreJugador.ToString() == "1") nombreJ1 = "Jugador 1";
                else
                {
                    if (i == 0) nombreJ0 = _listaJugadores[i].nombreJugador.ToString();
                    if (i == 1) nombreJ1 = _listaJugadores[i].nombreJugador.ToString();
                }
            }

            string ganadorFormateado = nombreGanadorFinal;
            if (nombreGanadorFinal == "0") ganadorFormateado = "Jugador 0";
            if (nombreGanadorFinal == "1") ganadorFormateado = "Jugador 1";

            IDJugadorGanador = ganadorFormateado;
            _partidaFinalizadaGlobal.Value = true;

            DesplegarInterfazFinalRpc(nombreJ0, nombreJ1, ganadorFormateado, VidasPerdidasJugador0, VidasPerdidasJugador1);
        }

        [Rpc(SendTo.Everyone)]
        private void DesplegarInterfazFinalRpc(
            Unity.Collections.FixedString32Bytes nombreJ0, 
            Unity.Collections.FixedString32Bytes nombreJ1, 
            Unity.Collections.FixedString32Bytes nombreGanador, 
            int muertesJ0, 
            int muertesJ1)
        {
            IDJugadorGanador = nombreGanador.ToString();
            VidasPerdidasJugador0 = muertesJ0;
            VidasPerdidasJugador1 = muertesJ1;

            Debug.Log($"<color=lime>[UI Fin] -> Mostrando Overview final. Ganador: {IDJugadorGanador}</color>");

            if (ventanaFinalUI != null)
            {
                ventanaFinalUI.InicializarYMostrarPantallaFinal(nombreJ0.ToString(), nombreJ1.ToString(), IDJugadorGanador, muertesJ0, muertesJ1);
            }
        }

        private void EjecutarRespawnerSeguro()
        {
            if (!IsServer) return;

            _rondaComenzada.Value = true;

            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong idCliente = cliente.ClientId;
                Vector3 posicionSpawn = (idCliente == 0 && puntoSpawnJugador0 != null) ? puntoSpawnJugador0.position : 
                                        (puntoSpawnJugador1 != null) ? puntoSpawnJugador1.position : Vector3.zero;

                GameObject gallina;

                if (!_referenciasGallinasInstanciadas.ContainsKey(idCliente) || _referenciasGallinasInstanciadas[idCliente] == null)
                {
                    gallina = Instantiate(chickenPrefab, posicionSpawn, Quaternion.identity);
                    _referenciasGallinasInstanciadas[idCliente] = gallina;

                    string nombreOficial = idCliente.ToString();
                    PlayerIdentity identidad = gallina.GetComponent<PlayerIdentity>();
                    if (identidad != null) identidad.NombreIdentificador = nombreOficial;

                    if (gallina.TryGetComponent<NetworkObject>(out var netObjGallina))
                    {
                        netObjGallina.SpawnWithOwnership(idCliente);
                    }
                }
                else
                {
                    gallina = _referenciasGallinasInstanciadas[idCliente];
                }

                PlayerObjectAttackManager attackManager = gallina.GetComponentInChildren<PlayerObjectAttackManager>();
                if (attackManager != null)
                {
                    attackManager.ResetearAlGolpeBase();
                }

                PlayerMovement movement = gallina.GetComponentInChildren<PlayerMovement>();
                if (movement != null)
                {
                    movement.TeletransportarGallina(posicionSpawn);
                }
                else
                {
                    gallina.transform.position = posicionSpawn;
                }

                RestablecerVidaSpecificaServidor(idCliente.ToString(), _vidaInicialGlobal.Value);
                
                if (gallina.TryGetComponent<NetworkObject>(out var netObjEstado))
                {
                    ConfigurarEstadoGallinaRpc(netObjEstado, true);
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void ModificarVidaJugadorServerRpc(string nombreUnico, float cantidad)
        {
            if (_bloqueoDeDanioVentanaCarga) return;

            if (!_rondaComenzada.Value) return;

            int indexJugadorModificado = -1;

            for (int i = 0; i < _listaJugadores.Count; i++)
            {
                if (_listaJugadores[i].nombreJugador.ToString() == nombreUnico)
                {
                    PlayerData datosModificados = _listaJugadores[i];
                    float vidaAnterior = datosModificados.vidaActual;

                    datosModificados.vidaActual += cantidad;

                    if (datosModificados.vidaActual < 0f) datosModificados.vidaActual = 0f;
                    if (datosModificados.vidaActual > _vidaInicialGlobal.Value) datosModificados.vidaActual = _vidaInicialGlobal.Value;

                    _listaJugadores[i] = datosModificados; 
                    indexJugadorModificado = i;

                    Debug.Log($"<color=magenta>[Daño] -> Jugador {nombreUnico} recibió alteración de vida: ({cantidad}). HP Anterior: {vidaAnterior} -> HP Actual: {datosModificados.vidaActual}</color>");
                    break;
                }
            }

            if (indexJugadorModificado != -1 && _listaJugadores[indexJugadorModificado].vidaActual <= 0f)
            {
                _rondaComenzada.Value = false;

                ulong idGanador = 0;
                string nombreGanador = "Nadie";

                ulong idPerdedor = ulong.Parse(nombreUnico);
                if (_vidasPerdidasPorCliente.ContainsKey(idPerdedor))
                {
                    _vidasPerdidasPorCliente[idPerdedor]++;
                }
                else
                {
                    _vidasPerdidasPorCliente[idPerdedor] = 1;
                }

                foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
                {
                    if (cliente.ClientId.ToString() != nombreUnico)
                    {
                        idGanador = cliente.ClientId;
                        nombreGanador = idGanador.ToString();
                        break;
                    }
                }

                if (_puntosDeVictoriaPorCliente.ContainsKey(idGanador))
                {
                    _puntosDeVictoriaPorCliente[idGanador]++;
                }
                else
                {
                    _puntosDeVictoriaPorCliente[idGanador] = 1;
                }

                Debug.Log($"<color=red>[Match] -> ¡Muerte detectada! Jugador {nombreUnico} cae a 0 HP (Vidas perdidas en total: {_vidasPerdidasPorCliente[idPerdedor]}). Ganador del Round: {nombreGanador}. Saltando a FaseFinRonda.</color>");

                FuncionFinRonda(nombreGanador);
            }
        }

        [Rpc(SendTo.Everyone)]
        private void ConfigurarEstadoGallinaRpc(NetworkObjectReference gallinaRef, bool activaParaJugar)
        {
            if (gallinaRef.TryGet(out NetworkObject netObject))
            {
                SpriteRenderer[] renderers = netObject.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in renderers) sr.enabled = activaParaJugar;

                Canvas[] uiLocales = netObject.GetComponentsInChildren<Canvas>(true);
                foreach (var canvas in uiLocales) canvas.enabled = activaParaJugar;

                Rigidbody2D rb = netObject.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.bodyType = activaParaJugar ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
                }
            }
        }

        private void RestablecerVidaSpecificaServidor(string nombreUnico, float vidaSincronizada)
        {
            for (int i = 0; i < _listaJugadores.Count; i++)
            {
                if (_listaJugadores[i].nombreJugador.ToString() == nombreUnico)
                {
                    PlayerData datos = _listaJugadores[i];
                    datos.vidaActual = vidaSincronizada;
                    _listaJugadores[i] = datos;
                    break;
                }
            }
        }

        public void RegistrarNuevoJugador(string nombre)
        {
            if (!IsServer) return;
            PlayerData nuevoJugador = new PlayerData { nombreJugador = nombre, vidaActual = _vidaInicialGlobal.Value };
            _listaJugadores.Add(nuevoJugador);
        }
        
        public void ModificarVidaJugador(string nombreUnico, float cantidad)
        {
            if (!IsServer) return;
            ModificarVidaJugadorServerRpc(nombreUnico, cantidad);
        }

        public bool EstaLaRondaActiva() { return _rondaComenzada.Value; }
        private void OnListaJugadoresModificada(NetworkListEvent<PlayerData> changeEvent) { AlModificarListaJugadores?.Invoke(changeEvent); }
        public override void OnNetworkDespawn() { _listaJugadores.OnListChanged -= OnListaJugadoresModificada; }
        
        public int ObtenerVidasPerdidasDeCliente(ulong idCliente) => _vidasPerdidasPorCliente.ContainsKey(idCliente) ? _vidasPerdidasPorCliente[idCliente] : 0;
        public int ObtenerPuntosDeCliente(ulong idCliente) => _puntosDeVictoriaPorCliente.ContainsKey(idCliente) ? _puntosDeVictoriaPorCliente[idCliente] : 0;

        public int RondaActual => _rondaActual;
        public NetworkList<PlayerData> ListaJugadores => _listaJugadores;
        
        [Header("Configuración de Salida")]
        [Tooltip("Nombre exacto de la escena del menú principal para redirigir")]
        [SerializeField] private string escenaMenuPrincipal = "MainMenu"; 

        public void SolicitarSalirDePartidaGlobal()
        {
            if (!IsServer)
            {
                SolicitarSalirDePartidaServerRpc();
            }
            else
            {
                EjecutarSalidaSincronizadaEnTodosRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SolicitarSalirDePartidaServerRpc()
        {
            EjecutarSalidaSincronizadaEnTodosRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void EjecutarSalidaSincronizadaEnTodosRpc()
        {
            _rondaComenzada.Value = false;
            Debug.Log("<color=orange>[Match Out] -> Orden de salida recibida. Activando pantalla de carga local...</color>");

            foreach (var registro in _referenciasGallinasInstanciadas)
            {
                GameObject gallina = registro.Value;
                if (gallina != null)
                {
                    SpriteRenderer[] renderers = gallina.GetComponentsInChildren<SpriteRenderer>(true);
                    foreach (var sr in renderers) sr.enabled = false;

                    Canvas[] uiLocales = gallina.GetComponentsInChildren<Canvas>(true);
                    foreach (var canvas in uiLocales) canvas.enabled = false;

                    gallina.SetActive(false);
                }
            }

            if (GlobalGameStateManager.Instance != null)
            {
                GlobalGameStateManager.Instance.LlamarCortinaCargaServer(); 
            }

            StartCoroutine(RutinaRetrasoSalidaLocal());
        }

        private IEnumerator RutinaRetrasoSalidaLocal()
        {
            yield return new WaitForSeconds(1.0f);

            if (IsServer)
            {
                Debug.Log($"<color=cyan>[Match Out] -> Servidor ordenando cambio de escena global a {escenaMenuPrincipal}...</color>");
                
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnRegresoAlMenuCompletado;
                NetworkManager.Singleton.SceneManager.LoadScene(escenaMenuPrincipal, UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }

        private void OnRegresoAlMenuCompletado(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (sceneName == escenaMenuPrincipal)
            {
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnRegresoAlMenuCompletado;

                Debug.Log("<color=red>[Match Out] -> Todos a salvo en 'MainMenu'. Apagando conexiones de red de forma segura.</color>");
                
                if (MainMenuSection.GameplayNetworkManager.Instance != null)
                {
                    MainMenuSection.GameplayNetworkManager.Instance.CloseConnection();
                }
            }
        }
        private bool _bloqueoDeDanioVentanaCarga = false;

        public void BloquearDescuentoDeVida()
        {
            _bloqueoDeDanioVentanaCarga = true;
            Debug.Log("<color=yellow>[Match Manager] -> Daño bloqueado localmente por ventana de carga.</color>");
        }

        public void ActivarDescuentoDeVida()
        {
            _bloqueoDeDanioVentanaCarga = false;
            Debug.Log("<color=green>[Match Manager] -> Daño reactivado. Los jugadores vuelven a recibir impactos.</color>");
        }
    }
}
