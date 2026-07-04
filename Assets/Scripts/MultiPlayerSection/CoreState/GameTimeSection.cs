using System.Collections.Generic;
using MultiPlayerSection.HUDScripts;
using UnityEngine;

namespace MultiPlayerSection.CoreState
{
    public class GameTimeSection : MonoBehaviour
    {
        [Header("Configuración General")]
        [SerializeField] private float tiempoMaximoRonda = 99f;

        [Header("Línea de Tiempo Dinámica")]
        [SerializeField] private List<TimeElement> lineaDeTiempo = new List<TimeElement>();

        private Dictionary<int, GameObject> _instanciasClonadas = new Dictionary<int, GameObject>();
        private float _tiempoRestante;
        private bool _rondaActiva = false;

        public System.Action<float> AlCambiarTiempoLocal;
        public System.Action AlAgotarseTiempoLocal;

        private void Awake()
        {
            if (TryGetComponent<RectTransform>(out var rect))
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
            }
        }


        public void LimpiarYReiniciarSeccionLocal()
        {
            _rondaActiva = false;
            _tiempoRestante = 0f;

            foreach (var kvp in _instanciasClonadas)
            {
                if (kvp.Value != null) 
                {
                    Destroy(kvp.Value);
                }
            }
            _instanciasClonadas.Clear();

            foreach (var elemento in lineaDeTiempo)
            {
                if (elemento != null)
                {
                    elemento.yaAparecio = false;
                    elemento.yaTermino = false;
                }
            }

            Debug.Log("<color=orange>[GameTimeSection] -> Sección de tiempo limpiada y reseteada con éxito para la siguiente ronda.</color>");
        }


        public void IniciarCronometroMaestro()
        {
            LimpiarYReiniciarSeccionLocal();

            _tiempoRestante = tiempoMaximoRonda;
            _rondaActiva = true;

            Debug.Log($"<color=green>[GameTimeSection] -> Cronómetro Maestro INICIADO. Tiempo máximo: {tiempoMaximoRonda}s.</color>");
        }

        private void Update()
        {
            if (!_rondaActiva) return;

            if (_tiempoRestante > 0f)
            {
                _tiempoRestante -= Time.deltaTime;
                AlCambiarTiempoLocal?.Invoke(_tiempoRestante);
                ProcesarEventosDeTiempoLocal(_tiempoRestante);
            }
            else
            {
                _tiempoRestante = 0f;
                _rondaActiva = false;
                AlAgotarseTiempoLocal?.Invoke();
                FinalizarRondaPorTiempoLocal();
            }
        }

        private void ProcesarEventosDeTiempoLocal(float segundoActual)
        {
            for (int i = 0; i < lineaDeTiempo.Count; i++)
            {
                TimeElement elemento = lineaDeTiempo[i];
                if (elemento.objetoVisualoFisico == null) continue;

                if (segundoActual <= elemento.segundoAparicion && !elemento.yaAparecio && !elemento.yaTermino)
                {
                    elemento.yaAparecio = true;
                    GestionarEstadoObjetoLocal(i, true, false);
                }

                if (segundoActual <= elemento.segundoDesaparicion && elemento.yaAparecio && !elemento.yaTermino)
                {
                    elemento.yaTermino = true;
                    bool destruir = elemento.queHacerAlTerminar == TimeElement.AccionFinal.DestruirObjeto;
                    GestionarEstadoObjetoLocal(i, false, destruir);
                }
            }
        }

        private void GestionarEstadoObjetoLocal(int indiceElemento, bool activar, bool destruir)
        {
            if (indiceElemento < 0 || indiceElemento >= lineaDeTiempo.Count) return;

            TimeElement elemento = lineaDeTiempo[indiceElemento];
            if (elemento.objetoVisualoFisico == null) return;

            if (activar)
            {
                GameObject clon = Instantiate(elemento.objetoVisualoFisico);
                _instanciasClonadas[indiceElemento] = clon;

                if (elemento.puntoDeSpawneo != null)
                {
                    clon.transform.SetParent(elemento.puntoDeSpawneo, false);
                    clon.transform.position = elemento.puntoDeSpawneo.position;
                    
                    if (elemento.heredarTransformCompleto)
                    {
                        clon.transform.rotation = elemento.puntoDeSpawneo.rotation;
                        clon.transform.localScale = elemento.puntoDeSpawneo.localScale;
                    }
                }

                Debug.Log($"<color=teal>[Timeline] -> Activado GameObject: '{elemento.objetoVisualoFisico.name}' en el segundo: {_tiempoRestante:F2}</color>");
            }
            else
            {
                if (_instanciasClonadas.TryGetValue(indiceElemento, out GameObject clonAsociado))
                {
                    if (clonAsociado == null) return;

                    if (destruir) Destroy(clonAsociado);
                    else clonAsociado.transform.SetParent(null);

                    _instanciasClonadas.Remove(indiceElemento);
                }
            }
        }

        private void FinalizarRondaPorTiempoLocal()
        {
            Debug.Log("<color=red>[GameTimeSection] -> ¡TIEMPO AGOTADO LOCALMENTE! Esperando resolución del Servidor...</color>");
        }

        private void OnDestroy()
        {
            foreach (var kvp in _instanciasClonadas)
            {
                if (kvp.Value != null) Destroy(kvp.Value);
            }
            _instanciasClonadas.Clear();
        }
        
        public float TiempoRestante => _tiempoRestante;
    }
}
