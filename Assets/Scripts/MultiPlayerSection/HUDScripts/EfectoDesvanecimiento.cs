using System.Collections;
using UnityEngine;

namespace MultiPlayerSection.HUDScripts
{
    [RequireComponent(typeof(CanvasGroup))]
    public class EfectoDesvanecimiento : MonoBehaviour
    {
        [Header("Configuración de Tiempos")]
        [SerializeField] private float tiempoAparecer = 0.5f;   // Fade In
        [SerializeField] private float tiempoEspera = 1.0f;     // Tiempo que se queda visible
        [SerializeField] private float tiempoDesvanecer = 0.5f; // Fade Out

        [Header("Comportamiento")]
        [Tooltip("Si está marcado, el efecto se ejecutará solo en cuanto el objeto se encienda")]
        [SerializeField] private bool ejecutarAlActivar = true;

        private CanvasGroup _canvasGroup;
        private Coroutine _rutinaEfecto;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (ejecutarAlActivar)
            {
                DispararEfecto();
            }
        }

        /// <summary>
        /// Método público para activar este efecto manualmente desde cualquier otro script
        /// </summary>
        public void DispararEfecto()
        {
            // Si ya se estaba ejecutando, lo reiniciamos de forma segura
            if (_rutinaEfecto != null)
            {
                StopCoroutine(_rutinaEfecto);
            }
            _rutinaEfecto = StartCoroutine(RutinaFadeInOut());
        }

        private IEnumerator RutinaFadeInOut()
        {
            // --- 1. FADE IN (Aparecer gradualmente) ---
            float tiempoPasado = 0f;
            while (tiempoPasado < tiempoAparecer)
            {
                tiempoPasado += Time.deltaTime;
                // Calculamos el porcentaje de progreso (de 0 a 1)
                _canvasGroup.alpha = Mathf.Clamp01(tiempoPasado / tiempoAparecer);
                yield return null;
            }
            _canvasGroup.alpha = 1f; // Nos aseguramos de que quede totalmente opaco

            // --- 2. ESPERA (Mantenerse visible) ---
            yield return new WaitForSeconds(tiempoEspera);

            // --- 3. FADE OUT (Desvanecerse gradualmente) ---
            tiempoPasado = 0f;
            while (tiempoPasado < tiempoDesvanecer)
            {
                tiempoPasado += Time.deltaTime;
                // Restamos el progreso de 1 hacia 0
                _canvasGroup.alpha = Mathf.Clamp01(1f - (tiempoPasado / tiempoDesvanecer));
                yield return null;
            }
            _canvasGroup.alpha = 0f; // Nos aseguramos de que quede totalmente invisible
            
            _rutinaEfecto = null;
        }
    }
}
