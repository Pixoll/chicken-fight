using TMPro;
using UnityEngine;

namespace MultiPlayerSection.HUDScripts
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class HUDCronometroLocal : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private float tiempoInicial = 99f;

        private TextMeshProUGUI _textoCronometro;
        private float _tiempoActual;
        private bool _corriendo = false;

        private void Awake()
        {
            _textoCronometro = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            _tiempoActual = tiempoInicial;
            _corriendo = true;
            ActualizarTexto();
        }

        private void Update()
        {
            if (!_corriendo) return;

            if (_tiempoActual > 0f)
            {
                _tiempoActual -= Time.deltaTime;
                ActualizarTexto();
            }
            else
            {
                _tiempoActual = 0f;
                _corriendo = false;
                ActualizarTexto();
            }
        }

        private void ActualizarTexto()
        {
            if (_textoCronometro == null) return;

            int segundosEnteros = Mathf.CeilToInt(_tiempoActual);
            _textoCronometro.text = segundosEnteros.ToString("F0");
        }
    }
}
