using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MultiplayerScripts.GlobalGameState
{
    public class GameTimeSection : NetworkBehaviour
    {
        [Header("Configuración General")]
        [SerializeField] private float tiempoMaximoPartida = 99f;

        [Header("Línea de Tiempo Dinámica")]
        [SerializeField] private List<TimeElement> lineaDeTiempo = new List<TimeElement>();

        private Dictionary<int, GameObject> _instanciasClonadas = new Dictionary<int, GameObject>();

        private NetworkVariable<float> _tiempoRestante = new NetworkVariable<float>(99f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private bool _partidaActiva = false;

        public override void OnNetworkSpawn()
        {
        }

        public void IniciarCronometroMaestro()
        {
            if (IsServer == false) return;

            _tiempoRestante.Value = tiempoMaximoPartida;
            _partidaActiva = true;
        }

        private void Update()
        {
            if (IsServer == false || _partidaActiva == false) return;

            if (_tiempoRestante.Value > 0f)
            {
                _tiempoRestante.Value -= Time.deltaTime;
                ProcesarEventosDeTiempoServer(_tiempoRestante.Value);
            }
            else
            {
                _tiempoRestante.Value = 0f;
                _partidaActiva = false;
            }
        }

        private void ProcesarEventosDeTiempoServer(float segundoActual)
        {
            for (int i = 0; i < lineaDeTiempo.Count; i++)
            {
                TimeElement elemento = lineaDeTiempo[i];
                if (elemento.objetoVisualoFisico == null) continue;

                if (segundoActual <= elemento.segundoAparicion && elemento.yaAparecio == false && elemento.yaTermino == false)
                {
                    elemento.yaAparecio = true;
                    GestionarEstadoObjetoRpc(i, true, false);
                }

                if (segundoActual <= elemento.segundoDesaparicion && elemento.yaAparecio == true && elemento.yaTermino == false)
                {
                    elemento.yaTermino = true;
                    bool destruir = elemento.queHacerAlTerminar == TimeElement.AccionFinal.DestruirObjeto;
                    GestionarEstadoObjetoRpc(i, false, destruir);
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        private void GestionarEstadoObjetoRpc(int indiceElemento, bool activar, bool destruir)
        {
            if (indiceElemento < 0 || indiceElemento >= lineaDeTiempo.Count) return;

            TimeElement elemento = lineaDeTiempo[indiceElemento];
            if (elemento.objetoVisualoFisico == null) return;

            if (activar == true)
            {
                GameObject clon = Instantiate(elemento.objetoVisualoFisico);
                
                _instanciasClonadas[indiceElemento] = clon;

                if (elemento.puntoDeSpawneo != null)
                {
                    clon.transform.SetParent(elemento.puntoDeSpawneo, false);
                    clon.transform.position = elemento.puntoDeSpawneo.position;
                }
            }
            else
            {
                if (_instanciasClonadas.TryGetValue(indiceElemento, out GameObject clonAsociado))
                {
                    if (clonAsociado == null) return;

                    if (destruir == true)
                    {
                        Destroy(clonAsociado);
                    }
                    else
                    {
                        clonAsociado.transform.SetParent(null);
                    }

                    _instanciasClonadas.Remove(indiceElemento);
                }
            }
        }
        
        public float TiempoRestante => _tiempoRestante.Value;
    }
}
