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
        }

        private void Start() {
            // Vinculamos el transporte nativo de Unity Netcode
            if (NetworkManager.Singleton != null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }
        }

        /// <summary>
        /// Crea la sesión del Host e imprime inmediatamente los datos exactos del socket en consola.
        /// </summary>
        public void CreateHost() {
            if (
                NetworkManager.Singleton == null
                || NetworkManager.Singleton.IsServer
                || NetworkManager.Singleton.IsClient
            ) {
                return;
            }

            // Forzamos la actualización del componente antes de arrancar
            if (_transport == null) {
                _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            }

            Debug.Log("<color=green>[GameplayNetworkManager] Iniciando Host...</color>");

            NetworkManager.Singleton.StartHost();

            // IMPRESIÓN EXACTA EN DEBUG
            string ipLocal = GetLocalIPv4();
            string codigoLetras = IpToBase64(ipLocal);
            ushort puertoActual = GetCurrentPort();

            Debug.Log(
                "<color=cyan>[DEBUG RED] Host Creado Exitosamente.</color>\n"
                + $"▶ IP Física: {ipLocal}\n"
                + $"▶ Puerto Utilizado: {puertoActual}\n"
                + $"▶ Código de Letras (Base64): {codigoLetras}"
            );
        }

        /// <summary>
        /// Cierra el servidor e imprime la confirmación para asegurar que los puertos fueron liberados.
        /// </summary>
        public void CloseHost() {
            if (
                NetworkManager.Singleton == null
                || (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            ) {
                return;
            }

            ushort puertoLiberado = GetCurrentPort();

            NetworkManager.Singleton.Shutdown();

            Debug.Log($"<color=red>[DEBUG RED] Host Cerrado. Puerto {puertoLiberado} liberado correctamente.</color>");
        }

        /// <summary>
        /// Retorna el puerto exacto de red que está utilizando el transporte en este instante.
        /// </summary>
        public ushort GetCurrentPort() {
            if (_transport != null) {
                return _transport.ConnectionData.Port;
            }

            return 0;
        }

        /// <summary>
        /// Retorna la dirección IP limpia en texto string.
        /// </summary>
        public string GetServerAddress() {
            return _transport != null ? _transport.ConnectionData.Address : "0.0.0.0";
        }

        // ==========================================
        // MÉTODOS HEREDADOS DE TU CÓDIGO VIEJO
        // ==========================================

        private string GetLocalIPv4() {
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

        private string IpToBase64(string ip) {
            try {
                return Convert.ToBase64String(ip.Split('.').Select(byte.Parse).ToArray()).TrimEnd('=');
            } catch {
                return "CÓDIGO_INVÁLIDO";
            }
        }
    }
}
