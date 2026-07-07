using MultiPlayerSection.NetworkScripts;
using UnityEngine;
using UnityEngine.UI;

namespace MultiPlayerSection.HUDScripts
{
    [RequireComponent(typeof(Button))]
    public class BotonSalirPartida : MonoBehaviour
    {
        private Button _boton;

        private void Awake()
        {
            // Obtiene el componente Button del mismo GameObject
            _boton = GetComponent<Button>();
        }

        private void OnEnable()
        {
            // Nos suscribimos al evento de click del botón
            if (_boton != null)
            {
                _boton.onClick.AddListener(AlHacerClick);
            }
        }

        private void OnDisable()
        {
            // Nos desuscribimos para evitar fugas de memoria
            if (_boton != null)
            {
                _boton.onClick.RemoveListener(AlHacerClick);
            }
        }

        private void AlHacerClick()
        {
            Debug.Log("<color=orange>[UI] -> Botón de salir oprimido. Buscando MatchInformationManager...</color>");

            // Buscamos el manager en la escena actual
            MatchInformationManager matchManager = Object.FindFirstObjectByType<MatchInformationManager>();

            if (matchManager != null)
            {
                // Desactivamos el botón inmediatamente para evitar que lo opriman varias veces
                if (_boton != null) _boton.interactable = false;

                // Ejecutamos la salida unificada para ambos jugadores en red
                matchManager.SolicitarSalirDePartidaGlobal();
            }
            else
            {
                Debug.LogError("[UI] -> No se pudo salir: No se encontró el MatchInformationManager en la escena.");
            }
        }
    }
}
