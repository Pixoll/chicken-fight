using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.Efects
{
    public class ControlIndicadorLocal : NetworkBehaviour
    {
        private Vector3 _posicionRelativaOriginal;
        private bool _inicializado = false;

        private void Awake()
        {
            _posicionRelativaOriginal = transform.localPosition;
        }

        public override void OnNetworkSpawn()
        {
            _inicializado = true;
            ActualizarPosicionIndicador();
        }

        private void OnEnable()
        {
            if (_inicializado)
            {
                ActualizarPosicionIndicador();
            }
        }

        private void LateUpdate()
        {
            if (_inicializado)
            {
                ActualizarPosicionIndicador();
            }
        }

        private void ActualizarPosicionIndicador()
        {
            NetworkObject netObjPadre = transform.root.GetComponent<NetworkObject>();

            if (netObjPadre != null)
            {
                if (netObjPadre.IsOwner)
                {
                    transform.localPosition = _posicionRelativaOriginal;
                }
                else
                {
                    transform.localPosition = new Vector3(_posicionRelativaOriginal.x, _posicionRelativaOriginal.y, -5000f);
                }
            }
        }
    }
}