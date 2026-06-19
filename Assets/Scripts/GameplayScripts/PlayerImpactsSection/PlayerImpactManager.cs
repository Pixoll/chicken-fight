using GameplayScripts.PlayerImpactsSection.PlayerReceiverSection;
using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection
{
    public class PlayerImpactManager : MonoBehaviour
    {
        private PlayerPunchReceiver _punchReceiver;

        private void Awake()
        {
            _punchReceiver = GetComponentInChildren<PlayerPunchReceiver>();
        }

        public void ReceiveImpact(HurtboxCharacteristics characteristics, Vector3 forwardEnemigo, Vector3 upEnemigo)
        {
            if (characteristics == null) return;

            switch (characteristics.Type)
            {
                case HurtboxCharacteristics.ImpactType.Punch:
                    if (_punchReceiver != null)
                    {
                        float tiempoAturdimiento = characteristics.Stunning ? characteristics.StunningTime : 0f;

                        _punchReceiver.EnviarImpactoFisicoALaRed(
                            characteristics.Knockback, 
                            characteristics.Inclinacion,
                            characteristics.Direccion,
                            tiempoAturdimiento,
                            forwardEnemigo,
                            upEnemigo
                        );
                    }
                    break;

                case HurtboxCharacteristics.ImpactType.Environmental:
                    break;
            }
        }
    }
}
