using System.Collections;
using System.Collections.Generic;
using MultiPlayerSection.GameplayScripts;
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
        [SerializeField] private int maxRondasConfigurables = 5;

        [Header("Configuración de Spawn Manual")]
        [SerializeField] private GameObject chickenPrefab;
        [SerializeField] private Transform puntoSpawnJugador0;
        [SerializeField] private Transform puntoSpawnJugador1;

        [Header("Referencias Locales")]
        [SerializeField] private GameTimeSection timeSection;

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
            if (timeSection == null) timeSection = GetComponentInChildren<GameTimeSection>();
        }

        public override void OnNetworkSpawn()
        {
            _listaJugadores.OnListChanged += OnListaJugadoresModificada;

            if (IsServer)
            {
                _vidaInicialGlobal.Value = vidaInicialPredeterminada;
                _rondaComenzada.Value = false;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnTodasLasEscenasCargadas;
            }
        }

        private void OnTodasLasEscenasCargadas(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnTodasLasEscenasCargadas;

            if (IsServer)
            {
                StartCoroutine(FaseCreacionDeEscenaRoutine());
            }
        }

        private IEnumerator FaseCreacionDeEscenaRoutine()
        {
            if (!IsServer) yield break;

            _rondaComenzada.Value = false;

            string logJugadoresUnidos = "[MatchInformationManager] Jugadores que entraron a la escena: ";
            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                logJugadoresUnidos += $"[ID Cliente: {cliente.ClientId}] ";
            }
            Debug.Log($"<color=white>{logJugadoresUnidos}</color>");
            Debug.Log("<color=yellow>[MatchInformationManager] Se inicia fase de creacion de scena. Configurando gallinas invisibles y reiniciando datos...</color>");

            _rondaActual = 1;
            _puntosDeVictoriaPorCliente.Clear();
            _referenciasGallinasInstanciadas.Clear();

            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong idCliente = cliente.ClientId;
                _puntosDeVictoriaPorCliente[idCliente] = 0; 

                Vector3 posicionInicial = (idCliente == 0 && puntoSpawnJugador0 != null) ? puntoSpawnJugador0.position : 
                    (puntoSpawnJugador1 != null) ? puntoSpawnJugador1.position : Vector3.zero;

                GameObject nuevoPollo = Instantiate(chickenPrefab, posicionInicial, Quaternion.identity);
                _referenciasGallinasInstanciadas[idCliente] = nuevoPollo;

                string nombreOficial = idCliente.ToString();
                
                PlayerIdentity identidad = nuevoPollo.GetComponent<PlayerIdentity>();
                if (identidad != null)
                {
                    identidad.NombreIdentificador = nombreOficial;
                }

                RegistrarNuevoJugador(nombreOficial);
        
                nuevoPollo.GetComponent<NetworkObject>().SpawnWithOwnership(idCliente);
            }

            yield return new WaitForSeconds(3.0f);

            FaseInicioRound();
        }


        public void FaseInicioRound()
        {
            if (!IsServer) return;

            Debug.Log($"<color=orange>[MatchInformationManager] Se inicia la fase de inicio de round. Comenzando Ronda: {_rondaActual}. ¡Gallinas visibles!</color>");

            _rondaComenzada.Value = true;

            foreach (var item in _referenciasGallinasInstanciadas)
            {
                ulong idCliente = item.Key;
                GameObject gallina = item.Value;

                if (gallina != null)
                {
                    Vector3 posicionSpawn = (idCliente == 0 && puntoSpawnJugador0 != null) ? puntoSpawnJugador0.position : 
                                            (puntoSpawnJugador1 != null) ? puntoSpawnJugador1.position : Vector3.zero;
                    
                    gallina.transform.position = posicionSpawn;

                    ConfigurarEstadoGallinaRpc(gallina.GetComponent<NetworkObject>(), true);

                    string nombreBuscado = idCliente.ToString();
                    RestablecerVidaSpecificaServidor(nombreBuscado, _vidaInicialGlobal.Value);
                }
            }

            foreach (var item in _referenciasGallinasInstanciadas)
            {
                GameObject gallina = item.Value;
                if (gallina != null)
                {
                    PlayerIdentity identity = gallina.GetComponent<PlayerIdentity>();
                    string nombreFisico = identity != null ? identity.NombreIdentificador : "Desconocido";
                    
                    float vidaActualFisica = 0f;
                    for (int i = 0; i < _listaJugadores.Count; i++)
                    {
                        if (_listaJugadores[i].nombreJugador.ToString() == nombreFisico)
                        {
                            vidaActualFisica = _listaJugadores[i].vidaActual;
                            break;
                        }
                    }
                    Debug.Log($"<color=lime>[MatchInformationManager] Verificación Instancia Real -> PlayerIdentity: {nombreFisico} | Vida actual en Red: {vidaActualFisica}</color>");
                }
            }

            if (timeSection != null)
            {
                timeSection.IniciarCronometroMaestro();
            }
        }

        public void FaseFinRound(string nombreGanadorRound)
        {
            if (!IsServer) return;

            _rondaComenzada.Value = false;

            foreach (var cliente in NetworkManager.Singleton.ConnectedClientsList)
            {
                string nombreEsperado = cliente.ClientId.ToString();
                if (nombreEsperado == nombreGanadorRound)
                {
                    _puntosDeVictoriaPorCliente[cliente.ClientId]++;
                    break;
                }
            }

            _rondaActual++;

            if (_rondaActual <= maxRondasConfigurables)
            {
                FaseInicioRound();
            }
            else
            {
                Debug.Log("<color=red>[MatchInformationManager] Se ha alcanzado el límite de rondas. Fin de la partida.</color>");
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
                    if (activaParaJugar)
                    {
                        rb.bodyType = RigidbodyType2D.Dynamic;
                        rb.linearVelocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                    }
                    else
                    {
                        rb.bodyType = RigidbodyType2D.Kinematic;
                        rb.linearVelocity = Vector2.zero;
                        rb.angularVelocity = 0f;
                    }
                }
            }
        }

        public bool EstaLaRondaActiva()
        {
            return _rondaComenzada.Value;
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

                    if (datosModificados.vidaActual <= 0f)
                    {
                        string nombreGanador = (nombreUnico == "0") ? "1" : "0";
                        FaseFinRound(nombreGanador);
                    }
                    break;
                }
            }
        }

        private void OnListaJugadoresModificada(NetworkListEvent<PlayerData> changeEvent) { AlModificarListaJugadores?.Invoke(changeEvent); }
        public override void OnNetworkDespawn() { _listaJugadores.OnListChanged -= OnListaJugadoresModificada; }
    }
}
