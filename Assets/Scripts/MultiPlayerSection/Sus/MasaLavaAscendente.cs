using System.Collections;
using UnityEngine;

namespace MultiPlayerSection.Efects
{
    public class MasaLavaAscendente : MonoBehaviour
    {
        [Header("Configuración de Activación (🌟 NUEVO)")]
        [Tooltip("Tiempo en segundos que la lava esperará quieta antes de empezar a subir")]
        [SerializeField] private float tiempoEsperaAntesDeSubir = 5f;

        [Header("Movimiento Vertical (Subida)")]
        [Tooltip("Qué tan rápido sube el bloque de lava en unidades por segundo")]
        [SerializeField] private float velocidadSubida = 0.5f;

        [Header("Movimiento Horizontal (Temblor)")]
        [Tooltip("Qué tan rápido tiembla de izquierda a derecha")]
        [SerializeField] private float velocidadTemblor = 35f;

        [Tooltip("La distancia máxima que se desvía hacia los lados")]
        [SerializeField] private float intensidadTemblor = 0.06f;

        private Vector3 _posicionInicialLocal;
        private float _progresoVertical;
        private bool _estaSubiendo;
        private Coroutine _cronometroLavaCoroutine;

        private void Awake()
        {
            _posicionInicialLocal = transform.localPosition;
            ConfigurarEstadoInicial();
        }

        private void Start()
        {
            IniciarCronometroLava();
        }

        private void IniciarCronometroLava()
        {
            if (_cronometroLavaCoroutine != null) 
            {
                StopCoroutine(_cronometroLavaCoroutine);
            }
            
            _cronometroLavaCoroutine = StartCoroutine(CronometroActivacionLavaRoutine());
        }

        private IEnumerator CronometroActivacionLavaRoutine()
        {
            _estaSubiendo = false;

            yield return new WaitForSeconds(tiempoEsperaAntesDeSubir);

            _estaSubiendo = true;
            Debug.Log($"<color=red>[Lava Local] -> ¡Tiempo de espera agotado! La masa de peligro comienza a subir.</color>");
        }

        private void Update()
        {
            float temblorX = Mathf.Sin(Time.time * velocidadTemblor) * intensidadTemblor;

            if (_estaSubiendo)
            {
                _progresoVertical += velocidadSubida * Time.deltaTime;
            }

            transform.localPosition = new Vector3(
                _posicionInicialLocal.x + temblorX, 
                _posicionInicialLocal.y + _progresoVertical, 
                _posicionInicialLocal.z
            );
        }

        private void ConfigurarEstadoInicial()
        {
            _progresoVertical = 0f;
            _estaSubiendo = false;
        }

        public void RespawnearLocal()
        {
            transform.localPosition = _posicionInicialLocal;
            
            ConfigurarEstadoInicial();

            IniciarCronometroLava();

            Debug.Log($"<color=orange>[Lava Local] -> Reseteada y Cronómetro reiniciado. Esperando {tiempoEsperaAntesDeSubir}s para subir.</color>");
        }


        public void SetSubidaActiva(bool activa)
        {
            _estaSubiendo = activa;
            if (!activa && _cronometroLavaCoroutine != null)
            {
                StopCoroutine(_cronometroLavaCoroutine);
            }
        }

        public void ResetearVariableRecogidoServer() { }
    }
}
