using MultiplayerScripts;
using UnityEngine;

namespace SingleplayerScripts {
    public class TrainModeSceneController : MonoBehaviour {
        private void Start() {
            if (GameplayNetworkManager.Instance == null) return;

            GameplayNetworkManager.Instance.CreateHost();
        }
    }
}
