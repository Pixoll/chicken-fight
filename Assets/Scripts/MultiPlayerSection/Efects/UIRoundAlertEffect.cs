using UnityEngine;
using TMPro;
using DG.Tweening;

namespace MultiPlayerSection.Efects
{
    public class UIRoundAlertEffect : MonoBehaviour
    {
        [Header("Tiempos (Deben coincidir con tu GameUIEventSection)")]
        [SerializeField] private float tiempoDeVidaTotal = 2f;
        [SerializeField] private float duracionEntrada = 0.3f;
        [SerializeField] private float duracionSalida = 0.3f;

        [Header("Componentes")]
        [SerializeField] private RectTransform contenedorTexto;
        
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (contenedorTexto == null) return;


            contenedorTexto.localScale = Vector3.one * 3f; 
            _canvasGroup.alpha = 0f;

            contenedorTexto.DOScale(Vector3.one, duracionEntrada).SetEase(Ease.OutBack);
            _canvasGroup.DOFade(1f, duracionEntrada);

            contenedorTexto.DOPunchPosition(new Vector3(0, 15, 0), duracionEntrada, 5, 0.5f);

            float momentoSalida = Mathf.Max(0.1f, tiempoDeVidaTotal - duracionSalida);
            Invoke(nameof(EjecutarSalida), momentoSalida);
        }

        private void EjecutarSalida()
        {

            contenedorTexto.DOScaleX(2f, duracionSalida).SetEase(Ease.InQuad);
            contenedorTexto.DOScaleY(0.2f, duracionSalida).SetEase(Ease.InQuad);
            _canvasGroup.DOFade(0f, duracionSalida);
        }
    }
}
