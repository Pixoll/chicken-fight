using System.Collections.Generic;
using UnityEngine;

namespace MultiPlayerSection.Efects
{
    public class EfectoImpactoPared : MonoBehaviour
    {
        public enum OrigenVisual { Centro, BordeIzquierdo, BordeDerecho }

        [Header("Efecto Visual")]
        [Tooltip("Arrastra aquí el prefab de la imagen o destello que quieres que aparezca")]
        [SerializeField] private GameObject prefabEfectoVisual;
        [SerializeField] private float tiempoDeVidaEfecto = 1.5f;

        [Header("Proporción y Escala")]
        [Tooltip("Regula el tamaño de la imagen de forma proporcional. 1 = Tamaño original, 0.5 = Mitad, 2 = Doble")]
        [SerializeField] private float escalaMultiplicador = 1f;

        [Header("Punto de Anclaje de la Imagen")]
        [SerializeField] private OrigenVisual puntoDeOrigen = OrigenVisual.Centro;

        [Header("Configuración de Cooldown")]
        [SerializeField] private float cooldownPorGallina = 0.5f;

        private int _playerLayer;
        private Collider2D _miCollider;

        private struct RegistroContacto {
            public float tiempoExpiracionCooldown;
        }
        private readonly Dictionary<Collider2D, RegistroContacto> _gallinasEnContacto = new Dictionary<Collider2D, RegistroContacto>();

        private void Awake()
        {
            _playerLayer = LayerMask.NameToLayer("Player");
            _miCollider = GetComponent<Collider2D>();
            
            if (_playerLayer == -1)
            {
                Debug.LogError("<color=red>[EfectoImpactoPared] -> La capa 'Player' no existe.</color>");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision) => EvaluarYProcesarImpacto(collision);
        private void OnTriggerStay2D(Collider2D collision) => EvaluarYProcesarImpacto(collision);

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == _playerLayer && _gallinasEnContacto.ContainsKey(collision))
            {
                _gallinasEnContacto.Remove(collision);
            }
        }

        private void EvaluarYProcesarImpacto(Collider2D collision)
        {
            if (collision.gameObject.layer != _playerLayer) return;

            if (_gallinasEnContacto.TryGetValue(collision, out RegistroContacto registro))
            {
                if (Time.time < registro.tiempoExpiracionCooldown) return;
            }

            _gallinasEnContacto[collision] = new RegistroContacto {
                tiempoExpiracionCooldown = Time.time + cooldownPorGallina
            };

            if (prefabEfectoVisual == null || _miCollider == null) return;


            Vector3 puntoImpactoFinal = _miCollider.ClosestPoint(collision.transform.position);
            
            puntoImpactoFinal.z = transform.position.z - 0.1f;

            GameObject nuevaInstancia = Instantiate(prefabEfectoVisual, puntoImpactoFinal, Quaternion.identity);


            nuevaInstancia.transform.localScale = prefabEfectoVisual.transform.localScale * escalaMultiplicador;

            if (nuevaInstancia.TryGetComponent<SpriteRenderer>(out var spriteRenderer) && spriteRenderer.sprite != null)
            {
                float anchoReal = spriteRenderer.bounds.size.x;
                float mitadAncho = anchoReal / 2f;

                Vector3 desplazamiento = Vector3.zero;


                if (puntoDeOrigen == OrigenVisual.BordeIzquierdo)
                {
                    desplazamiento = transform.right * mitadAncho;
                }
                else if (puntoDeOrigen == OrigenVisual.BordeDerecho)
                {
                    desplazamiento = -transform.right * mitadAncho;
                }

                nuevaInstancia.transform.position += desplazamiento;
            }

            Debug.Log($"<color=lime>[EfectoImpactoPared] -> Imagen creada y ajustada. Escala: {escalaMultiplicador}</color>");
            Destroy(nuevaInstancia, tiempoDeVidaEfecto);
        }
    }
}