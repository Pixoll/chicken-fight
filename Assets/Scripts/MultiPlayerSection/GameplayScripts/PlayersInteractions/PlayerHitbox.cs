using System.Collections.Generic;
using MultiPlayerSection.GameplayScripts.Objects;
using MultiPlayerSection.PlayerScripts;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions
{
    public class PlayerHitbox : MonoBehaviour
    {
        private int _hurtboxLayer;
        private int _objectBoxLayer;
        
        private Collider2D[] _myColliders;
        private Transform _myChickenRoot;
        private PlayerImpactManager _impactManager;

        private struct TrackedInfo
        {
            public HurtboxCharacteristics characteristics;
            public float expireTime;
        }

        private readonly Dictionary<Collider2D, TrackedInfo> _trackedHurtboxes = new Dictionary<Collider2D, TrackedInfo>();

        private void Awake()
        {
            _hurtboxLayer = LayerMask.NameToLayer("Hurtbox");
            _objectBoxLayer = LayerMask.NameToLayer("ObjectBox");
            
            _myColliders = GetComponents<Collider2D>();
            _myChickenRoot = transform.root;
            _impactManager = _myChickenRoot.GetComponentInChildren<PlayerImpactManager>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == _hurtboxLayer)
            {
                if (collision.transform.root == _myChickenRoot) return;

                HurtboxCharacteristics characteristics = collision.GetComponent<HurtboxCharacteristics>();
                if (characteristics == null) return;

                if (IsImmuneTo(collision)) return;

                RegisterImpact(collision, characteristics);
        
                if (_impactManager != null)
                {
                    // Le pasamos las características y la colisión al manager local del atacante
                    _impactManager.ReceiveImpact(characteristics, collision.transform.right, collision.transform.up, collision.gameObject);
                }
                return;
            }

            if (collision.gameObject.layer == _objectBoxLayer)
            {
                ObjectBoxCharacteristics objectCharacteristics = collision.GetComponent<ObjectBoxCharacteristics>();
                if (objectCharacteristics == null) return;

                PlayerPunch playerPunch = _myChickenRoot.GetComponentInChildren<PlayerPunch>();

                if (playerPunch != null)
                {
                    if (objectCharacteristics.NombreObjeto == "Espada")
                    {
                        playerPunch.ObjetoActual = PlayerPunch.TipoObjetoEquipado.Espada;
                        Debug.Log($"[EQUIPAR] {gameObject.name} ha equipado: ESPADA");
                    }
                }

                Destroy(collision.gameObject);
            }
        }

        private void FixedUpdate()
        {
            EvaluateHurtboxExpirations();
        }

        private bool IsImmuneTo(Collider2D hurtbox)
        {
            if (_trackedHurtboxes.TryGetValue(hurtbox, out TrackedInfo info))
            {
                return Time.time < info.expireTime;
            }
            return false;
        }

        private void RegisterImpact(Collider2D hurtbox, HurtboxCharacteristics characteristics)
        {
            TrackedInfo info;
            info.characteristics = characteristics;
            info.expireTime = Time.time + characteristics.Cooldwon;
            
            _trackedHurtboxes[hurtbox] = info;
        }

        private void EvaluateHurtboxExpirations()
        {
            if (_trackedHurtboxes.Count == 0) return;

            List<Collider2D> toRemove = new List<Collider2D>();
            var keys = new List<Collider2D>(_trackedHurtboxes.Keys);

            foreach (Collider2D hurtbox in keys)
            {
                if (!hurtbox || !hurtbox.gameObject.activeInHierarchy)
                {
                    toRemove.Add(hurtbox);
                    continue;
                }

                bool isStillTouching = IsStillTouching(hurtbox);
                
                if (!isStillTouching)
                {
                    toRemove.Add(hurtbox);
                    continue;
                }

                TrackedInfo info = _trackedHurtboxes[hurtbox];
                bool cooldownEnded = Time.time >= info.expireTime;

                if (cooldownEnded && isStillTouching)
                {
                    RegisterImpact(hurtbox, info.characteristics);
                    
                    if (_impactManager)
                    {
                        _impactManager.ReceiveImpact(info.characteristics, hurtbox.transform.right, hurtbox.transform.up, hurtbox.gameObject);
                    }
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
