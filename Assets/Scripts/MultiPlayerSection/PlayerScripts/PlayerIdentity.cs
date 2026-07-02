using UnityEngine;

namespace MultiPlayerSection.PlayerScripts
{
    public class PlayerIdentity : MonoBehaviour
    {
        [Header("Identidad del Jugador")]
        [Tooltip("Nombre único de la gallina para registrar en el MatchManager (ej: Jugador_0, Jugador_1)")]
        [SerializeField] private string nombreIdentificador;

        public string NombreIdentificador 
        {
            get => nombreIdentificador;
            set => nombreIdentificador = value;
        }
    }
}