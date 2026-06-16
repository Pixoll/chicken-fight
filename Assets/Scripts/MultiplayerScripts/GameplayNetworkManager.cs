using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace MultiplayerScripts {
    public class GameplayNetworkManager : MonoBehaviour {
        public static GameplayNetworkManager Instance { get; private set; }

        private UnityTransport _transport;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("<color=orange>[GameplayNetworkManager] Awake ejecutado. Instancia única creada con éxito.</color>");
        }

        private void Start() {
            if (NetworkManager.Singleton != null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                Debug.Log("<color=orange>[GameplayNetworkManager] UnityTransport vinculado correctamente en el Start.</color>");
            } else {
                Debug.LogError("[GameplayNetworkManager] ¡CRÍTICO: No se encontró el NetworkManager Singleton en la escena!");
            }
        }

        public void CreateHost() {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient) {
                return;
            }

            if (_transport == null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }

            // =======================================================
            // FUERZA TU IP REAL AQUÍ ANTES DE ABRIR EL PUERTO
            // =======================================================
            string ipRealWindows = "10.143.137.111"; 
            if (_transport != null) {
                _transport.ConnectionData.Address = ipRealWindows;
            }
            // =======================================================

            Debug.Log("<color=green>[GameplayNetworkManager] Iniciando Host...</color>");

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Usamos el método modificado que lee la IP fija
            string ipLocal = ipRealWindows;
            string codigoLetras = IpToBase64(ipLocal);
            ushort puertoActual = GetCurrentPort();

            Debug.Log(
                "<color=cyan>[DEBUG RED - HOST CREADO]</color>\n"
                + $"▶ IP Física Detectada en PC: {ipLocal}\n"
                + $"▶ Puerto de Escucha: {puertoActual}\n"
                + $"▶ Código Generado (Base64): {codigoLetras}\n"
                + "▶ Estado: Suscrito a eventos de conexión esperando clientes externos..."
            );
        }

        public void CloseHost() {
            if (NetworkManager.Singleton == null) return;

            ushort puertoLiberado = GetCurrentPort();

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.Shutdown();

            Debug.Log($"<color=red>[DEBUG RED] Host Cerrado de forma voluntaria. Puerto {puertoLiberado} liberado.</color>");
        }

        public ushort GetCurrentPort() {
            return _transport != null ? _transport.ConnectionData.Port : (ushort)0;
        }

        public string GetServerAddress() {
            return _transport != null ? _transport.ConnectionData.Address : "0.0.0.0";
        }

        private string GetLocalIPv4() {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .FirstOrDefault() ?? "127.0.0.1";
        }

        private string IpToBase64(string ip) {
            try {
                return Convert.ToBase64String(ip.Split('.').Select(byte.Parse).ToArray()).TrimEnd('=');
            } catch {
                Debug.LogError($"[GameplayNetworkManager] Falló la codificación de la IP: {ip}");
                return "CÓDIGO_INVÁLIDO";
            }
        }
        
        public string GetCurrentHostCode() {
            string ipRealWindows = "10.143.137.111";
            return IpToBase64(ipRealWindows);
        }
        
        private string Base64ToIp(string base64Code) {
            try {
                int mod = base64Code.Length % 4;
                if (mod > 0) base64Code += new string('=', 4 - mod);

                byte[] bytes = Convert.FromBase64String(base64Code);
                string ipDecodificada = string.Join(".", bytes.Select(b => b.ToString()));
                Debug.Log($"<color=purple>[GameplayNetworkManager] Traduciendo Código '{base64Code}' -> IP Real: {ipDecodificada}</color>");
                return ipDecodificada;
            } catch (Exception ex) {
                Debug.LogError($"[GameplayNetworkManager] Error fatal al decodificar Base64: {ex.Message}");
                return null;
            }
        }
        
        public void JoinHost(string hostIdCode) {
            if (NetworkManager.Singleton == null) {
                Debug.LogError("[GameplayNetworkManager] No se puede conectar: NetworkManager.Singleton es NULL.");
                return;
            }

            Debug.Log($"<color=yellow>[GameplayNetworkManager] Solicitud de unión recibida. Código ingresado por el usuario: {hostIdCode}</color>");

            string targetIp = Base64ToIp(hostIdCode.Trim());

            if (string.IsNullOrEmpty(targetIp)) {
                Debug.LogError("[GameplayNetworkManager] Abortando JoinHost: La dirección IP recuperada está vacía o corrupta.");
                return;
            }

            if (_transport == null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }

            if (_transport != null) {
                _transport.ConnectionData.Address = targetIp;
                Debug.Log($"<color=yellow>[GameplayNetworkManager] Inyectando IP en UnityTransport. Address = {targetIp}, Port = {_transport.ConnectionData.Port}</color>");
        
                // Nos suscribimos también en el cliente para ver si él mismo se entera de su éxito
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                
                Debug.Log("<color=orange>[GameplayNetworkManager] Lanzando NetworkManager.Singleton.StartClient()...</color>");
                bool clienteLanzado = NetworkManager.Singleton.StartClient();
                Debug.Log($"[GameplayNetworkManager] Resultado de StartClient(): {clienteLanzado}");
            } else {
                Debug.LogError("[GameplayNetworkManager] No se pudo configurar la IP porque UnityTransport es NULL en el NetworkManager.");
            }
        }
        
        public System.Action<ulong> OnPlayerJoined;

        private void OnClientConnected(ulong clientId) {
            // Este log se imprimirá SI O SI si la red conecta, sin importar si eres host o cliente
            Debug.Log($"<color=magenta>=========== [ALERTA NATIVA NETCODE] OnClientConnected disparado para ID: {clientId} ===========</color>");
            Debug.Log($"[INFO] LocalClientId de esta instancia actual es: {NetworkManager.Singleton.LocalClientId}");

            if (clientId != NetworkManager.Singleton.LocalClientId) {
                Debug.Log($"<color=green>[GameplayNetworkManager] ¡ÉXITO! Un cliente real (ID externo: {clientId}) superó el handshake de red y entró al Host.</color>");
                OnPlayerJoined?.Invoke(clientId);
            } else {
                Debug.Log("[GameplayNetworkManager] OnClientConnected detectó la autoconexión de la instancia local (Host conectándose a sí mismo). Ignorando para la UI externa.");
            }
        }

        private void OnClientDisconnected(ulong clientId) {
            Debug.Log($"<color=red>[GameplayNetworkManager] Callback nativo: El ID {clientId} se desconectó del servidor.</color>");
        }
    }
}