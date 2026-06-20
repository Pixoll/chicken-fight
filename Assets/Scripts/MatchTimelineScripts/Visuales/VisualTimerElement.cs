using TMPro;
using UnityEngine;

namespace MatchTimelineScripts.Visuales
{
    [RequireComponent(typeof(TMP_Text))]
    public class VisualTimerElement : MonoBehaviour
    {
        private TMP_Text _textoContador;
        private float _cronometroInterno = 0f;
        private int _segundosMostrados = 0;

        private void Awake()
        {
            // Busca el componente de texto en este mismo GameObject
            _textoContador = GetComponent<TMP_Text>();
            
            // Inicializa el texto en 0
            if (_textoContador != null)
            {
                _textoContador.text = "0";
            }
        }

        private void OnEnable()
        {
            // Cada vez que el TimeSection active este objeto, reiniciamos sus valores
            _cronometroInterno = 0f;
            _segundosMostrados = 0;
            
            if (_textoContador != null)
            {
                _textoContador.text = "0";
            }
        }

        private void Update()
        {
            // El tiempo transcurre de forma independiente dentro del objeto
            _cronometroInterno += Time.deltaTime;

            // Si pasó un segundo completo, actualizamos el texto
            if (_cronometroInterno >= _segundosMostrados + 1)
            {
                _segundosMostrados += 1;
                
                if (_textoContador)
                {
                    _textoContador.text = _segundosMostrados.ToString();
                }
            }
        }
    }
}