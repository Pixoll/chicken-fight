using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace MultiplayerScripts.GlobalGameState
{
    public class GameTimeSection : NetworkBehaviour
    {
        private enum FasePartida 
        { 
            ConteoInicial, 
            Peleando, 
            Ultimos10Segundos, 
            FinPartida 
        }

        [SerializeField] private float tiempoMaximoPartida = 99f;
        [SerializeField] private float duracionConteoInicial = 3f;
        
        [SerializeField] private GameObject countPanel;
        [SerializeField] private GameObject timePanel;
        [SerializeField] private GameObject warningPanel;
        [SerializeField] private GameObject endPanel;
        
        [SerializeField] private TMP_Text textoReloj;
        [SerializeField] private TMP_Text textoConteo;

        private NetworkVariable<float> _tiempoRestante = new NetworkVariable<float>(99f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<FasePartida> _faseActual = new NetworkVariable<FasePartida>(FasePartida.ConteoInicial, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        
        private bool _partidaActiva = false;
        private float _cronometroConteo;

        public override void OnNetworkSpawn()
        {
            _tiempoRestante.OnValueChanged += ActualizarRelojUI;
            _faseActual.OnValueChanged += OnFasePartidaChanged;

            if (countPanel != null) countPanel.SetActive(true);
            if (timePanel != null) timePanel.SetActive(true);
            if (warningPanel != null) warningPanel.SetActive(false);
            if (endPanel != null) endPanel.SetActive(false);
        }

        public void IniciarCronometroMaestro()
        {
            if (IsServer == false) return;

            _tiempoRestante.Value = tiempoMaximoPartida;
            _faseActual.Value = FasePartida.ConteoInicial;
            
            if (duracionConteoInicial <= 0f)
            {
                _cronometroConteo = 3f;
            }
            else
            {
                _cronometroConteo = duracionConteoInicial;
            }
            
            _partidaActiva = true;
        }

        private void Update()
        {
            if (IsServer == false || _partidaActiva == false) return;

            if (_faseActual.Value == FasePartida.ConteoInicial)
            {
                _cronometroConteo -= Time.deltaTime;
                int segundos = Mathf.CeilToInt(_cronometroConteo);
                
                if (segundos > 0) 
                {
                    ActualizarTextoConteoRpc(segundos.ToString());
                }
                else 
                {
                    _faseActual.Value = FasePartida.Peleando;
                }
            }
            else
            {
                if (_tiempoRestante.Value > 0f)
                {
                    _tiempoRestante.Value -= Time.deltaTime;
                    
                    if (_tiempoRestante.Value <= 10f && _faseActual.Value == FasePartida.Peleando) 
                    {
                        _faseActual.Value = FasePartida.Ultimos10Segundos;
                    }
                }
                else
                {
                    _tiempoRestante.Value = 0f;
                    _faseActual.Value = FasePartida.FinPartida;
                    _partidaActiva = false;
                }
            }
        }

        private void OnFasePartidaChanged(FasePartida faseAnterior, FasePartida faseNueva)
        {
            if (faseNueva == FasePartida.Peleando)
            {
                if (textoConteo != null) textoConteo.text = "¡PELEEN!";
                Invoke("KillCountPanel", 1.2f);
            }
            else if (faseNueva == FasePartida.Ultimos10Segundos)
            {
                if (warningPanel != null) warningPanel.SetActive(true);
            }
            else if (faseNueva == FasePartida.FinPartida)
            {
                if (timePanel != null) timePanel.SetActive(false);
                if (warningPanel != null) warningPanel.SetActive(false);
                if (endPanel != null) endPanel.SetActive(true);
            }
        }

        private void ActualizarRelojUI(float valorAnterior, float valorNuevo)
        {
            if (textoReloj != null) 
            {
                textoReloj.text = Mathf.CeilToInt(valorNuevo).ToString();
            }
        }

        [Rpc(SendTo.Everyone)]
        private void ActualizarTextoConteoRpc(string texto) 
        { 
            if (textoConteo != null && _faseActual.Value == FasePartida.ConteoInicial) 
            {
                textoConteo.text = texto; 
            }
        }

        private void KillCountPanel() 
        { 
            if (countPanel != null) 
            {
                Destroy(countPanel); 
            }
        }

        public override void OnNetworkDespawn() 
        {
            _tiempoRestante.OnValueChanged -= ActualizarRelojUI;
            _faseActual.OnValueChanged -= OnFasePartidaChanged;
        }
    }
}