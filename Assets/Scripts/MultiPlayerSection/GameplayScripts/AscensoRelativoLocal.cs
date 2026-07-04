using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.Environmental
{
    public class AscensoRelativoLocal : MonoBehaviour
    {
        [Header("Configuración de Movimiento")]
        [SerializeField] private float velocidadSubida = 0.5f;

        private bool _estaSubiendo = false;


        public void IniciarSubida()
        {
            _estaSubiendo = true;
            Debug.Log($"<color=teal>[Ascenso Local] -> Iniciando subida constante a {velocidadSubida} m/s desde la posición local actual.</color>");
        }


        public void DetenerSubida()
        {
            _estaSubiendo = false;
        }

        private void Update()
        {
            if (!_estaSubiendo) return;

            float desplazamientoY = velocidadSubida * Time.deltaTime;

            transform.localPosition += new Vector3(0f, desplazamientoY, 0f);
        }

        [ContextMenu("Probar Iniciar Subida")]
        private void TestIniciar() => IniciarSubida();

        [ContextMenu("Probar Detener Subida")]
        private void TestDetener() => DetenerSubida();
    }
}