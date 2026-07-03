using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenuSection.MenuScripts {
    public class FightButton : MonoBehaviour {
        public void Fight() {
            NetworkManager.Singleton.SceneManager.LoadScene("RoundMode", LoadSceneMode.Single);
        }
    }
}
