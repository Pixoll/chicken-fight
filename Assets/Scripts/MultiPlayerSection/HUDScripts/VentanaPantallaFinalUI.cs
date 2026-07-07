using System.Collections;
using MultiPlayerSection.NetworkScripts;
using TMPro;
using UnityEngine;

namespace MultiPlayerSection.HUDScripts
{
    public class VentanaPantallaFinalUI : MonoBehaviour
    {
        [Header("Componentes de Interfaz (Textos)")]
        [SerializeField] private TextMeshProUGUI textoNombreGanador;
        [SerializeField] private TextMeshProUGUI textoDatosJugador0;
        [SerializeField] private TextMeshProUGUI textoDatosJugador1;

        [Header("Contadores de Calaveras (Vidas Perdidas)")]
        [SerializeField] private GameObject[] calaverasJugador0 = new GameObject[3];
        [SerializeField] private GameObject[] calaverasJugador1 = new GameObject[3];

        private void Awake()
        {
            gameObject.SetActive(false);
            ApagarTodasLasCalaveras();
        }


        public void InicializarYMostrarPantallaFinal(string nombreJ0, string nombreJ1, string nombreGanador, int muertesJ0, int muertesJ1)
        {
            if (textoNombreGanador != null) textoNombreGanador.text = $"¡VICTORIA DEFINITIVA!\n{nombreGanador}";
            if (textoDatosJugador0 != null) textoDatosJugador0.text = nombreJ0;
            if (textoDatosJugador1 != null) textoDatosJugador1.text = nombreJ1;

            ApagarTodasLasCalaveras();
            ActualizarCalaverasEspecificas(calaverasJugador0, muertesJ0);
            ActualizarCalaverasEspecificas(calaverasJugador1, muertesJ1);

            gameObject.SetActive(true);

            StartCoroutine(RutinaTemporizadorRegresoMenu());
        }

        private IEnumerator RutinaTemporizadorRegresoMenu()
        {
            Debug.Log("<color=yellow>[UI Final] -> Pantalla final desplegada. Esperando 10 segundos para regresar automáticamente...</color>");
            
            yield return new WaitForSeconds(10f);

            Debug.Log("<color=orange>[UI Final] -> Tiempo cumplido. Solicitando salida sincronizada al MatchInformationManager...</color>");

            MatchInformationManager matchManager = Object.FindFirstObjectByType<MatchInformationManager>();
            if (matchManager != null)
            {
                matchManager.SolicitarSalirDePartidaGlobal();
            }
            else
            {
                Debug.LogError("[UI Final] -> No se pudo regresar de forma automática: Falta el MatchInformationManager en la escena.");
            }
        }

        private void ActualizarCalaverasEspecificas(GameObject[] listaCalaveras, int cantidadMuertes)
        {
            for (int i = 0; i < listaCalaveras.Length; i++)
            {
                if (listaCalaveras[i] != null)
                {
                    listaCalaveras[i].SetActive(i < cantidadMuertes);
                }
            }
        }

        private void ApagarTodasLasCalaveras()
        {
            foreach (var calavera in calaverasJugador0) if (calavera != null) calavera.SetActive(false);
            foreach (var calavera in calaverasJugador1) if (calavera != null) calavera.SetActive(false);
        }
    }
}
