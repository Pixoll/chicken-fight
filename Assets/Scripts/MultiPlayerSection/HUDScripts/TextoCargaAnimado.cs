using System.Collections;
using TMPro;
using UnityEngine;

namespace MultiPlayerSection.HUDScripts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextoCargaAnimado : MonoBehaviour
    {
        [Header("Configuración del Texto")]
        [Tooltip("El texto base que se mostrará antes de los puntos. Ej: Cargando, Conectando, Buscando")]
        [SerializeField] private string textoBase = "Cargando";

        [Tooltip("Tiempo en segundos para cambiar entre cada punto")]
        [SerializeField] private float velocidadAnimacion = 0.5f;

        [Header("Configuración del Sonido Único")]
        [SerializeField] private AudioSource sonidoDeInicio;
        [Range(0f, 1f)] [SerializeField] private float volumenSonido = 1f;
        [SerializeField] private float tiempoEspera = 1f;

        private static bool _playedSound;

        private TextMeshProUGUI _textMeshPro;
        private Coroutine _rutinaAnimacion;

        private void Awake()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            _rutinaAnimacion = StartCoroutine(RutinaPuntosSuspensivos());

            StartCoroutine(RutinaSonido());
        }

        private void OnDisable()
        {
            if (_rutinaAnimacion != null)
            {
                StopCoroutine(_rutinaAnimacion);
                _rutinaAnimacion = null;
            }
        }

        private IEnumerator RutinaPuntosSuspensivos()
        {
            int contadorPuntos = 0;

            while (true)
            {
                string textoFinal = textoBase;

                switch (contadorPuntos)
                {
                    case 1:
                        textoFinal += " .";
                        break;
                    case 2:
                        textoFinal += " . .";
                        break;
                    case 3:
                        textoFinal += " . . .";
                        break;
                    default:
                        // Caso 0: Se queda solo con el texto base sin puntos
                        break;
                }

                if (_textMeshPro != null)
                {
                    _textMeshPro.text = textoFinal;
                }

                contadorPuntos = (contadorPuntos + 1) % 4;

                yield return new WaitForSeconds(velocidadAnimacion);
            }
        }

        private IEnumerator RutinaSonido() {
            if (_playedSound || sonidoDeInicio == null) yield break;

            yield return new WaitForSeconds(velocidadAnimacion);

            StartCoroutine(AudioPlayer.Play(sonidoDeInicio, volumenSonido, 0.1f));
            _playedSound = true;
        }

        public void CambiarTextoBase(string nuevoTexto)
        {
            textoBase = nuevoTexto;
        }
    }
}