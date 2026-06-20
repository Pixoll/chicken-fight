using MultiplayerScripts;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SingleplayerScripts {
    public class MenuSectionControllers : MonoBehaviour {
        public void LaunchMainMenu() {
            SceneManager.LoadScene("Scenes/MainMenu");
        }

        public void OnExitButtonClicked() {
            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.CloseConnection();

                Debug.Log("<color=red>[UI] Botón Salir presionado. Servidor cerrado.</color>");
            }

            LaunchMainMenu();
        }
    }
}
