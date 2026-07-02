using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameplayScripts {
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

            // Guardamos cuál era la gallina vieja si es que existía una
            string gallinaAnterior = _miGallinaLocalInput != null ? _miGallinaLocalInput.gameObject.name : "Ninguna";
            
            _miGallinaLocalInput = input;
            
            // Verificamos si tiene el joystick asignado
            if (fixedJoystick != null) {
                _miGallinaLocalInput.ConfigurarJoystickLocal(fixedJoystick);
            }

            Debug.Log($"<color=green><b>[UI LINK EXITOSO]</b> La interfaz del Canvas capturó a su dueño legítimo.</color>\n" +
                      $"▶ Objeto enlazado: <b>{_miGallinaLocalInput.gameObject.name}</b>\n" +
                      $"▶ Ubicación del Script: {input.transform.parent.name} -> {input.gameObject.name}\n" +
                      $"▶ Reemplazó a la gallina: {gallinaAnterior}");
        }

        public void InactiveFightMenuSection() { fightMenuSection.SetActive(false); }
        public void ActiveFightMenuSection() { fightMenuSection.SetActive(true); }

        public void OnJumpButtonClicked() {
            if (_miGallinaLocalInput != null) {
                _miGallinaLocalInput.TriggerUIJump();
            } else {
                Debug.LogWarning("<color=orange>[UI CLICK] Click en SALTO ignorado: No hay ninguna gallina vinculada a esta UI local todavía.</color>");
            }
        }

        public void OnPunhButtonClicked() {
            if (_miGallinaLocalInput != null) {
                _miGallinaLocalInput.TriggerUIPunch();
            } else {
                Debug.LogWarning("<color=orange>[UI CLICK] Click en GOLPE ignorado: No hay ninguna gallina vinculada a esta UI local todavía.</color>");
            }
        }
    }
}
