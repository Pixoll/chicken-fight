using Unity.Netcode.Components;
using UnityEngine;

namespace MultiplayerScripts
{
    /// <summary>
    /// Modifica el NetworkAnimator nativo para que el dueño del personaje 
    /// tenga el poder de sincronizar las animaciones directamente por la red.
    /// </summary>
    [DisallowMultipleComponent]
    public class OwnerNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            // Al retornar false, le quitamos la autoridad al servidor y se la damos al cliente (Owner)
            return false;
        }
    }
}