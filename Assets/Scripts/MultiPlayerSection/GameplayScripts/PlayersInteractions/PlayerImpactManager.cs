using MultiPlayerSection.GameplayScripts.PlayersInteractions.PlayerReceivers;
using MultiPlayerSection.PlayerScripts;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions
{
    public class PlayerImpactManager : MonoBehaviour
    {
        private PlayerWithOwnerReceiver _ownerReceiver;
        private PlayerEnvironmentalReceiver _environmentalReceiver;

        private void Awake()
        {
            _ownerReceiver = GetComponent<PlayerWithOwnerReceiver>();
            if (_ownerReceiver == null) _ownerReceiver = GetComponentInChildren<PlayerWithOwnerReceiver>();
            
            _environmentalReceiver = GetComponent<PlayerEnvironmentalReceiver>();
            if (_environmentalReceiver == null) _environmentalReceiver = GetComponentInChildren<PlayerEnvironmentalReceiver>();
        }

        public void ReceiveImpact(HurtboxCharacteristics characteristics, Vector3 forwardEnemigo, Vector3 upEnemigo, GameObject hurtboxGolpeada)
        {
            if (characteristics == null || hurtboxGolpeada == null) return;

            float tiempoAturdimiento = characteristics.Stunning ? characteristics.StunningTime : 0f;
            
            if (characteristics.Propiedad == HurtboxCharacteristics.PropiedadDaño.ConDueño)
            {
                if (_ownerReceiver != null)
                {
                    PlayerIdentity identidadVictima = GetComponentInParent<PlayerIdentity>();
                    string nombreVictima = "";
                    
                    if (identidadVictima != null && !string.IsNullOrEmpty(identidadVictima.NombreIdentificador))
                        nombreVictima = identidadVictima.NombreIdentificador;
                    else
                    {
                        Unity.Netcode.NetworkObject netObjVictima = GetComponentInParent<Unity.Netcode.NetworkObject>();
                        if (netObjVictima != null) nombreVictima = netObjVictima.OwnerClientId.ToString();
                    }

                    PlayerIdentity identidadAtacante = characteristics.GetComponentInParent<PlayerIdentity>();
                    string nombreAtacante = "";
                    
                    if (identidadAtacante != null && !string.IsNullOrEmpty(identidadAtacante.NombreIdentificador))
                        nombreAtacante = identidadAtacante.NombreIdentificador;
                    else
                    {
                        Unity.Netcode.NetworkObject netObjAtacante = characteristics.GetComponentInParent<Unity.Netcode.NetworkObject>();
                        if (netObjAtacante != null) nombreAtacante = netObjAtacante.OwnerClientId.ToString();
                    }
                    
                    if (string.IsNullOrEmpty(nombreVictima)) nombreVictima = "Desconocido";
                    if (string.IsNullOrEmpty(nombreAtacante)) nombreAtacante = "Desconocido";

                    _ownerReceiver.EnviarImpactoFisicoALaRed(
                        characteristics.Damage,
                        characteristics.Knockback, 
                        characteristics.Inclinacion,
                        characteristics.Direccion,
                        tiempoAturdimiento,
                        forwardEnemigo,
                        upEnemigo,
                        nombreVictima,
                        nombreAtacante,
                        characteristics.Heal, 
                        characteristics.AppliesSlow,
                        characteristics.SlowIntensity,
                        characteristics.SlowDuration 
                    );
                }
            }
            else if (characteristics.Propiedad == HurtboxCharacteristics.PropiedadDaño.SinDueño)
            {
                if (_environmentalReceiver == null) return;

                PlayerIdentity identidadAfectada = GetComponentInParent<PlayerIdentity>();
                string nombreAfectado = "";
                
                if (identidadAfectada != null && !string.IsNullOrEmpty(identidadAfectada.NombreIdentificador))
                    nombreAfectado = identidadAfectada.NombreIdentificador;
                else
                {
                    Unity.Netcode.NetworkObject netObjAfectado = GetComponentInParent<Unity.Netcode.NetworkObject>();
                    if (netObjAfectado != null) nombreAfectado = netObjAfectado.OwnerClientId.ToString();
                }

                if (string.IsNullOrEmpty(nombreAfectado)) nombreAfectado = "Desconocido";

                _environmentalReceiver.EnviarImpactoAmbientalALaRed(
                    characteristics.Damage,
                    characteristics.Knockback, 
                    characteristics.Inclinacion,
                    characteristics.Direccion,
                    tiempoAturdimiento,
                    forwardEnemigo,
                    upEnemigo,
                    nombreAfectado,
                    characteristics.Heal,
                    characteristics.AppliesSlow,
                    characteristics.SlowIntensity,
                    characteristics.SlowDuration 
                );
            }
        }
    }
}
