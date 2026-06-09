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

        [Header("Clasificación del Impacto")]
        [SerializeField] private ImpactType impactType = ImpactType.Punch;

        [Header("Atributos del Daño")]
        [SerializeField] private float damageAmount = 10f;
        [SerializeField] private float knockbackForce = 15f;

        public ImpactType Type => impactType;
        public float Damage => damageAmount;
        public float Knockback => knockbackForce;
        

        public Vector3 GetOriginPosition()
        {
            return transform.position;
        }
    }
}