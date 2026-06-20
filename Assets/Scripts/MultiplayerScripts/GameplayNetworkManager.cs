using System;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerScripts {
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

            Debug.Log(
                "<color=orange>[GameplayNetworkManager] Awake ejecutado. Instancia única creada con éxito.</color>");
        }

        private void Start() {
            if (NetworkManager.Singleton != null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

                Debug.Log(
                    "<color=orange>[GameplayNetworkManager] UnityTransport vinculado correctamente en el Start.</color>");
            } else {
                Debug.LogError(
                    "[GameplayNetworkManager] ¡CRÍTICO: No se encontró el NetworkManager Singleton en la escena!");
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

            Debug.Log("<color=green>[GameplayNetworkManager] Iniciando Host...</color>");

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            IsPlayer1 = true;

            // Usamos el método modificado que lee la IP fija
            string ipLocal = GetServerAddress();
            ushort puertoActual = GetCurrentPort();

            Debug.Log(
                "<color=cyan>[DEBUG RED - HOST CREADO]</color>\n"
                + $"▶ IP Física Detectada en PC: {ipLocal}\n"
                + $"▶ Puerto de Escucha: {puertoActual}\n"
                + "▶ Estado: Suscrito a eventos de conexión esperando clientes externos..."
            );
        }

        public void CloseConnection() {
            if (NetworkManager.Singleton == null) return;

            ushort puertoLiberado = GetCurrentPort();

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.Shutdown();

            Debug.Log(
                $"<color=red>[DEBUG RED] Host Cerrado de forma voluntaria. Puerto {puertoLiberado} liberado.</color>");
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
                Debug.LogError($"[GameplayNetworkManager] Falló la codificación de los octetos: {octets}");
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
                    Debug.LogError("[GameplayNetworkManager] Código hex tiene longitud inválida");
                    return null;
                }

                string result = string.Join(".", Enumerable.Range(0, hexCode.Length / 2)
                    .Select(i => byte.Parse(hexCode.Substring(i * 2, 2), NumberStyles.HexNumber).ToString()));

                Debug.Log($"<color=purple>[GameplayNetworkManager] Traduciendo Código '{hexCode}' -> Octetos: {result}</color>");

                return result;
            } catch (Exception ex) {
                Debug.LogError($"[GameplayNetworkManager] Error fatal al decodificar Hex: {ex.Message}");
                return null;
            }
        }

        public void JoinHost(string hostIdCode) {
            if (NetworkManager.Singleton == null) {
                Debug.LogError("[GameplayNetworkManager] No se puede conectar: NetworkManager.Singleton es NULL.");
                return;
            }

            Debug.Log(
                $"<color=yellow>[GameplayNetworkManager] Solicitud de unión recibida. Código ingresado por el usuario: {hostIdCode}</color>");

            string ipWithoutLastOctets = string.Join(".", GetLocalIPv4().Split('.').SkipLast(2));
            string targetIp = string.Join(".", ipWithoutLastOctets, HexToOctets(hostIdCode.Trim()));

            if (string.IsNullOrEmpty(targetIp)) {
                Debug.LogError(
                    "[GameplayNetworkManager] Abortando JoinHost: La dirección IP recuperada está vacía o corrupta.");

                return;
            }

            if (_transport == null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }

            if (_transport != null) {
                _transport.SetConnectionData(targetIp, 7777);

                Debug.Log(
                    $"<color=yellow>[GameplayNetworkManager] Inyectando IP en UnityTransport. Address = {targetIp}, Port = {_transport.ConnectionData.Port}</color>");

                // Nos suscribimos también en el cliente para ver si él mismo se entera de su éxito
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                IsPlayer1 = false;

                Debug.Log(
                    "<color=orange>[GameplayNetworkManager] Lanzando NetworkManager.Singleton.StartClient()...</color>");

                bool clienteLanzado = NetworkManager.Singleton.StartClient();
                Debug.Log($"[GameplayNetworkManager] Resultado de StartClient(): {clienteLanzado}");
            } else {
                Debug.LogError(
                    "[GameplayNetworkManager] No se pudo configurar la IP porque UnityTransport es NULL en el NetworkManager.");
            }
        }

        public Action<ulong> OnPlayerJoined;

        private void OnClientConnected(ulong clientId) {
            // Este log se imprimirá SI O SI si la red conecta, sin importar si eres host o cliente
            Debug.Log(
                $"<color=magenta>=========== [ALERTA NATIVA NETCODE] OnClientConnected disparado para ID: {clientId} ===========</color>");

            Debug.Log($"[INFO] LocalClientId de esta instancia actual es: {NetworkManager.Singleton.LocalClientId}");

            if (clientId != NetworkManager.Singleton.LocalClientId) {
                Debug.Log(
                    $"<color=green>[GameplayNetworkManager] ¡ÉXITO! Un cliente real (ID externo: {clientId}) superó el handshake de red y entró al Host.</color>");

                OnPlayerJoined?.Invoke(clientId);
            } else {
                Debug.Log(
                    "[GameplayNetworkManager] OnClientConnected detectó la autoconexión de la instancia local (Host conectándose a sí mismo). Ignorando para la UI externa.");
            }

            SceneManager.LoadScene("RoundMode");
        }

        private void OnClientDisconnected(ulong clientId) {
            Debug.Log(
                $"<color=red>[GameplayNetworkManager] Callback nativo: El ID {clientId} se desconectó del servidor.</color>");
        }
    }
}
