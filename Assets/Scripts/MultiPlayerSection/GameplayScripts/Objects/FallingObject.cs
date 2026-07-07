using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.Objects
{
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class FallingObject : NetworkBehaviour
    {
        [Header("Configuración de Caída")]
        [SerializeField] private float tiempoFlotando = 3f;
        [SerializeField] private LayerMask capaSuelo;
        
        [Header("Configuración de Recompensa")]
        [SerializeField] private int objetoID = 1;
        [SerializeField] private LayerMask capaJugador;

        private Rigidbody2D _rb;
        private bool _yaSuelo = false;
        
        private Vector3 _posicionInicial;
        private Coroutine _cronometroCoroutine;
        private Collider2D _collider;
        private SpriteRenderer _spriteRenderer;

        private readonly NetworkVariable<bool> _yaRecogido = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            _posicionInicial = transform.position;

            ConfigurarEstadoFlotandoInicial();
        }

        private void Start()
        {
            IniciarCronometro();
        }

        private void IniciarCronometro()
        {
            if (_cronometroCoroutine != null) StopCoroutine(_cronometroCoroutine);
            _cronometroCoroutine = StartCoroutine(CronometroFlotacionRoutine());
        }

        private IEnumerator CronometroFlotacionRoutine()
        {
            yield return new WaitForSeconds(tiempoFlotando);
            if (!_yaSuelo)
            {
                _rb.gravityScale = 1f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & capaJugador) != 0)
            {
                NetworkObject playerNetObj = collision.transform.root.GetComponent<NetworkObject>();
                if (playerNetObj != null && playerNetObj.IsOwner)
                {
                    IntentarRecogerObjeto(playerNetObj);
                }
                return;
            }

            if (_yaSuelo || ((1 << collision.gameObject.layer) & capaSuelo) == 0) return;
            FrenarEnSuelo();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_yaSuelo || ((1 << collision.gameObject.layer) & capaSuelo) == 0) return;
            FrenarEnSuelo();
        }

        private void FrenarEnSuelo()
        {
            _yaSuelo = true;
            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll; 
        }

        private void ConfigurarEstadoFlotandoInicial()
        {
            _yaSuelo = false;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = Vector2.zero;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation; 
        }

        public void IntentarRecogerObjeto(NetworkObject playerNetObj)
        {
            SolicitarRecogidaServerRpc(playerNetObj);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SolicitarRecogidaServerRpc(NetworkObjectReference playerNetObjRef)
        {
            if (_yaRecogido.Value) return;

            _yaRecogido.Value = true;

            if (playerNetObjRef.TryGet(out NetworkObject playerNetObj))
            {
                PlayerObjectAttackManager attackManager = playerNetObj.GetComponentInChildren<PlayerObjectAttackManager>();
                if (attackManager != null)
                {
                    attackManager.CambiarIDGolpeActivo(objetoID);
                }

                SincronizarIdAtaqueClienteRpc(playerNetObjRef, objetoID);
            }

            OcultarObjetoEnClientesRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void SincronizarIdAtaqueClienteRpc(NetworkObjectReference playerNetObjRef, int nuevoID)
        {
            if (IsServer) return; 

            if (playerNetObjRef.TryGet(out NetworkObject playerNetObj))
            {
                PlayerObjectAttackManager attackManager = playerNetObj.GetComponentInChildren<PlayerObjectAttackManager>();
                if (attackManager != null)
                {
                    attackManager.CambiarIDGolpeActivo(nuevoID);
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        private void OcultarObjetoEnClientesRpc()
        {
            DesaparecerObjeto();
        }

        public void DesaparecerObjeto()
        {
            if (_cronometroCoroutine != null) StopCoroutine(_cronometroCoroutine);

            if (_spriteRenderer != null) _spriteRenderer.enabled = false;
            if (_collider != null) _collider.enabled = false;

            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        public void RespawnearLocal()
        {
            transform.position = _posicionInicial;

            if (_spriteRenderer != null) _spriteRenderer.enabled = true;
            if (_collider != null) _collider.enabled = true;

            ConfigurarEstadoFlotandoInicial();
            IniciarCronometro();
        }

        public void ResetearVariableRecogidoServer()
        {
            if (IsServer) _yaRecogido.Value = false;
        }
    }
}
