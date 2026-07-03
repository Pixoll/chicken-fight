using System.Collections.Generic;
using MultiPlayerSection.PlayerScripts;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions
{
    public class PlayerHitbox : MonoBehaviour
    {
        private int _hurtboxLayer;
        private Transform _myChickenRoot;
        private PlayerImpactManager _impactManager;

        private struct HurtboxRegistro {
            public float tiempoExpiracionCooldown;
        }

        private readonly Dictionary<Collider2D, HurtboxRegistro> _hurtboxesEnContacto = new Dictionary<Collider2D, HurtboxRegistro>();

        private void Awake() {
            _hurtboxLayer = LayerMask.NameToLayer("Hurtbox");
            _myChickenRoot = transform.root;
            
            _impactManager = _myChickenRoot.GetComponentInChildren<PlayerImpactManager>();
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            EvaluarYProcesarImpacto(collision);
        }

        private void OnTriggerStay2D(Collider2D collision) {
            EvaluarYProcesarImpacto(collision);
        }

        private void OnTriggerExit2D(Collider2D collision) {
            if (collision.gameObject.layer == _hurtboxLayer) {
                if (_hurtboxesEnContacto.ContainsKey(collision)) {
                    _hurtboxesEnContacto.Remove(collision);
                }
            }
        }

        private void EvaluarYProcesarImpacto(Collider2D collision) {
            if (collision.gameObject.layer != _hurtboxLayer) return;
            if (collision.transform.root == _myChickenRoot) return;

            HurtboxCharacteristics characteristics = collision.GetComponent<HurtboxCharacteristics>();
            if (characteristics == null) return;

            if (_hurtboxesEnContacto.TryGetValue(collision, out HurtboxRegistro registro)) {
                if (Time.time < registro.tiempoExpiracionCooldown) return; 
            }

            _hurtboxesEnContacto[collision] = new HurtboxRegistro {
                tiempoExpiracionCooldown = Time.time + characteristics.Cooldwon
            };

            Debug.Log($"<color=lime>[PlayerHitbox] -> Trigger detectado y enviado al Manager. Hurtbox: {collision.gameObject.name}</color>");

            if (_impactManager != null) {
                _impactManager.ReceiveImpact(characteristics, collision.transform.right, collision.transform.up, collision.gameObject);
            }
            else {
                Debug.LogError($"<color=red>[ERROR CRÍTICO] -> ¡PlayerImpactManager NO ENCONTRADO en el prefab de la gallina!</color>");
            }
        }
    }
}
