using Unity.Netcode;
using UnityEngine;

namespace MultiplayerScripts.GlobalGameState
{
    [RequireComponent(typeof(GameTimeSection))]
    public class GlobalGameStateManager : NetworkBehaviour
    {
        public static GlobalGameStateManager Instance { get; private set; }

        [HideInInspector] public GameTimeSection TimeSection;
        [HideInInspector] public GameUIEventSection UIEventSection;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            TimeSection = GetComponentInChildren<GameTimeSection>();
            UIEventSection = GetComponentInChildren<GameUIEventSection>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                // 1. Esperamos 1 segundo de cortesía para que la red se estabilice
                Invoke(nameof(DispararArranqueDePartida), 1f);
            }
        }

        private void DispararArranqueDePartida()
        {
            Debug.Log("<color=magenta>[MANAGER] Servidor listo. Invocando la sección de tiempo ahora.</color>");
            
            if (TimeSection != null)
            {
                TimeSection.IniciarCronometroMaestro();
            }
        }
        
        private void SimularInicioDePelea()
        {
            if (UIEventSection != null) UIEventSection.EnviarCartelGlobal("¡ROUND 1!", 2f);
        }
    }
}
