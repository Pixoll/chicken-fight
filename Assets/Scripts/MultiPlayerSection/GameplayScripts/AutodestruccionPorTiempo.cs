using UnityEngine;

namespace MultiPlayerSection.GameplayScripts
{
    public class AutodestruccionPorTiempo : MonoBehaviour
    {
        [Header("Configuración de Tiempo")]
        [Tooltip("Tiempo en segundos antes de que este objeto se elimine por completo de la escena.")]
        [SerializeField] private float tiempoParaDestruir = 1.0f;

        private void Start()
        {
            Destroy(gameObject, tiempoParaDestruir);
        }
    }
}