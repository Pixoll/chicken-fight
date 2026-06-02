using Unity.Netcode;
using UnityEngine;

public class DisconnectGame : MonoBehaviour {
    public void Disconnect() {
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
