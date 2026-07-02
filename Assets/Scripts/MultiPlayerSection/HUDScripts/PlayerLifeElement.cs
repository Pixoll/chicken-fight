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

            // Intentamos calcular nuestra identidad de inmediato si la red ya levantó
            EstablecerIdentidadLocal();
        }

        private void EstablecerIdentidadLocal()
        {
            if (_identidadEstablecida) return;

            // Verificamos si el NetworkManager local ya está activo y corriendo en esta máquina
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
            {
                ulong miIdLocal = NetworkManager.Singleton.LocalClientId;
                
                // Construimos el string exacto que el MatchInformationManager le inyecta a las gallinas
                _miNombreDeRedOficial = "Jugador_" + miIdLocal;
                _identidadEstablecida = true;

                Debug.Log($"<color=green><b>[UI LOCAL AUTOMÁTICA]</b> Esta interfaz se ha enlazado a sí misma. Buscando datos de: {_miNombreDeRedOficial}</color>");
            }
        }

        private void OnVidaJugadorCambiada(NetworkListEvent<PlayerData> changeEvent)
        {
            // Si no se pudo establecer al Start porque la red no estaba lista, lo reintentamos aquí
            if (!_identidadEstablecida)
            {
                EstablecerIdentidadLocal();
                if (!_identidadEstablecida) return; // Si sigue sin estar lista, esperamos al siguiente cambio
            }

            // Ignoramos eventos de eliminación de datos de la lista
            if (changeEvent.Type == NetworkListEvent<PlayerData>.EventType.RemoveAt || 
                changeEvent.Type == NetworkListEvent<PlayerData>.EventType.Remove) return;

            PlayerData datosJugador = changeEvent.Value;

            // 👁️ FILTRO CIEGO: Solo reaccionamos si el cambio en la lista le pertenece a MI ID local
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
