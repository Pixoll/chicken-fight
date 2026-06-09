using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MultiplayerScripts
{
    public class ConnectUI : MonoBehaviour {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button clientButton;
        [SerializeField] private Button singleplayerButton;
        [SerializeField] private GameObject inGameUI;
        [SerializeField] private GameObject multiplayerMenu;

        [FormerlySerializedAs("ipInputField")] [SerializeField]
        private TMP_InputField codeInputField;

        [FormerlySerializedAs("localIPDisplay")] [SerializeField]
        private TMP_Text yourCodeDisplay;

        [SerializeField] private ushort unityTransportPort;

        private void Start() {
            yourCodeDisplay.text = "Your code: " + IpToBase64(GetLocalIPv4());

            hostButton.onClick.AddListener(HostButtonOnClick);
            clientButton.onClick.AddListener(ClientButtonOnClick);
            singleplayerButton.onClick.AddListener(HostButtonOnClick);
        }

        private void HostButtonOnClick() {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            Debug.Log($"[Connect] Host binding on port {transport.ConnectionData.Port}");
            NetworkManager.Singleton.StartHost();
            Debug.Log($"[Connect] IsHost after start: {NetworkManager.Singleton.IsHost}");
        }

        private void ClientButtonOnClick() {
            if (string.IsNullOrWhiteSpace(codeInputField.text)) {
                Debug.LogError("[Connect] Code input field is empty");
                return;
            }

            try {
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                string ip = Base64ToIp(codeInputField.text.Trim());
                transport.SetConnectionData(ip, unityTransportPort);
                Debug.Log($"[Connect] Client dialling {transport.ConnectionData.Address}:{transport.ConnectionData.Port}");

                if (!NetworkManager.Singleton.StartClient()) {
                    Debug.LogError("[Connect] Failed to connect");
                    return;
                }

                inGameUI.SetActive(true);
                multiplayerMenu.SetActive(false);
            } catch (FormatException ex) {
                Debug.LogError($"[Connect] Invalid code format: {ex.Message}");
            } catch (Exception ex) {
                Debug.LogError($"[Connect] Failed to connect: {ex.Message}");
            }
        }

        private string GetLocalIPv4() {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n =>
                    n.OperationalStatus == OperationalStatus.Up
                    && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                )
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(a => a.Address.ToString())
                .FirstOrDefault() ?? "Not found";
        }

        private string IpToBase64(string ip) =>
            Convert.ToBase64String(ip.Split('.').Select(byte.Parse).ToArray()).TrimEnd('=');

        private string Base64ToIp(string base64) =>
            string.Join('.', Convert.FromBase64String(base64.PadRight((base64.Length + 3) / 4 * 4, '=')));
    }
}
