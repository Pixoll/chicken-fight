using TMPro;
using UnityEngine;

namespace MainMenuSection.MenuScripts {
    public class MainMenuController : MonoBehaviour {
        [Header("Menu Sections")] 
        [SerializeField] private GameObject mainSection;
        [SerializeField] private GameObject singleplayerSection;
        [SerializeField] private GameObject multiplayerSection;
        [SerializeField] private GameObject multiplayerOptionsSection;
        [SerializeField] private GameObject multiplayerLobbySection;
        [SerializeField] private GameObject multiplayerJoinLobbySection;
        [SerializeField] private GameObject preferencesSection;

        [Header("Multiplayer UI Elements")]
        [SerializeField] private TMP_Text hostCodeText;
        [SerializeField] private TMP_InputField joinCodeInputField; 

        [Header("Lobby Status")]
        [SerializeField] private TMP_Text lobbyStatusText;

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
                if (hostCodeText != null) {
                    hostCodeText.text = $"CÓDIGO DE SALA: {generatedCode}";
                }

                if (lobbyStatusText != null) {
                    lobbyStatusText.text = "ESPERANDO QUE UN RIVAL SE UNA...";
                }

                GameplayNetworkManager.Instance.OnPlayerJoined += ActualizarTextoLobbyClienteConectado;
            }

            multiplayerOptionsSection.SetActive(false);
            multiplayerLobbySection.SetActive(true);
            multiplayerJoinLobbySection.SetActive(false);
        }

        public void ConfirmJoinLobby() {
            if (joinCodeInputField == null) {
                return;
            }

            string inputCode = joinCodeInputField.text;

            if (string.IsNullOrWhiteSpace(inputCode)) {
                return;
            }

            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.JoinHost(inputCode);
            }
        }

        public void CancelHostAndReturn() {
            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.OnPlayerJoined -= ActualizarTextoLobbyClienteConectado;
                GameplayNetworkManager.Instance.CloseConnection();
            }
            OpenMultiplayerOptionsMenu();
        }
        
        private void ActualizarTextoLobbyClienteConectado(ulong clientId) {
            if (lobbyStatusText != null) {
                lobbyStatusText.text = $"<color=green>¡UN JUGADOR SE HA UNIDO! (ID: {clientId})</color>\nIniciando partida...";
            }
        }

        public void OpenMultiplayerJoinLobbyMenu() {
            if (joinCodeInputField != null) {
                joinCodeInputField.text = "";
            }
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
