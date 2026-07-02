using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.Objects
{
    public class ObjectBoxCharacteristics : MonoBehaviour
    {
        [Header("Configuración del Objeto")]
        [Tooltip("ID o Nombre único de este objeto para registrar en el inventario/red")]
        [SerializeField] private string nombreObjeto = "Espada_Madera";

        public string NombreObjeto => nombreObjeto;
    }
}
