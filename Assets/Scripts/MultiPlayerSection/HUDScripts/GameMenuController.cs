using UnityEngine;

namespace MultiPlayerSection.HUDScripts {
    public class GameMenuController : MonoBehaviour {
        [SerializeField] private GameObject pauseSection;
        [SerializeField] private GameObject exitConfirmationSection;
        [SerializeField] private GameObject gameHudUiCanvas;
        [SerializeField] private GameObject pauseUiCanvas;
        [SerializeField] private GameObject overviewUiCanvas;

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
