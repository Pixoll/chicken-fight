using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions
{
    public class HurtboxCharacteristics : MonoBehaviour
    {
        public enum InclinacionVertical { Mid, Top, Bottom }
        public enum DireccionHorizontal { Forward, Backward, Up, Down }
        public enum PropiedadDaño { ConDueño, SinDueño }

        [Header("Clasificación de Origen")]
        [SerializeField] private PropiedadDaño propiedadOrigen = PropiedadDaño.ConDueño;

        [Header("Configuración de Empuje")]
        [SerializeField] private InclinacionVertical inclinacion = InclinacionVertical.Mid;
        [SerializeField] private DireccionHorizontal direccion = DireccionHorizontal.Forward;
        [SerializeField] private float knockbackForce = 15f;
        
        [Header("Efectos Básicos")]
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private float damageCooldown = 0.5f;
        [SerializeField] private bool stunning = false;
        [SerializeField] private float stunningTime = 0f;

        [Header("Nuevos Efectos de Estado")]
        [SerializeField] private float healAmount = 0f;
        [SerializeField] private bool appliesSlow = false;
        [Range(0f, 1f)] 
        [SerializeField] private float slowIntensity = 0.5f;
        [SerializeField] private float slowDuration = 0f; 

        public PropiedadDaño Propiedad => propiedadOrigen;
        public InclinacionVertical Inclinacion => inclinacion;
        public DireccionHorizontal Direccion => direccion;
        public float Knockback => knockbackForce;
        public float Damage => damageAmount;
        public float Cooldwon => damageCooldown;
        public bool Stunning => stunning;
        public float StunningTime => stunningTime;
        
        public float Heal => healAmount;
        public bool AppliesSlow => appliesSlow;
        public float SlowIntensity => slowIntensity;
        public float SlowDuration => slowDuration;
    }
}
