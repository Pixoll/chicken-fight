using Unity.Netcode;
using UnityEngine;

namespace MultiplayerScripts
{
    public class DisconnectGame : MonoBehaviour {
        public void Disconnect() {
            if (NetworkManager.Singleton != null) {
                NetworkManager.Singleton.Shutdown();
            }
        }
    }
}
