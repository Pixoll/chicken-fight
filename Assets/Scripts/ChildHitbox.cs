using UnityEngine;

public class ChildHitbox : MonoBehaviour {
    private HitReceiver _parentReceiver;

    private void Awake() {
        _parentReceiver = GetComponentInParent<HitReceiver>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.name == "AttackHitbox") {
            if (_parentReceiver != null) {
                Vector2 attackerCenter = collision.transform.parent.position;
                _parentReceiver.ReceiveHit(attackerCenter);
            }
        }
    }
}
