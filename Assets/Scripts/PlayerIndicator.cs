using Unity.Netcode;
using UnityEngine;

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
