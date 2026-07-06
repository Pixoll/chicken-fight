using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.Objects
{
    public class PlayerObjectAttackManager : MonoBehaviour
    {
        [Header("Configuración de IDs de Golpe")]
        [Tooltip("El ID del objeto de golpe que el jugador usará al iniciar la ronda.")]
        [SerializeField] private int idGolpeBase = 0;

        [Tooltip("ID del objeto de golpe que se encuentra activo en este frame actual.")]
        [SerializeField] private int idGolpeActivoActual;

        private void Awake()
        {
            // Inicializa la ronda con el ataque base (por ejemplo, puño normal)
            ResetearAlGolpeBase();
        }

        /// <summary>
        /// Cambia el ID del golpe actual por uno nuevo (ej: al equipar un arma o cambiar de habilidad).
        /// </summary>
        public void CambiarIDGolpeActivo(int nuevoID)
        {
            idGolpeActivoActual = nuevoID;
            Debug.Log($"<color=cyan>[ObjectManager] -> ID de golpe actualizado a: {idGolpeActivoActual}</color>");
        }

        /// <summary>
        /// Restablece el ID del golpe activo al valor base definido en el inspector.
        /// </summary>
        public void ResetearAlGolpeBase()
        {
            idGolpeActivoActual = idGolpeBase;
            Debug.Log($"<color=orange>[ObjectManager] -> ID de golpe reseteado al estado base: {idGolpeActivoActual}</color>");
        }

        // Propiedades públicas para lectura externa
        public int IdGolpeActivoActual => idGolpeActivoActual;
        public int IdGolpeBase => idGolpeBase;
    }
}
