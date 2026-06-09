using UnityEngine;
using UnityEngine.Serialization;
using Unity.Netcode;

namespace GameplayScripts
{
    public class FightMenuSectionController : MonoBehaviour 
    {
        [FormerlySerializedAs("mainSection")]
        [Header("Menu Sections")]
        [SerializeField] private GameObject fightMenuSection;
    
        private void Start()
        {
            ActiveFightMenuSection();
        }
    
        public void InactiveFightMenuSection() {
            fightMenuSection.SetActive(false);
        }
    
        public void ActiveFightMenuSection() {
            fightMenuSection.SetActive(true);
        }

        public void OnJumpButtonClicked()
        {
            PlayerInputHandler[] allInputs = FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);

            foreach (PlayerInputHandler input in allInputs)
            {
                NetworkObject networkObject = input.transform.root.GetComponent<NetworkObject>();

                if (networkObject != null && networkObject.IsOwner)
                {
                    input.TriggerUIJump();
                    return;
                }
            }
        }

        public void OnPunhButtonClicked()
        {
            PlayerInputHandler[] allInputs = FindObjectsByType<PlayerInputHandler>(FindObjectsSortMode.None);

            foreach (PlayerInputHandler input in allInputs)
            {
                NetworkObject networkObject = input.transform.root.GetComponent<NetworkObject>();

                if (networkObject != null && networkObject.IsOwner)
                {
                    input.TriggerUIPunch();
                    return;
                }
            }
        }
    }
}