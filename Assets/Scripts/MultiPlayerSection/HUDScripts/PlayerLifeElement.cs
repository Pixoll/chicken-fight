using MultiPlayerSection.NetworkScripts;
using MultiPlayerSection.PlayerScripts;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.HUDScripts
{
    [RequireComponent(typeof(TMP_Text))]
    public class PlayerLifeTextElement : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private MatchInformationManager matchManager;

        private TMP_Text _textoVida;
        private string _miNombreDeRedOficial = "";
        private bool _identidadEstablecida = false;

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

            EstablecerIdentidadLocal();
        }

        private void EstablecerIdentidadLocal()
        {
            if (_identidadEstablecida) return;

            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
            {
                ulong miIdLocal = NetworkManager.Singleton.LocalClientId;
                
                _miNombreDeRedOficial = "Jugador_" + miIdLocal;
                _identidadEstablecida = true;
            }
        }

        private void OnVidaJugadorCambiada(NetworkListEvent<PlayerData> changeEvent)
        {
            if (!_identidadEstablecida)
            {
                EstablecerIdentidadLocal();
                if (!_identidadEstablecida) return;
            }

            if (changeEvent.Type == NetworkListEvent<PlayerData>.EventType.RemoveAt || 
                changeEvent.Type == NetworkListEvent<PlayerData>.EventType.Remove) return;

            PlayerData datosJugador = changeEvent.Value;

            if (datosJugador.nombreJugador.ToString() == _miNombreDeRedOficial)
            {
                ActualizarTexto(datosJugador.vidaActual);
            }
        }

        private void ActualizarTexto(float vida)
        {
            if (_textoVida != null)
            {
                _textoVida.text = $"HP: {Mathf.CeilToInt(vida)}";
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
