using MultiPlayerSection.GameplayScripts.PlayersInteractions.PlayerReceivers;
using MultiPlayerSection.PlayerScripts;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions
{
    public class PlayerImpactManager : MonoBehaviour
    {
        // El receptor para ataques de otros jugadores (Con Dueño)
        private PlayerWithOwnerReceiver _ownerReceiver;
        
        // El nuevo receptor para peligros del mapa (Sin Dueño)
        private PlayerEnvironmentalReceiver _environmentalReceiver;

        private void Awake()
        {
            _ownerReceiver = GetComponentInChildren<PlayerWithOwnerReceiver>();
            _environmentalReceiver = GetComponentInChildren<PlayerEnvironmentalReceiver>();
        }

        public void ReceiveImpact(HurtboxCharacteristics characteristics, Vector3 forwardEnemigo, Vector3 upEnemigo, GameObject hurtboxGolpeada)
        {
            if (characteristics == null || hurtboxGolpeada == null) return;

            float tiempoAturdimiento = characteristics.Stunning ? characteristics.StunningTime : 0f;

            if (characteristics.Propiedad == HurtboxCharacteristics.PropiedadDaño.ConDueño)
            {
                if (_ownerReceiver != null)
                {
                    // Buscamos el nombre de la gallina víctima desde su raíz
                    PlayerIdentity identidadVictima = hurtboxGolpeada.transform.root.GetComponent<PlayerIdentity>();
                    string nombreVictima = identidadVictima != null ? identidadVictima.NombreIdentificador : "Desconocido";

                    // LLAMAMOS AL RECEIVER DEL ATACANTE (LOCAL / DUEÑO)
                    _ownerReceiver.EnviarImpactoFisicoALaRed(
                        characteristics.Damage,
                        characteristics.Knockback, 
                        characteristics.Inclinacion,
                        characteristics.Direccion,
                        tiempoAturdimiento,
                        forwardEnemigo,
                        upEnemigo,
                        nombreVictima
                    );
                }
            }
            else if (characteristics.Propiedad == HurtboxCharacteristics.PropiedadDaño.SinDueño)
            {
                // Daño proveniente del entorno global (Lava, pinchos, eventos de mapa)
                if (_environmentalReceiver != null)
                {
                    _environmentalReceiver.EnviarImpactoAmbientalALaRed(
                        characteristics.Damage,
                        characteristics.Knockback, 
                        characteristics.Inclinacion,
                        characteristics.Direccion,
                        tiempoAturdimiento,
                        forwardEnemigo,
                        upEnemigo
                    );
                }
            }
        }
    }
}