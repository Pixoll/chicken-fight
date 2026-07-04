using System.Collections;
using System.Collections.Generic;
using MultiPlayerSection.GameplayScripts;
using MultiPlayerSection.PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.NetworkScripts
{
    public class MatchInformationManager : NetworkBehaviour
    {
        [Header("Reglas Generales de Partida")]
        [SerializeField] private float vidaInicialPredeterminada = 100f;
        [SerializeField] private int maxRondasConfigurables = 5;

        [Header("Configuración de Spawn Manual")]
        [SerializeField] private GameObject chickenPrefab;
        [SerializeField] private Transform puntoSpawnJugador0;
        [SerializeField] private Transform puntoSpawnJugador1;

        private NetworkVariable<float> _vidaInicialGlobal = new NetworkVariable<float>(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkList<PlayerData> _listaJugadores;

        private int _rondaActual = 0;
        private Dictionary<ulong, int> _puntosDeVictoriaPorCliente = new Dictionary<ulong, int>();
        private Dictionary<ulong, GameObject> _referenciasGallinasInstanciadas = new Dictionary<ulong, GameObject>();

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

            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong idCliente = cliente.ClientId;
                _puntosDeVictoriaPorCliente[idCliente] = 0; 
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

            Debug.Log($"<color=red>[Match] -> Fase 3: FuncionFinRonda. Deteniendo daño. Ganador: {nombreGanadorRound}.</color>");

            if (GlobalGameStateManager.Instance != null)
            {
                GlobalGameStateManager.Instance.FinDeRondaServer($"GANADOR {nombreGanadorRound}");
            }

            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                RestablecerVidaSpecificaServidor(cliente.ClientId.ToString(), _vidaInicialGlobal.Value);
            }

            Invoke(nameof(FuncionInicioRonda), 3f);
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

                Debug.Log($"<color=red>[Match] -> ¡Muerte detectada! Jugador {nombreUnico} cae a 0 HP. Ganador del Round: {nombreGanador}. Saltando a FaseFinRonda.</color>");

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
        
        public int RondaActual => _rondaActual;
        public NetworkList<PlayerData> ListaJugadores => _listaJugadores;
    }
}
