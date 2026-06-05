using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts
{
    public class ChildHitbox : NetworkBehaviour {
        private HitReceiver _parentReceiver;

        private void Awake() {
            _parentReceiver = GetComponentInParent<HitReceiver>();
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (_parentReceiver == null || collision.gameObject.name != "AttackHitbox") return;

            // Buscamos el NetworkObject del atacante en sus padres para hallar la posición real del personaje entero
            NetworkObject attackerNetworkObject = collision.GetComponentInParent<NetworkObject>();
        
            Vector2 attackerPosition = attackerNetworkObject != null 
                ? (Vector2)attackerNetworkObject.transform.position 
                : (Vector2)collision.transform.position;

            _parentReceiver.ReceiveHit(attackerPosition);
        }
    }
}
