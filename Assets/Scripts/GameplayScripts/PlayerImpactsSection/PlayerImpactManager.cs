using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection {
    public class PlayerImpactManager : NetworkBehaviour {
        private PlayerPunchReceiver _punchReceiver;

        private void Awake() {
            _punchReceiver = GetComponent<PlayerPunchReceiver>();
        }

        public void ReceiveImpact(HurtboxCharacteristics characteristics) {
            switch (characteristics.Type) {
                case HurtboxCharacteristics.ImpactType.Punch:
                    if (_punchReceiver != null) {
                        _punchReceiver.ApplyPunchKnockback(characteristics.GetOriginPosition(),
                            characteristics.Knockback);
                    }

                    break;

                case HurtboxCharacteristics.ImpactType.Environmental:
                    Debug.Log(
                        $"<color=yellow>[ImpactManager] Daño ambiental recibido: {characteristics.Damage}</color>");

                    // A futuro: _health.TakeDamage(characteristics.Damage);
                    break;
            }
        }
    }
}
