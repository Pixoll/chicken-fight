using System.Collections;
using System.Collections.Generic;
using MultiPlayerSection.CoreState;
using MultiPlayerSection.Efects;
using MultiPlayerSection.GameplayScripts.Objects; // Acceso a FallingObject
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.NetworkScripts
{
    public class GlobalGameStateManager : NetworkBehaviour
    {
        public static GlobalGameStateManager Instance { get; private set; }

        [Header("Referencias Seccionales")]
        [SerializeField] private GameTimeSection timeSection; 
        [SerializeField] private GameUIEventSection uiEventSection;

        [Header("Gestión de Escenario")]
        [Tooltip("Arrastra aquí todos los GameObjects fijos (Cajas, Lava, Trampas) colocados en la escena")]
        [SerializeField] private List<GameObject> objetosFijosEnEscenario = new List<GameObject>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (uiEventSection == null) uiEventSection = GetComponentInChildren<GameUIEventSection>();
            if (timeSection == null) timeSection = GetComponentInChildren<GameTimeSection>();
        }
        
        public void LlamarCortinaCargaServer()
        {
            if (!IsServer) return;

            Debug.Log("<color=white>[GameState] -> Solicitando Cortina de Carga a la UI.</color>");
            if (uiEventSection != null)
            {
                uiEventSection.MostrarCortinaCarga();
            }
            else
            {
                Debug.LogError("[GlobalGameStateManager] uiEventSection es NULL.");
            }
        }
        
        public void IniciarRondaServer(string textoRound)
        {
            if (!IsServer) return;

            MatchInformationManager matchInfo = Object.FindFirstObjectByType<MatchInformationManager>();
            if (matchInfo != null)
            {
                matchInfo.ActivarDescuentoDeVida();
            }
            else
            {
                Debug.LogWarning("[GlobalGameStateManager] No se encontró la instancia de MatchInformationManager en la escena para reactivar el daño.");
            }

            Debug.Log($"<color=yellow>[GameState] -> Iniciando Ronda: {textoRound}. Ejecutando sincronización de interfaces y tiempos.</color>");
            
            if (uiEventSection != null) uiEventSection.MostrarAvisoInicioRound(textoRound);

            ReiniciarEIniciarCronometroLocalRpc(textoRound);
        }

        [Rpc(SendTo.Everyone)]
        private void ReiniciarEIniciarCronometroLocalRpc(string textoRound)
        {
            if (!IsServer && uiEventSection != null)
            {
                uiEventSection.MostrarAvisoInicioRound(textoRound);
            }

            if (timeSection != null)
            {
                if (IsServer)
                {
                    timeSection.AlAgotarseTiempoLocal -= EvaluarGanadorPorTiempoAgotado;
                }

                timeSection.IniciarCronometroMaestro();
                
                RespawnearObjetosLocalmenteEnCadaPantalla();

                if (IsServer)
                {
                    timeSection.AlAgotarseTiempoLocal += EvaluarGanadorPorTiempoAgotado;
                }
            }
            else
            {
                Debug.LogError("[GlobalGameStateManager] ¡Falta asignar el componente timeSection en el inspector!");
            }
        }

        private void RespawnearObjetosLocalmenteEnCadaPantalla()
        {
            foreach (var obj in objetosFijosEnEscenario)
            {
                if (obj == null) continue;

                if (obj.TryGetComponent<FallingObject>(out var fallingObj))
                {
                    fallingObj.RespawnearLocal();
                    if (IsServer) fallingObj.ResetearVariableRecogidoServer();
                }
                else if (obj.TryGetComponent<MasaLavaAscendente>(out var lavaObj))
                {
                    lavaObj.RespawnearLocal();
                }
            }
        }

        private void EvaluarGanadorPorTiempoAgotado()
        {
            if (!IsServer) return;
            
            if (timeSection != null) timeSection.AlAgotarseTiempoLocal -= EvaluarGanadorPorTiempoAgotado;

            Debug.Log("<color=red>[GameState] -> El tiempo de la ronda expiró en el Servidor. Evaluando vidas en MatchInformationManager...</color>");
        }
        
        public void FinDeRondaServer(string textoGanador)
        {
            if (!IsServer) return;

            MatchInformationManager matchInfo = Object.FindFirstObjectByType<MatchInformationManager>();
            if (matchInfo != null)
            {
                matchInfo.BloquearDescuentoDeVida();
            }
            else
            {
                Debug.LogWarning("[GlobalGameStateManager] No se encontró la instancia de MatchInformationManager en la escena para bloquear el daño.");
            }

            Debug.Log($"<color=red>[GameState] -> Fin de Ronda. Solicitando cartel de fin con texto: {textoGanador}.</color>");
            
            if (uiEventSection != null)
            {
                uiEventSection.MostrarAvisoFinRound(textoGanador);
            }
            else
            {
                Debug.LogError("[GlobalGameStateManager] uiEventSection es NULL al intentar mostrar Fin de Ronda.");
            }
        }
    }
}
