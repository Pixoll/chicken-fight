using MainMenuSection.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenuSection.MenuScripts {
    public class FightButton : MonoBehaviour {
        public void Fight() {
            GameplayNetworkManager.Instance.ClearEventListeners();
            NetworkManager.Singleton.SceneManager.LoadScene("RoundMode", LoadSceneMode.Single);
        }
    }
}
