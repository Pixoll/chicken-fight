using MultiPlayerSection.PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace MultiPlayerSection.HUDScripts
{
    public class PlayerHealthBarUI : MonoBehaviour
    {
        [Header("Referencias de UI")]
        [Tooltip("Arrastra aquí la imagen de la barra verde (debe ser tipo 'Filled')")]
        [SerializeField] private Image barraVidaVerde;

        [Header("Configuración Máxima")]
        [Tooltip("El valor máximo de vida (por defecto 100, se adapta al manager)")]
        [SerializeField] private float vidaMaxima = 100f;

        private MultiPlayerSection.NetworkScripts.MatchInformationManager _matchManager;
        private string _miNombreIdentificadorLocal;

        private void Start()
        {
            // Esperamos un frame o buscamos la referencia del Match Manager en la escena
            _matchManager = Object.FindFirstObjectByType<MultiPlayerSection.NetworkScripts.MatchInformationManager>();

            if (_matchManager != null)
            {
                // Nos suscribimos al evento nativo que ya tienes en tu script
                _matchManager.AlModificarListaJugadores += OnVidaModificadaEnRed;
                
                // Definimos cuál es nuestro nombre único en base al ClientId asignado por el MatchManager
                _miNombreIdentificadorLocal = NetworkManager.Singleton.LocalClientId.ToString();
                
                // Actualización inicial preventiva
                ActualizarBarraVisualInicial();
            }
            else
            {
                Debug.LogError("[UI Barra Vida] -> No se encontró el MatchInformationManager en la escena.");
            }
        }

        private void OnDestroy()
        {
            if (_matchManager != null)
            {
                _matchManager.AlModificarListaJugadores -= OnVidaModificadaEnRed;
            }
        }

        /// <summary>
        /// Este método reacciona automáticamente cada vez que el MatchInformationManager altera la lista en red.
        /// </summary>
        private void OnVidaModificadaEnRed(NetworkListEvent<PlayerData> cambioEvent)
        {
            // Solo nos interesan los eventos de cambio de valor (Value) dentro de la lista
            if (cambioEvent.Type == NetworkListEvent<PlayerData>.EventType.Value)
            {
                // Verificamos si el elemento de la lista modificado corresponde a nuestra gallina local
                if (cambioEvent.Value.nombreJugador.ToString() == _miNombreIdentificadorLocal)
                {
                    float vidaActual = cambioEvent.Value.vidaActual;
                    
                    // Calculamos el fillAmount (va de 0f a 1f)
                    float nuevoFill = Mathf.Clamp01(vidaActual / vidaMaxima);
                    barraVidaVerde.fillAmount = nuevoFill;

                    Debug.Log($"<color=lime>[UI Local] -> Tu barra de vida se actualizó. HP: {vidaActual} -> FillAmount: {nuevoFill}</color>");
                }
            }
        }

        private void ActualizarBarraVisualInicial()
        {
            if (_matchManager == null) return;

            foreach (var jugador in _matchManager.ListaJugadores)
            {
                if (jugador.nombreJugador.ToString() == _miNombreIdentificadorLocal)
                {
                    barraVidaVerde.fillAmount = Mathf.Clamp01(jugador.vidaActual / vidaMaxima);
                    break;
                }
            }
        }
    }
}
