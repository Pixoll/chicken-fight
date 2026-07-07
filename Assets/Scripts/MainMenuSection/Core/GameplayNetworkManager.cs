using System;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace MainMenuSection.Core {
    public class GameplayNetworkManager : MonoBehaviour {
        public static GameplayNetworkManager Instance { get; private set; }

        private UnityTransport _transport;

        public bool IsPlayer1 { get; private set; }
        public bool IsPlayer2 => !IsPlayer1;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            if (NetworkManager.Singleton != null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }
        }

        public void CreateHost() {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsServer ||
                NetworkManager.Singleton.IsClient) {
                return;
            }

            if (_transport == null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }

            // Suscribimos el evento de inicio del servidor para detectar al Host como jugador inicial
            NetworkManager.Singleton.OnServerStarted += OnHostStartedLocal;

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            IsPlayer1 = true;

            string ipLocal = GetServerAddress();
            ushort puertoActual = GetCurrentPort();

            Debug.Log(
                $"<color=cyan>[GameplayNetworkManager] Intentando crear e iniciar un nuevo Host en {ipLocal}:{puertoActual}...</color>");
        }

        public void CloseConnection() {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;

            Debug.Log("<color=red>[GameplayNetworkManager] Cerrando conexión y apagando instancias de red.</color>");

            NetworkManager.Singleton.OnServerStarted -= OnHostStartedLocal;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnServerShutdown;
            NetworkManager.Singleton.Shutdown();
        }

        private ushort GetCurrentPort() {
            return _transport != null ? _transport.ConnectionData.Port : (ushort)0;
        }

        private string GetServerAddress() {
            return _transport != null ? _transport.ConnectionData.Address : "127.0.0.1";
        }

        private static string GetLocalIPv4() {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up
                    && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                )
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .FirstOrDefault() ?? "127.0.0.1";
        }

        private static string OctetsToHex(params string[] octets) {
            try {
                return string.Concat(octets.Select(octet => byte.Parse(octet).ToString("X2")));
            } catch {
                return "CÓDIGO_INVÁLIDO";
            }
        }

        public static string GetCurrentHostCode() {
            string[] octets = GetLocalIPv4().Split('.');
            return OctetsToHex(octets[2], octets[3]).ToUpper();
        }

        private static string HexToOctets(string hexCode) {
            try {
                if (hexCode.Length % 2 != 0) {
                    return null;
                }

                string result = string.Join(".", Enumerable.Range(0, hexCode.Length / 2)
                    .Select(i => byte.Parse(hexCode.Substring(i * 2, 2), NumberStyles.HexNumber).ToString()));

                return result;
            } catch {
                return null;
            }
        }

        public void JoinHost(string hostIdCode) {
            if (NetworkManager.Singleton == null) {
                return;
            }

            string ipWithoutLastOctets = string.Join(".", GetLocalIPv4().Split('.').SkipLast(2));
            string targetIp = string.Join(".", ipWithoutLastOctets, HexToOctets(hostIdCode.Trim()));

            if (string.IsNullOrEmpty(targetIp)) {
                return;
            }

            if (_transport == null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                if (_transport == null) return;
            }

            _transport.SetConnectionData(targetIp, 7777);
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerShutdown; 
            IsPlayer1 = false;

            NetworkManager.Singleton.StartClient();
        }

        public Action OnPlayerJoined;
        public Action OnPlayerLeft;
        public Action OnHostLeft;
        public Action OnJoinedRoom;

        public void ClearEventListeners() {
            OnPlayerJoined = null;
            OnPlayerLeft = null;
            OnHostLeft = null;
            OnJoinedRoom = null;
        }

        private void OnHostStartedLocal() {
            // Desuscribimos inmediatamente para evitar ejecuciones repetidas involuntarias
            NetworkManager.Singleton.OnServerStarted -= OnHostStartedLocal;

            // El Host local en Netcode siempre posee el LocalClientId (usualmente 0)
            ulong hostId = NetworkManager.Singleton.LocalClientId;
            ImprimirMensajeUnionJugador(hostId);
        }

        private void OnClientConnected(ulong clientId) {
            switch (NetworkManager.Singleton.IsServer) {
                // Este log se ejecutará localmente en cualquier instancia cuando se conecte con éxito
                case false:
                    ImprimirMensajeUnionJugador(clientId);
                    OnJoinedRoom?.Invoke();
                    break;

                case true when clientId != NetworkManager.Singleton.LocalClientId:
                    ImprimirMensajeUnionJugador(clientId);
                    OnPlayerJoined?.Invoke();
                    break;
            }
        }

        private void OnClientDisconnected(ulong clientId) {
            Debug.Log("<color=cyan>[GameplayNetworkManager] Client disconnected</color>");
            OnPlayerLeft?.Invoke();
        }

        private void OnServerShutdown(ulong clientId) {
            Debug.Log("<color=cyan>[GameplayNetworkManager] Host disconnected</color>");
            OnHostLeft?.Invoke();
        }

        private void ImprimirMensajeUnionJugador(ulong nuevoClientId) {
            string listaJugadores = "Ninguno";

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.ConnectedClientsIds != null) {
                // Obtenemos los IDs únicos de red de todos los jugadores actuales en el host
                listaJugadores = string.Join(", ",
                    NetworkManager.Singleton.ConnectedClientsIds.Select(id => $"[ID: {id}]"));
            }

            Debug.Log(
                $"<color=green>[GameplayNetworkManager] Se ha unido un nuevo jugador (ID asignado: {nuevoClientId}). " +
                $"Jugadores actuales en el host: {listaJugadores}</color>");
        }
    }
}
