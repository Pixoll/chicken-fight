using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.Objects
{
    public class ObjectBoxCharacteristics : MonoBehaviour
    {
        [Header("Configuración del Objeto")]
        [Tooltip("ID numérico único que identifica este objeto de golpe.")]
        [SerializeField] private int objetoID = 0;

        public int ObjetoID => objetoID;
    }
}
