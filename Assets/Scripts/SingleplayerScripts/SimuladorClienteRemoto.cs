using GameplayScripts;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Script exclusivo de laboratorio. Se le pone al Clon en la escena para 
/// forzarlo a actuar como un cliente externo sin autoridad desde el frame 1.
/// </summary>
[DefaultExecutionOrder(-10)] // Obliga a este script a ejecutarse antes que los de movimiento
public class SimuladorClienteRemoto : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Si estamos iniciando el juego como Host/Servidor
        if (IsServer)
        {
            Debug.Log("<color=magenta>[LABORATORIO] ✂️ Desvinculando clon... Quitándole la propiedad de dueño al Host.</color>");
                
            // Le quitamos la autoridad al Host local sobre este objeto específico
            NetworkObject.RemoveOwnership();
        }

        // Invocamos el apagado en el siguiente frame para asegurar que Netcode ya procesó el cambio de dueño
        Invoke(nameof(ForzarAislamientoDeInput), 0.01f);
    }

    private void ForzarAislamientoDeInput()
    {
        // Si el truco funcionó, IsOwner ahora será FALSO para este clon en tu pantalla
        if (!IsOwner)
        {
            Debug.Log("<color=cyan>[LABORATORIO] 🛑 Clon configurado con éxito como Cliente Remoto Sin Autoridad.</color>");

            // Buscamos el PlayerInputHandler en sus hijos y lo destruimos por completo
            // Así el clon queda sordo al joystick y a los botones, evitando que se mueva contigo
            PlayerInputHandler input = GetComponentInChildren<PlayerInputHandler>();
            if (input != null)
            {
                input.enabled = false;
                Destroy(input); 
            }
        }
    }
}