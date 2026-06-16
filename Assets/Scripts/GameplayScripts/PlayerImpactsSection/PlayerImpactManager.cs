using GameplayScripts.PlayerImpactsSection.PlayerReceiverSection;
using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection
{
    // Distribuidor local en la pantalla del atacante
    public class PlayerImpactManager : MonoBehaviour
    {
        private PlayerPunchReceiver _punchReceiver;

        private void Awake()
        {
            _punchReceiver = GetComponentInChildren<PlayerPunchReceiver>();
        }

        /// <summary>
        /// El detector de choques (Hitbox) llama a este método localmente.
        /// </summary>
        public void ReceiveImpact(HurtboxCharacteristics characteristics)
        {
            if (characteristics == null) return;

            switch (characteristics.Type)
            {
                case HurtboxCharacteristics.ImpactType.Punch:
                    
                    if (_punchReceiver != null)
                    {
                        // Si la Hurtbox tiene activado el aturdimiento, mandamos su tiempo. Si no, mandamos 0.
                        float tiempoAturdimiento = characteristics.Stunning ? characteristics.StunningTime : 0f;

                        // Le pasamos la pelota al receptor físico incluyendo el tiempo de Stun de la Hurtbox
                        _punchReceiver.EnviarImpactoFisicoALaRed(
                            characteristics.Knockback, 
                            characteristics.Direction, 
                            tiempoAturdimiento
                        );
                    }
                    break;

                case HurtboxCharacteristics.ImpactType.Environmental:
                    // Aquí irían las lógicas de trampas en el futuro
                    break;
            }
        }
    }
}