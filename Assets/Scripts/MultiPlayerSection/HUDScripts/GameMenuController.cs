using MainMenuSection;
using TMPro;
using UnityEngine;

namespace MultiPlayerSection.HUDScripts {
    public class GameMenuController : MonoBehaviour {
        [Header("UI Sections")] [SerializeField]
        private GameObject pauseSection;

        [SerializeField] private GameObject exitConfirmationSection;
        [SerializeField] private GameObject gameHudUiCanvas;
        [SerializeField] private GameObject pauseUiCanvas;
        [SerializeField] private GameObject overviewUiCanvas;

        [Header("UI Elements")] [SerializeField]
        private TMP_Text healthBarUsernameText;

        [SerializeField] private TMP_Text player1NameText;
        [SerializeField] private TMP_Text player2NameText;

        private void Start() {
            healthBarUsernameText.text = GameplayNetworkManager.Instance.CurrentPlayerUsername;
            player1NameText.text = GameplayNetworkManager.Instance.player1Username;
            player2NameText.text = GameplayNetworkManager.Instance.player2Username;
        }

        public void OpenPauseMenu() {
            pauseSection.SetActive(true);
            exitConfirmationSection.SetActive(false);
        }

        public void ClosePauseMenu() {
            pauseSection.SetActive(false);
            exitConfirmationSection.SetActive(false);
        }

        public void OpenExitConfirmation() {
            exitConfirmationSection.SetActive(true);
        }

        public void CloseExitConfirmation() {
            exitConfirmationSection.SetActive(false);
        }

        public void DisplayOverview() {
            gameHudUiCanvas.SetActive(false);
            pauseUiCanvas.SetActive(false);
            overviewUiCanvas.SetActive(true);
        }
    }
}
