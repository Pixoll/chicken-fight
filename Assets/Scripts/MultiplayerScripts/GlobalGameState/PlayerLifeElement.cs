using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace MultiplayerScripts.GlobalGameState
{
    [RequireComponent(typeof(TMP_Text))]
    public class PlayerLifeTextElement : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private MatchInformationManager matchManager;

        [Header("Configuración de Testeo")]
        [Tooltip("Escribe aquí el nombre exacto de la gallina cuya vida quieres que muestre este texto (ej: Jugador_0)")]
        [SerializeField] private string nombreGallinaARastrear = "Jugador_0";

        private TMP_Text _textoVida;

        private void Awake()
        {
            _textoVida = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            if (matchManager == null)
            {
                matchManager = FindFirstObjectByType<MatchInformationManager>();
            }

            if (matchManager != null)
            {
                matchManager.AlModificarListaJugadores += OnVidaJugadorCambiada;
            }
        }

        private void OnVidaJugadorCambiada(NetworkListEvent<PlayerData> changeEvent)
        {
            if (changeEvent.Type != NetworkListEvent<PlayerData>.EventType.Value) return;

            PlayerData datosJugador = changeEvent.Value;

            Debug.Log($"[UI TEST] La red avisa que cambió: {datosJugador.nombreJugador}. Yo estoy buscando a: {nombreGallinaARastrear}");

            if (datosJugador.nombreJugador.ToString() == nombreGallinaARastrear)
            {
                ActualizarTexto(datosJugador.vidaActual);
            }
        }

        private void ActualizarTexto(float vida)
        {
            if (_textoVida != null)
            {
                _textoVida.text = $"PL: {Mathf.CeilToInt(vida)}";
            }
        }

        private void OnDestroy()
        {
            if (matchManager != null)
            {
                matchManager.AlModificarListaJugadores -= OnVidaJugadorCambiada;
            }
        }
    }
}