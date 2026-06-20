using MultiplayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuScripts {
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
            Debug.Log("[UI - MainMenuController] Botón 'Crear Sala' pulsado. Preparando paneles...");

            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.CreateHost();

                string generatedCode = GameplayNetworkManager.GetCurrentHostCode();
                if (hostCodeText != null) {
                    hostCodeText.text = $"CÓDIGO DE SALA: {generatedCode}";
                }

                if (lobbyStatusText != null) {
                    lobbyStatusText.text = "ESPERANDO QUE UN RIVAL SE UNA...";
                }

                // Suscripción al evento
                GameplayNetworkManager.Instance.OnPlayerJoined += ActualizarTextoLobbyClienteConectado;
                Debug.Log("[UI - MainMenuController] Script de UI suscrito con éxito al evento OnPlayerJoined del Manager.");
            } else {
                Debug.LogError("[UI - MainMenuController] Error fatal: No se encontró el GameplayNetworkManager en la escena al abrir el Lobby.");
            }

            multiplayerOptionsSection.SetActive(false);
            multiplayerLobbySection.SetActive(true);
            multiplayerJoinLobbySection.SetActive(false);
        }

        public void ConfirmJoinLobby() {
            Debug.Log("[UI - MainMenuController] Botón 'Confirmar Conexión' / 'Unirse' pulsado físicamente.");

            if (joinCodeInputField == null) {
                Debug.LogError("[UI - MainMenuController] Error: La variable joinCodeInputField está vacía en el Inspector.");
                return;
            }

            string inputCode = joinCodeInputField.text;
            Debug.Log($"[UI - MainMenuController] Texto rescatado del InputField: '{inputCode}'");

            if (string.IsNullOrWhiteSpace(inputCode)) {
                Debug.LogWarning("[UI - MainMenuController] Conexión cancelada en UI: El InputField está vacío o contiene puros espacios.");
                return;
            }

            if (GameplayNetworkManager.Instance != null) {
                Debug.Log("[UI - MainMenuController] Enviando código al GameplayNetworkManager...");
                GameplayNetworkManager.Instance.JoinHost(inputCode);
            } else {
                Debug.LogError("[UI - MainMenuController] Error: GameplayNetworkManager.Instance es NULL al intentar unirse.");
            }
        }

        public void CancelHostAndReturn() {
            Debug.Log("[UI - MainMenuController] Cancelando Host/Lobby. Limpiando suscripciones...");
            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.OnPlayerJoined -= ActualizarTextoLobbyClienteConectado;
                GameplayNetworkManager.Instance.CloseConnection();
            }
            OpenMultiplayerOptionsMenu();
        }
        
        private void ActualizarTextoLobbyClienteConectado(ulong clientId) {
            Debug.Log($"<color=green>[UI - MainMenuController] ¡El evento OnPlayerJoined llegó a la UI! ClientID recibido: {clientId}</color>");
            if (lobbyStatusText != null) {
                lobbyStatusText.text = $"<color=green>¡UN JUGADOR SE HA UNIDO! (ID: {clientId})</color>\nIniciando partida...";
            } else {
                Debug.LogWarning("[UI - MainMenuController] El texto 'lobbyStatusText' es NULL, no se puede actualizar el mensaje en pantalla.");
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