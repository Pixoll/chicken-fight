using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions
{
    public class HurtboxCharacteristics : MonoBehaviour
    {
        public enum ImpactType { Punch, Environmental, StatusEffect, SpecialAttack }
        public enum InclinacionVertical { Mid, Top, Bottom }
        public enum DireccionHorizontal { Forward, Backward, Up, Down }
        
        // 🌟 NUEVO: Clasificación de propiedad del daño
        public enum PropiedadDaño { ConDueño, SinDueño }

        [Header("Clasificación de Origen")]
        [SerializeField] private ImpactType impactType;
        [SerializeField] private PropiedadDaño propiedadOrigen = PropiedadDaño.ConDueño;

        [Header("Configuración de Empuje")]
        [SerializeField] private InclinacionVertical inclinacion = InclinacionVertical.Mid;
        [SerializeField] private DireccionHorizontal direccion = DireccionHorizontal.Forward;
        [SerializeField] private float knockbackForce = 15f;
        
        [Header("Efectos y Daño")]
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private float damageCooldown = 0.5f;
        [SerializeField] private bool stunning = false;
        [SerializeField] private float stunningTime = 0f;

        // Propiedades públicas
        public ImpactType Type => impactType;
        public PropiedadDaño Propiedad => propiedadOrigen; // 🌟 Expuesta para el Manager
        public InclinacionVertical Inclinacion => inclinacion;
        public DireccionHorizontal Direccion => direccion;
        public float Knockback => knockbackForce;
        public float Damage => damageAmount;
        public float Cooldwon => damageCooldown;
        public bool Stunning => stunning;
        public float StunningTime => stunningTime;
    }
}
