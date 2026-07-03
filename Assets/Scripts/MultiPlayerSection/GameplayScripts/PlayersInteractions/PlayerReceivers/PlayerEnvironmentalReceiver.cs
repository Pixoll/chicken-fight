using MultiPlayerSection.NetworkScripts;
using MultiPlayerSection.PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions.PlayerReceivers
{
    public class PlayerEnvironmentalReceiver : NetworkBehaviour
    {
        private Rigidbody2D _rb;
        private PlayerMovement _playerMovement;
        private MatchInformationManager _matchManager;

        private void Awake()
        {
            Transform root = transform.root;
            _rb = root.GetComponent<Rigidbody2D>();
            _playerMovement = root.GetComponentInChildren<PlayerMovement>();
        }

        private void Start()
        {
            _matchManager = FindFirstObjectByType<MatchInformationManager>();
        }
        
        public void EnviarImpactoAmbientalALaRed(
            float damage,
            float force,
            HurtboxCharacteristics.InclinacionVertical inclinacion,
            HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun,
            Vector2 direccionDerechaEntorno,
            Vector2 direccionArribaEntorno,
            string nombreAfectado)
        {
            string miInstanciaDePantallaID = NetworkManager.Singleton.LocalClientId.ToString();

            if (miInstanciaDePantallaID != nombreAfectado)
            {
                return;
            }

            Debug.Log($"<color=orange>[EnvironmentalReceiver] -> ¡Mi gallina ({miInstanciaDePantallaID}) pisó el entorno! Aplicando consecuencias locales e informando al servidor...</color>");

            AplicarAturdimientoLocal(durationStun);
            AplicarFuerzaDeEmpujeLocal(force, inclinacion, direccion, direccionDerechaEntorno, direccionArribaEntorno);

            NotificarDañoAmbientalAlServidorServerRpc(damage, nombreAfectado);
        }

        [ServerRpc(RequireOwnership = false)]
        private void NotificarDañoAmbientalAlServidorServerRpc(float damage, string nombreVictima)
        {
            if (_matchManager != null)
            {
                _matchManager.ModificarVidaJugador(nombreVictima, -damage);
            }
        }

        private void AplicarAturdimientoLocal(float duracion)
        {
            if (duracion <= 0f || _playerMovement == null) return;
            _playerMovement.StunningTime(duracion);
            Debug.Log($"<color=yellow>[STUN AMBIENTAL] -> Joystick inhabilitado por {duracion}s.</color>");
        }

        private void AplicarFuerzaDeEmpujeLocal(
            float fuerza, 
            HurtboxCharacteristics.InclinacionVertical inclinacion, 
            HurtboxCharacteristics.DireccionHorizontal direccion, 
            Vector2 dirDerecha, 
            Vector2 dirArriba)
        {
            if (_rb == null) return;

            Vector2 vectorResultado = Vector2.zero;

            switch (direccion)
            {
                case HurtboxCharacteristics.DireccionHorizontal.Forward:   vectorResultado = dirDerecha; break;
                case HurtboxCharacteristics.DireccionHorizontal.Backward:  vectorResultado = -dirDerecha; break;
                case HurtboxCharacteristics.DireccionHorizontal.Up:        vectorResultado = dirArriba; break;
                case HurtboxCharacteristics.DireccionHorizontal.Down:      vectorResultado = -dirArriba; break;
            }

            if (inclinacion == HurtboxCharacteristics.InclinacionVertical.Top) vectorResultado += Vector2.up;
            else if (inclinacion == HurtboxCharacteristics.InclinacionVertical.Bottom) vectorResultado += Vector2.down;

            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(vectorResultado.normalized * fuerza, ForceMode2D.Impulse);
            Debug.Log($"<color=magenta>[FÍSICA AMBIENTAL] -> Empujando gallina con fuerza: {fuerza} | Dirección: {vectorResultado.normalized}</color>");
        }
    }
}
