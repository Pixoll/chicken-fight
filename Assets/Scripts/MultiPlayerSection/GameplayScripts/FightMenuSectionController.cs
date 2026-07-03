using MultiPlayerSection.PlayerScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiPlayerSection.GameplayScripts {
    public class FightMenuSectionController : MonoBehaviour {
        [FormerlySerializedAs("mainSection")] 
        [Header("Menu Sections")] 
        [SerializeField] private GameObject fightMenuSection;
        
        [Header("Componentes de Entrada de la UI")]
        [SerializeField] private Joystick fixedJoystick;

        private PlayerInputHandler _miGallinaLocalInput;

        private void Start() {
            ActiveFightMenuSection();
        }

        public void VincularGallinaLocal(PlayerInputHandler input) {
            if (input == null) {
                return;
            }

            string gallinaAnterior = _miGallinaLocalInput != null ? _miGallinaLocalInput.gameObject.name : "Ninguna";
            
            _miGallinaLocalInput = input;
            
            if (fixedJoystick != null) {
                _miGallinaLocalInput.ConfigurarJoystickLocal(fixedJoystick);
            }
        }

        public void InactiveFightMenuSection() { fightMenuSection.SetActive(false); }
        public void ActiveFightMenuSection() { fightMenuSection.SetActive(true); }

        public void OnJumpButtonClicked() {
            if (_miGallinaLocalInput != null) {
                _miGallinaLocalInput.TriggerUIJump();
            }
        }

        public void OnPunhButtonClicked() {
            if (_miGallinaLocalInput != null) {
                _miGallinaLocalInput.TriggerUIPunch();
            }
        }
    }
}
