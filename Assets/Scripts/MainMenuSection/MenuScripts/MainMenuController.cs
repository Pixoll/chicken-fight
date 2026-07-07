using System.Collections.Generic;
using MainMenuSection.Core;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace MainMenuSection.MenuScripts {
    public class MainMenuController : MonoBehaviour {
        [Header("Menu Sections")] [SerializeField]
        private GameObject mainSection;

        [SerializeField] private GameObject playSection;
        [SerializeField] private GameObject playOptionsSection;
        [SerializeField] private GameObject playCreateRoomSection;
        [SerializeField] private GameObject playJoinRoomSection;
        [SerializeField] private GameObject howToPlaySection;
        [SerializeField] private GameObject preferencesSection;
        [SerializeField] private GameObject statsSection;
        [SerializeField] private GameObject creditsSection;
        [SerializeField] private GameObject quitConfirmationSection;

        [Header("Multiplayer UI Elements")] [SerializeField]
        private TMP_Text hostCodeText;

        [SerializeField] private TMP_InputField joinCodeInputField;

        [Header("Room Status")] [SerializeField]
        private GameObject player2PlaceholderImage;

        [SerializeField] private GameObject player2Image;
        [SerializeField] private GameObject fightButton;
        [SerializeField] private GameObject waitingHostText;

        private List<GameObject> _primarySections;
        private List<GameObject> _secondaryPlaySections;

        private void Awake() {
            _primarySections = new List<GameObject> {
                mainSection,
                playSection,
                howToPlaySection,
                preferencesSection,
                statsSection,
                creditsSection,
            };

            _secondaryPlaySections = new List<GameObject> {
                playOptionsSection,
                playCreateRoomSection,
                playJoinRoomSection,
            };
        }

        private void Start() {
            OpenMainMenuSection();
        }

        public void OpenMainMenuSection() {
            ActivatePrimarySection(mainSection);
        }

        public void OpenPlayMenu() {
            ActivatePrimarySection(playSection);
            OpenPlayOptionsMenu();
        }

        public void OpenHowToPlaySection() {
            ActivatePrimarySection(howToPlaySection);
        }

        public void OpenPreferencesSection() {
            ActivatePrimarySection(preferencesSection);
        }

        public void OpenStatsSection() {
            ActivatePrimarySection(statsSection);
        }

        public void OpenCreditsSection() {
            ActivatePrimarySection(creditsSection);
        }

        public void OpenQuitConfirmationSection() {
            quitConfirmationSection.SetActive(true);
        }

        public void CloseQuitConfirmationSection() {
            quitConfirmationSection.SetActive(false);
        }

        public void OpenPlayOptionsMenu() {
            if (playCreateRoomSection.activeInHierarchy) {
                GameplayNetworkManager.Instance.OnPlayerJoined -= OnPlayerJoined;
                GameplayNetworkManager.Instance.OnPlayerLeft -= OnPlayerLeft;
                GameplayNetworkManager.Instance.CloseConnection();
            }

            ActivateSecondaryPlaySection(playOptionsSection);
        }

        public void OpenPlayCreateRoomMenu() {
            if (GameplayNetworkManager.Instance != null) {
                GameplayNetworkManager.Instance.CreateHost();
                string generatedCode = GameplayNetworkManager.GetCurrentHostCode();

                hostCodeText.text = generatedCode;
                player2PlaceholderImage.SetActive(true);
                player2Image.SetActive(false);
                fightButton.SetActive(false);

                GameplayNetworkManager.Instance.OnPlayerJoined += OnPlayerJoined;
                GameplayNetworkManager.Instance.OnPlayerLeft += OnPlayerLeft;
            }

            ActivateSecondaryPlaySection(playCreateRoomSection);
            player2PlaceholderImage.SetActive(true);
            player2Image.SetActive(false);
            fightButton.SetActive(false);
            waitingHostText.SetActive(false);
        }

        public void OpenPlayJoinRoomMenu() {
            joinCodeInputField.text = "";
            ActivateSecondaryPlaySection(playJoinRoomSection);
        }

        public void ConfirmJoinRoom() {
            string inputCode = joinCodeInputField.text;

            if (string.IsNullOrWhiteSpace(inputCode)) {
                return;
            }

            GameplayNetworkManager.Instance.OnJoinedRoom -= OnJoinedRoom;
            GameplayNetworkManager.Instance.OnJoinedRoom += OnJoinedRoom;
            GameplayNetworkManager.Instance.OnHostLeft += OnHostLeft;
            GameplayNetworkManager.Instance?.JoinHost(inputCode);
        }

        private void OnPlayerJoined() {
            if (player2PlaceholderImage.IsDestroyed()) {
                GameplayNetworkManager.Instance.OnPlayerJoined -= OnPlayerJoined;
                GameplayNetworkManager.Instance.OnPlayerLeft -= OnPlayerLeft;
                return;
            }

            player2PlaceholderImage.SetActive(false);
            player2Image.SetActive(true);
            fightButton.SetActive(true);
            waitingHostText.SetActive(false);
        }

        private void OnPlayerLeft() {
            if (player2PlaceholderImage.IsDestroyed()) {
                GameplayNetworkManager.Instance.OnPlayerJoined -= OnPlayerJoined;
                GameplayNetworkManager.Instance.OnPlayerLeft -= OnPlayerLeft;
                return;
            }

            player2PlaceholderImage.SetActive(true);
            player2Image.SetActive(false);
            fightButton.SetActive(false);
            waitingHostText.SetActive(false);
        }

        private void OnJoinedRoom() {
            OpenPlayCreateRoomMenu();
            player2PlaceholderImage.SetActive(false);
            player2Image.SetActive(true);
            waitingHostText.SetActive(true);
        }

        private void OnHostLeft() {
            GameplayNetworkManager.Instance.OnHostLeft -= OnHostLeft;
            OpenPlayJoinRoomMenu();
        }

        private void ActivatePrimarySection(GameObject section) {
            foreach (GameObject primary in _primarySections) {
                primary.SetActive(primary == section);
            }
        }

        private void ActivateSecondaryPlaySection(GameObject section) {
            foreach (GameObject secondary in _secondaryPlaySections) {
                secondary.SetActive(secondary == section);
            }
        }
    }
}
