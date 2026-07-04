using TMPro;
using UnityEngine;

namespace MainMenuSection.MenuScripts {
    public class MainMenuController : MonoBehaviour {
        [Header("Menu Sections")] [SerializeField]
        private GameObject mainSection;

        [SerializeField] private GameObject singleplayerSection;
        [SerializeField] private GameObject multiplayerSection;
        [SerializeField] private GameObject multiplayerOptionsSection;
        [SerializeField] private GameObject multiplayerLobbySection;
        [SerializeField] private GameObject multiplayerJoinLobbySection;
        [SerializeField] private GameObject preferencesSection;
        [SerializeField] private GameObject creditsSection;

        [Header("Multiplayer UI Elements")] [SerializeField]
        private TMP_Text hostCodeText;

        [SerializeField] private TMP_InputField joinCodeInputField;

        [Header("Lobby Status")] [SerializeField]
        private GameObject player2PlaceholderImage;

        [SerializeField] private GameObject player2Image;
        [SerializeField] private GameObject fightButton;
        [SerializeField] private GameObject waitingHostText;

        private void Start() {
            OpenMainMenuSection();
        }

        public void OpenSingleplayerMenu() {
            mainSection.SetActive(false);
            singleplayerSection.SetActive(true);
            multiplayerSection.SetActive(false);
            preferencesSection.SetActive(false);
            creditsSection.SetActive(false);
        }

        public void OpenMultiplayerMenu() {
            mainSection.SetActive(false);
            singleplayerSection.SetActive(false);
            multiplayerSection.SetActive(true);
            preferencesSection.SetActive(false);
            creditsSection.SetActive(false);
            OpenMultiplayerOptionsMenu();
        }

        public void OpenMultiplayerOptionsMenu() {
            if (multiplayerLobbySection.activeInHierarchy || multiplayerJoinLobbySection.activeInHierarchy) {
                GameplayNetworkManager.Instance.CloseConnection();
            }

            multiplayerOptionsSection.SetActive(true);
            multiplayerLobbySection.SetActive(false);
            multiplayerJoinLobbySection.SetActive(false);
        }

        public void OpenMultiplayerLobbyMenu() {
            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.CreateHost();
                string generatedCode = GameplayNetworkManager.GetCurrentHostCode();

                hostCodeText.text = generatedCode;
                player2PlaceholderImage.SetActive(true);
                player2Image.SetActive(false);
                fightButton.SetActive(false);

                GameplayNetworkManager.Instance.OnPlayerJoined += OnPlayerJoined;
            }

            multiplayerOptionsSection.SetActive(false);
            multiplayerLobbySection.SetActive(true);
            multiplayerJoinLobbySection.SetActive(false);
        }

        public void ConfirmJoinLobby() {
            string inputCode = joinCodeInputField.text;

            if (string.IsNullOrWhiteSpace(inputCode)) {
                return;
            }

            GameplayNetworkManager.Instance.OnJoinedLobby -= OnJoinedLobby;
            GameplayNetworkManager.Instance.OnJoinedLobby += OnJoinedLobby;
            GameplayNetworkManager.Instance?.JoinHost(inputCode);
        }

        public void CancelHostAndReturn() {
            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.OnPlayerJoined -= OnPlayerJoined;
                GameplayNetworkManager.Instance.CloseConnection();
            }

            OpenMultiplayerOptionsMenu();
        }

        private void OnPlayerJoined() {
            player2PlaceholderImage.SetActive(false);
            player2Image.SetActive(true);
            fightButton.SetActive(true);
            waitingHostText.SetActive(false);
        }

        private void OnJoinedLobby() {
            OpenMultiplayerLobbyMenu();
            player2PlaceholderImage.SetActive(false);
            player2Image.SetActive(true);
            waitingHostText.SetActive(true);
        }

        public void OpenMultiplayerJoinLobbyMenu() {
            joinCodeInputField.text = "";
            multiplayerOptionsSection.SetActive(false);
            multiplayerLobbySection.SetActive(false);
            multiplayerJoinLobbySection.SetActive(true);
        }

        public void OpenPreferencesSection() {
            mainSection.SetActive(false);
            singleplayerSection.SetActive(false);
            multiplayerSection.SetActive(false);
            preferencesSection.SetActive(true);
            creditsSection.SetActive(false);
        }

        public void OpenMainMenuSection() {
            mainSection.SetActive(true);
            singleplayerSection.SetActive(false);
            multiplayerSection.SetActive(false);
            preferencesSection.SetActive(false);
            creditsSection.SetActive(false);
        }

        public void OpenCreditsSection() {
            mainSection.SetActive(false);
            singleplayerSection.SetActive(false);
            multiplayerSection.SetActive(false);
            preferencesSection.SetActive(false);
            creditsSection.SetActive(true);
        }
    }
}
