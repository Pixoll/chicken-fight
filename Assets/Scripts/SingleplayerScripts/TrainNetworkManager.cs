using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class TrainNetworkManager : MonoBehaviour {
        
    [Header("Configuración Local del Puerto")]
    [SerializeField] private ushort puertoPrueba = 7777;

    private UnityTransport _transport;

    private void Start() {
        if (NetworkManager.Singleton == null) {
            Debug.LogError("<color=red>[TrainNetworkManager] ¡CRÍTICO: No pusiste el objeto NetworkManager en la jerarquía de esta escena!</color>");
            return;
        }
        _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            
        if (_transport != null) {
            _transport.ConnectionData.Address = "127.0.0.1";
            _transport.ConnectionData.Port = puertoPrueba;
        } else {
            Debug.LogWarning("[TrainNetworkManager] No se encontró UnityTransport. Asegúrate de haberlo arrastrado al NetworkManager.");
        }

        IniciarHostAutomatico();
    }

    private void IniciarHostAutomatico() {
        Debug.Log("<color=orange>[TrainMode] Inicializando simulación local de red...</color>");

        NetworkManager.Singleton.OnClientConnectedCallback += AlConectarCliente;

        bool exito = NetworkManager.Singleton.StartHost();

        if (exito) {
            Debug.Log("<color=green>[🚀 TRAIN MODE ACTIVO] Servidor local encendido. Ya puedes testear tus NetworkBehaviour de forma segura y sin lag.</color>");
        } else {
            Debug.LogError("[TrainMode] Falló el arranque automático del Host. Revisa si hay otra instancia corriendo.");
        }
    }

    private void AlConectarCliente(ulong clientId) {
        if (clientId == NetworkManager.Singleton.LocalClientId) {
            Debug.Log($"<color=cyan>[TrainMode] Tu propia pantalla se ha conectado exitosamente como Jugador Local (ID: {clientId}).</color>");
        }
    }

    private void OnDestroy() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.OnClientConnectedCallback -= AlConectarCliente;
        }
    }
}