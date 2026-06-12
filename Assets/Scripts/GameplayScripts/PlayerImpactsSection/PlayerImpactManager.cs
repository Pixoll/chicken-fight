using GameplayScripts.PlayerImpactsSection.PlayerReceiverSection;
using Unity.Netcode;
using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection
{
    public class PlayerImpactManager : NetworkBehaviour
    {
        private PlayerPunchReceiver _punchReceiver;
        

        private void Awake()
        {
            _punchReceiver = GetComponentInChildren<PlayerPunchReceiver>();
        }
        
        public void ReceiveImpact(HurtboxCharacteristics characteristics)
        {

            switch (characteristics.Type)
            {
                case HurtboxCharacteristics.ImpactType.Punch:
                    if (_punchReceiver != null)
                    {
                        Vector2 originPosition = characteristics.transform.position;

                        if (characteristics.Direction == HurtboxCharacteristics.KnockbackDirection.Top)
                        {
                            _punchReceiver.ApplyPunchKnockback(
                                originPosition, 
                                characteristics.Knockback, 
                                PlayerPunchReceiver.PunchInclination.Top
                            );
                        }
                        if (characteristics.Direction is HurtboxCharacteristics.KnockbackDirection.TopRight 
                            or HurtboxCharacteristics.KnockbackDirection.TopLeft)
                        {
                            _punchReceiver.ApplyPunchKnockback(
                                originPosition, 
                                characteristics.Knockback, 
                                PlayerPunchReceiver.PunchInclination.Mid
                            );
                        }
                        
                        if (characteristics.Direction is HurtboxCharacteristics.KnockbackDirection.Left 
                        or  HurtboxCharacteristics.KnockbackDirection.Right)
                        {
                            _punchReceiver.ApplyPunchKnockback(
                                originPosition, 
                                characteristics.Knockback, 
                                PlayerPunchReceiver.PunchInclination.Bottom
                            );
                        }
                    }
                    break;

                case HurtboxCharacteristics.ImpactType.Environmental:
                    Debug.Log($"<color=yellow>[ImpactManager] Aplicando efectos de entorno. Daño: {characteristics.Damage}</color>");
                    break;

                case HurtboxCharacteristics.ImpactType.StatusEffect:
                    Debug.Log($"<color=cyan>[ImpactManager] Efecto de estado detectado. Ignorado temporalmente.</color>");
                    break;

                case HurtboxCharacteristics.ImpactType.SpecialAttack:
                    Debug.Log($"<color=orange>[ImpactManager] Ataque especial detectado. Ignorado temporalmente.</color>");
                    break;
            }
        }
    }
}
