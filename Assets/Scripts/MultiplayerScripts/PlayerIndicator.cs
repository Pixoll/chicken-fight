using Unity.Netcode;
using UnityEngine;

namespace MultiplayerScripts
{
    public class PlayerIndicator : NetworkBehaviour {
        [SerializeField] private GameObject indicator;

        private void Awake() {
            indicator?.SetActive(false);
        }

        private void Start() {
            if (IsOwner) {
                indicator.SetActive(true);
            }
        }
    }
}
