using UnityEngine;

namespace MultiPlayerSection.Sus
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class EfectoOndasDeCalor : MonoBehaviour
    {
        [Header("Configuración del Calor")]
        [Tooltip("Qué tan rápido oscilan las ondas de calor")]
        [SerializeField] private float velocidadOndulacion = 5f;

        [Tooltip("La fuerza o tamaño de la distorsión horizontal")]
        [SerializeField] private float intensidadX = 0.08f;

        [Tooltip("La fuerza o tamaño de la distorsión vertical")]
        [SerializeField] private float intensidadY = 0.03f;

        [Header("Efecto de Escala (Opcional)")]
        [Tooltip("Simula el efecto de expansión por el aire caliente subiendo")]
        [SerializeField] private bool aplicarPulsoDeCalor = true;
        [SerializeField] private float velocidadPulso = 2f;
        [SerializeField] private float AmplitudPulso = 0.05f;

        private SpriteRenderer _spriteRenderer;
        private Vector3 _posicionInicialLocal;
        private Vector3 _escalaInicialLocal;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _posicionInicialLocal = transform.localPosition;
            _escalaInicialLocal = transform.localScale;
        }

        private void Update()
        {

            float desvioX = Mathf.Sin(Time.time * velocidadOndulacion) * intensidadX;
            float desvioY = Mathf.Cos(Time.time * velocidadOndulacion * 1.5f) * intensidadY;

            transform.localPosition = _posicionInicialLocal + new Vector3(desvioX, desvioY, 0f);

            if (aplicarPulsoDeCalor)
            {
                float factorPulso = Mathf.Sin(Time.time * velocidadPulso) * AmplitudPulso;
                transform.localScale = _escalaInicialLocal + new Vector3(factorPulso * 0.5f, factorPulso, 0f);
            }
        }

        private void OnDisable()
        {
            transform.localPosition = _posicionInicialLocal;
            transform.localScale = _escalaInicialLocal;
        }
    }
}