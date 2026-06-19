using System.Collections.Generic;
using UnityEngine;

namespace GameplayScripts.PlayerImpactsSection
{
    public class PlayerHitbox : MonoBehaviour
    {
        private int _hurtboxLayer;
        private Collider2D[] _myColliders;
        private Transform _myChickenRoot;
        private PlayerImpactManager _impactManager;

        private readonly Dictionary<Collider2D, float> _trackedHurtboxes = new Dictionary<Collider2D, float>();

        private void Awake()
        {
            _hurtboxLayer = LayerMask.NameToLayer("Hurtbox");
            _myColliders = GetComponents<Collider2D>();
            _myChickenRoot = transform.root;
            _impactManager = _myChickenRoot.GetComponentInChildren<PlayerImpactManager>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer != _hurtboxLayer) return;
            if (collision.transform.root == _myChickenRoot) return;

            HurtboxCharacteristics characteristics = collision.GetComponent<HurtboxCharacteristics>();
            if (characteristics == null) return;

            if (IsImmuneTo(collision)) return;

            RegisterImpact(collision, characteristics.Cooldwon);
    
            if (_impactManager != null)
            {
                _impactManager.ReceiveImpact(characteristics, collision.transform.right, collision.transform.up);
            }
        }

        private void FixedUpdate()
        {
            EvaluateHurtboxExpirations();
        }

 
        private bool IsImmuneTo(Collider2D hurtbox)
        {
            if (_trackedHurtboxes.TryGetValue(hurtbox, out float cooldownExpireTime))
            {
                return Time.time < cooldownExpireTime;
            }
            return false;
        }


        private void RegisterImpact(Collider2D hurtbox, float cooldownDuration)
        {
            float expireTime = Time.time + cooldownDuration;
            
            if (_trackedHurtboxes.ContainsKey(hurtbox))
            {
                _trackedHurtboxes[hurtbox] = expireTime;
            }
            else
            {
                _trackedHurtboxes.Add(hurtbox, expireTime);
            }
        }


        private void EvaluateHurtboxExpirations()
        {
            if (_trackedHurtboxes.Count == 0) return;

            List<Collider2D> toRemove = new List<Collider2D>();

            foreach (var kvp in _trackedHurtboxes)
            {
                Collider2D hurtbox = kvp.Key;
                float expireTime = kvp.Value;

                if (!hurtbox || !hurtbox.gameObject.activeInHierarchy)
                {
                    toRemove.Add(hurtbox);
                    continue;
                }

                bool cooldownEnded = Time.time >= expireTime;

                bool isNotTouching = !IsStillTouching(hurtbox);

                if (cooldownEnded || isNotTouching)
                {
                    toRemove.Add(hurtbox);
                }
            }

            foreach (Collider2D hurtbox in toRemove)
            {
                _trackedHurtboxes.Remove(hurtbox);
            }
        }
        
        private bool IsStillTouching(Collider2D hurtbox)
        {
            foreach (Collider2D myCollider in _myColliders)
            {
                if (myCollider.IsTouching(hurtbox))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
