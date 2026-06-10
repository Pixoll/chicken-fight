using MultiplayerScripts;
using UnityEngine;

// Nos aseguramos de importar el namespace donde vive tu manager de red

namespace MenuScripts {
    public class MainMenuController : MonoBehaviour {
        [Header("Menu Sections")] [SerializeField]
        private GameObject mainSection;

        [SerializeField] private GameObject singleplayerSection;
        [SerializeField] private GameObject multiplayerSection;
        [SerializeField] private GameObject multiplayerOptionsSection;
        [SerializeField] private GameObject multiplayerLobbySection;
        [SerializeField] private GameObject multiplayerJoinLobbySection;
        [SerializeField] private GameObject preferencesSection;

        private void Start() {
            OpenMainmenuSection();
        }

        public void OpenSingleplayerMenu() {
            mainSection.SetActive(false);
            singleplayerSection.SetActive(true);
            multiplayerSection.SetActive(false);
            preferencesSection.SetActive(false);
        }

        public void OpenMultiplayerMenu() {
            mainSection.SetActive(false);
            singleplayerSection.SetActive(false);
            multiplayerSection.SetActive(true);
            preferencesSection.SetActive(false);

            OpenMultiplayerOptionsMenu();
        }

        public void OpenMultiplayerOptionsMenu() {
            multiplayerOptionsSection.SetActive(true);
            multiplayerLobbySection.SetActive(false);
            multiplayerJoinLobbySection.SetActive(false);
        }

        public void OpenMultiplayerLobbyMenu() {
            Debug.Log("Abriendo Lobby .....");

            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.CreateHost();
                Debug.Log("Host creado");
            } else {
                Debug.LogError("[MainMenuController] No se encontró el GameplayNetworkManager en la escena.");
            }

            multiplayerOptionsSection.SetActive(false);
            multiplayerLobbySection.SetActive(true);
            multiplayerJoinLobbySection.SetActive(false);
        }

        public void CancelHostAndReturn() {
            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.CloseHost();
                Debug.Log("Host Cerrado");
            }

            OpenMultiplayerOptionsMenu();
        }

        public void OpenMultiplayerJoinLobbyMenu() {
            multiplayerOptionsSection.SetActive(false);
            multiplayerLobbySection.SetActive(false);
            multiplayerJoinLobbySection.SetActive(true);
        }

        public void OpenPreferencesSection() {
            mainSection.SetActive(false);
            singleplayerSection.SetActive(false);
            multiplayerSection.SetActive(false);
            preferencesSection.SetActive(true);
        }

        public void OpenMainmenuSection() {
            mainSection.SetActive(true);
            singleplayerSection.SetActive(false);
            multiplayerSection.SetActive(false);
            preferencesSection.SetActive(false);
        }
    }
}
