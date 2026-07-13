using UnityEngine;
using DG.Tweening;

namespace MultiPlayerSection.Efects
{
    public class UIAutotweenEffect : MonoBehaviour
    {
        [Header("Configuración del Tiempo")]
        [SerializeField] private float tiempoDeVidaTotal = 3f;

        [Header("Configuración de Animación")]
        [SerializeField] private float duracionEntrada = 0.5f;
        [SerializeField] private float duracionSalida = 0.5f;

        [Header("Componente Texto")]
        [SerializeField] private RectTransform textoFijoCarga;

        private RectTransform _rectTransform;
        private CanvasGroup _textoCanvasGroup;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            
            if (textoFijoCarga != null)
            {
                _textoCanvasGroup = textoFijoCarga.GetComponent<CanvasGroup>();
                if (_textoCanvasGroup == null)
                {
                    _textoCanvasGroup = textoFijoCarga.gameObject.AddComponent<CanvasGroup>();
                }
                textoFijoCarga.SetParent(transform.parent, true);
            }
            
            if (_rectTransform != null)
            {
                Vector2 tamañoActual = _rectTransform.sizeDelta;
                _rectTransform.sizeDelta = new Vector2(tamañoActual.x, Screen.height * 1.2f);
            }
        }

        private void Start()
        {
            if (_rectTransform == null) return;

            float posicionFueraDerecha = Screen.width * 1.5f;
            _rectTransform.anchoredPosition = new Vector2(posicionFueraDerecha, 0f);

            if (_textoCanvasGroup != null)
            {
                _textoCanvasGroup.alpha = 0f;
            }

            _rectTransform.DOAnchorPosX(0f, duracionEntrada).SetEase(Ease.OutQuad);
            
            if (_textoCanvasGroup != null)
            {
                _textoCanvasGroup.DOFade(1f, duracionEntrada);
            }

            float momentoDeSalida = Mathf.Max(0.1f, tiempoDeVidaTotal - duracionSalida);
            Invoke(nameof(EjecutarAnimacionSalida), momentoDeSalida);
        }

        private void EjecutarAnimacionSalida()
        {
            if (_rectTransform == null) return;

            float posicionFueraIzquierda = -Screen.width * 1.5f;
            _rectTransform.DOAnchorPosX(posicionFueraIzquierda, duracionSalida).SetEase(Ease.InQuad);

            if (_textoCanvasGroup != null)
            {
                _textoCanvasGroup.DOFade(0f, duracionSalida);
            }
        }
    }
}
