using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.GlobalGameState
{
    public class GameUIEventSection : NetworkBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject panelCartelGlobal;

        private void Awake()
        {
            if (panelCartelGlobal != null) panelCartelGlobal.SetActive(false);
        }
        
        public void EnviarCartelGlobal(string mensaje, float duracion)
        {
            MostrarCartelEnTodasLasPantallasRpc(mensaje, duracion);
        }

        [Rpc(SendTo.Everyone)]
        private void MostrarCartelEnTodasLasPantallasRpc(string mensaje, float duracion)
        {
            if (panelCartelGlobal == null) return;

            panelCartelGlobal.SetActive(true);

            CancelInvoke(nameof(OcultarCartel));
            Invoke(nameof(OcultarCartel), duracion);
        }

        private void OcultarCartel()
        {
            if (panelCartelGlobal != null) panelCartelGlobal.SetActive(false);
        }
    }
}
