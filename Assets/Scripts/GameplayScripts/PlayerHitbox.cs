using System.Collections.Generic;
using GameplayScripts.PlayerImpactsSection;
using UnityEngine;

namespace GameplayScripts
{
    public class PlayerHitbox : MonoBehaviour
    {
        private int _hurtboxLayer;
        private Collider2D[] _myColliders;
        private readonly HashSet<Collider2D> _damagedHurtboxes = new HashSet<Collider2D>();
        
        private Transform _myChickenRoot;

        private void Awake()
        {
            _hurtboxLayer = LayerMask.NameToLayer("Hurtbox");
            _myColliders = GetComponents<Collider2D>();
            
            // Buscamos el componente padre más alto en la jerarquía (la gallina)
            _myChickenRoot = transform.root;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == _hurtboxLayer)
            {
                if (collision.transform.root == _myChickenRoot) return;
                if (_damagedHurtboxes.Contains(collision)) return;

                HurtboxCharacteristics characteristics = collision.GetComponent<HurtboxCharacteristics>();
                if (characteristics != null) {
                    _damagedHurtboxes.Add(collision);

                    PlayerImpactManager manager = _myChickenRoot.GetComponent<PlayerImpactManager>();
            
                    if (manager != null)
                    {
                        manager.ReceiveImpact(characteristics);
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (_damagedHurtboxes.Count > 0)
            {
                _damagedHurtboxes.RemoveWhere(hurtbox =>
                {
                    if (!hurtbox || !hurtbox.gameObject.activeInHierarchy) return true;

                    foreach (Collider2D myCollider in _myColliders)
                    {
                        if (myCollider.IsTouching(hurtbox))
                        {
                            return false;
                        }
                    }
                    
                    Debug.Log($"<color=green>[Hitbox Unificada] El objeto {hurtbox.gameObject.name} salió del área total de los 3 cuadros. Reset listo.</color>");
                    return true;
                });
            }
        }
    }
}