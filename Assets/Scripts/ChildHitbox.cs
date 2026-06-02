using Unity.Netcode;
using UnityEngine;

public class ChildHitbox : NetworkBehaviour {
    private HitReceiver _parentReceiver;

    private void Awake() {
        _parentReceiver = GetComponentInParent<HitReceiver>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (_parentReceiver == null || collision.gameObject.name != "AttackHitbox") return;

        Vector2 attackerCenter = collision.transform.parent.position;
        _parentReceiver.ReceiveHit(attackerCenter);
    }
}
