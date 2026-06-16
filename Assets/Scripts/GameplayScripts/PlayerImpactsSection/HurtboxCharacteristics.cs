using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection
{
    public class HurtboxCharacteristics : MonoBehaviour
    {
        public enum ImpactType
        {
            Punch,
            Environmental,
            StatusEffect,
            SpecialAttack
        }
        public enum KnockbackDirection
        {
            Top,
            Left,
            Right,
            TopLeft,
            TopRight
        }

        [Header("Impact Classification")]
        [SerializeField] private ImpactType impactType;

        [Header("characteristics of the damage")]
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private float damageCooldown = 10f;
        [SerializeField] private float knockbackForce = 15f;
        [SerializeField] private KnockbackDirection knockbackDirection;

        [Header("Efects")]
        [SerializeField] private bool stunning = false;
        [SerializeField] private float stunningTime = 0f;


        public ImpactType Type => impactType;
        
        public float Damage => damageAmount;
        public float Cooldwon => damageCooldown;
        
        public float Knockback => knockbackForce;
        public KnockbackDirection Direction => knockbackDirection;
        
        public bool Stunning => stunning;
        public float StunningTime => stunningTime;
    }
}
