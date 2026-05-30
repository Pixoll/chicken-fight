using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class ConnectUI : MonoBehaviour {
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_Text localIPDisplay;
    [SerializeField] private ushort unityTransportPort;

    private void Start() {
        localIPDisplay.text = "Your IP: " + GetLocalIPv4();

        hostButton.onClick.AddListener(HostButtonOnClick);
        clientButton.onClick.AddListener(ClientButtonOnClick);
    }

    private void HostButtonOnClick() {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        Debug.Log($"[Connect] Host binding on port {transport.ConnectionData.Port}");
        NetworkManager.Singleton.StartHost();
        Debug.Log($"[Connect] IsHost after start: {NetworkManager.Singleton.IsHost}");
    }

    private void ClientButtonOnClick() {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipInputField.text.Trim(), unityTransportPort);
        Debug.Log($"[Connect] Client dialling {transport.ConnectionData.Address}:{transport.ConnectionData.Port}");
        NetworkManager.Singleton.StartClient();
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
}
