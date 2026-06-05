using UnityEngine;

namespace MenuScripts
{
    public class MainMenuController : MonoBehaviour {
        [Header("Menu Sections")]
        [SerializeField] private GameObject mainSection;
        [SerializeField] private GameObject singleplayerSection;
        [SerializeField] private GameObject multiplayerSection;
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