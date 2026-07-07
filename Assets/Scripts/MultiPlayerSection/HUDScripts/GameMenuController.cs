using MainMenuSection.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiPlayerSection.HUDScripts {
    public class GameMenuController : MonoBehaviour {
        [SerializeField] private GameObject pauseSection;
        [SerializeField] private GameObject exitConfirmationSection;
        [SerializeField] private GameObject gameHudUiCanvas;
        [SerializeField] private GameObject pauseUiCanvas;
        [SerializeField] private GameObject overviewUiCanvas;
        [SerializeField] private GameObject playerLeftWarning;

        private void Start() {
            GameplayNetworkManager.Instance.OnPlayerLeft += DisplayOtherPlayerLeftWarning;
            GameplayNetworkManager.Instance.OnHostLeft += DisplayOtherPlayerLeftWarning;
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
            GameplayNetworkManager.Instance.OnPlayerLeft -= DisplayOtherPlayerLeftWarning;
            GameplayNetworkManager.Instance.OnHostLeft -= DisplayOtherPlayerLeftWarning;
            gameHudUiCanvas.SetActive(false);
            pauseUiCanvas.SetActive(false);
            overviewUiCanvas.SetActive(true);
        }

        public void ExitMatch() {
            GameplayNetworkManager.Instance.ClearEventListeners();
            GameplayNetworkManager.Instance.CloseConnection();
            SceneManager.LoadScene("MainMenu");
        }

        private void DisplayOtherPlayerLeftWarning() {
            playerLeftWarning.SetActive(true);
        }
    }
}
