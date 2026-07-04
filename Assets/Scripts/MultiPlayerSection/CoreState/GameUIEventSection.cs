using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;

namespace MultiPlayerSection.CoreState
{
    public class GameUIEventSection : NetworkBehaviour
    {
        [Header("Contenedor de UI Principal")]
        [SerializeField] private Transform canvasRaizDeLaPartida;

        [Header("Configuración Cortina de Carga")]
        [SerializeField] private GameObject prefabCortinaCarga;
        [SerializeField] private float duracionCortinaCarga = 3f;
        private GameObject _instanciaCortinaActual;

        [Header("Configuración Aviso Inicio Round")]
        [SerializeField] private GameObject prefabAvisoInicioRound;
        [SerializeField] private float duracionAvisoInicioRound = 2f;
        private GameObject _instanciaAvisoInicioActual;

        [Header("Configuración Aviso Fin Round")]
        [SerializeField] private GameObject prefabAvisoFinRound;
        [SerializeField] private float duracionAvisoFinRound = 2f;
        private GameObject _instanciaAvisoFinActual;

        public float DuracionCortinaCarga => duracionCortinaCarga;

        public void MostrarCortinaCarga()
        {
            if (!IsServer) return;
            SolicitarCortinaCargaRpc();
        }

        public void MostrarAvisoInicioRound(string textoRound)
        {
            if (!IsServer) return;
            SolicitarAvisoInicioRoundRpc(textoRound);
        }

        public void MostrarAvisoFinRound(string textoGanador)
        {
            if (!IsServer) return;
            SolicitarAvisoFinRoundRpc(textoGanador);
        }
        
        [Rpc(SendTo.Everyone)]
        private void SolicitarCortinaCargaRpc()
        {
            if (canvasRaizDeLaPartida == null || prefabCortinaCarga == null) return;

            if (_instanciaCortinaActual != null) Destroy(_instanciaCortinaActual);

            _instanciaCortinaActual = Instantiate(prefabCortinaCarga, canvasRaizDeLaPartida);
            Destroy(_instanciaCortinaActual, duracionCortinaCarga);
        }

        [Rpc(SendTo.Everyone)]
        private void SolicitarAvisoInicioRoundRpc(string textoRound)
        {
            if (canvasRaizDeLaPartida == null || prefabAvisoInicioRound == null) return;

            if (_instanciaAvisoInicioActual != null) Destroy(_instanciaAvisoInicioActual);

            _instanciaAvisoInicioActual = Instantiate(prefabAvisoInicioRound, canvasRaizDeLaPartida);
            
            var tmp = _instanciaAvisoInicioActual.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = textoRound;

            Destroy(_instanciaAvisoInicioActual, duracionAvisoInicioRound);
        }

        [Rpc(SendTo.Everyone)]
        private void SolicitarAvisoFinRoundRpc(string textoGanador)
        {
            if (canvasRaizDeLaPartida == null || prefabAvisoFinRound == null) return;

            if (_instanciaAvisoFinActual != null) Destroy(_instanciaAvisoFinActual);

            _instanciaAvisoFinActual = Instantiate(prefabAvisoFinRound, canvasRaizDeLaPartida);

            var tmp = _instanciaAvisoFinActual.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = textoGanador;

            Destroy(_instanciaAvisoFinActual, duracionAvisoFinRound);
        }
    }
}
