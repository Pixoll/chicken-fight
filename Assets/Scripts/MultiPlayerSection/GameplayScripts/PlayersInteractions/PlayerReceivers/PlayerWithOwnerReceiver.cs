using MultiPlayerSection.PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace MultiPlayerSection.GameplayScripts.PlayersInteractions.PlayerReceivers
{
    public class PlayerWithOwnerReceiver : NetworkBehaviour 
    {
        private PlayerIdentity _playerIdentity;

        private void Awake()
        {
            // Obtenemos la identidad de la gallina dueña de este script
            _playerIdentity = transform.root.GetComponent<PlayerIdentity>();
        }

        /// <summary>
        /// Invocado localmente en la pantalla donde se detectó el choque físico de la Hitbox.
        /// </summary>
        public void EnviarImpactoFisicoALaRed(
            float damage,
            float force, 
            HurtboxCharacteristics.InclinacionVertical inclinacion,
            HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun,
            Vector2 direccionDerechaEnemigo,
            Vector2 direccionArribaEnemigo,
            string nombreVictima)
        {
            // 🐔 Identificamos quién está ENVIANDO el golpe en esta pantalla local
            string miNombreAtacante = _playerIdentity != null ? _playerIdentity.NombreIdentificador : "Desconocido";

            Debug.Log($"<color=#00FFFF><b>[PASO 1 - PANTALLA LOCAL]</b></color>\n" +
                      $"🔹 <b>Atacante (Emisor):</b> {miNombreAtacante}\n" +
                      $"🎯 <b>Víctima (Objetivo):</b> {nombreVictima}\n" +
                      $"📋 <b>Detalles Hurtbox:</b> Daño: {damage} | Fuerza: {force} | Stun: {durationStun}s\n" +
                      $"📐 <b>Dirección H:</b> {direccion} | Inclinación V: {inclinacion}\n" +
                      $"🚀 Enviando petición al Servidor por ServerRpc...");

            // Enviamos los datos al Servidor para ver si la señal llega al backend
            SolicitarProcesarImpactoEnServidorServerRpc(
                damage, 
                force, 
                inclinacion, 
                direccion, 
                durationStun, 
                direccionDerechaEnemigo, 
                direccionArribaEnemigo,
                nombreVictima,
                miNombreAtacante
            );
        }

        [ServerRpc(RequireOwnership = false)]
        private void SolicitarProcesarImpactoEnServidorServerRpc(
            float damage,
            float force, 
            HurtboxCharacteristics.InclinacionVertical inclinacion,
            HurtboxCharacteristics.DireccionHorizontal direccion,
            float durationStun,
            Vector2 dirDerecha, 
            Vector2 dirArriba,
            string nombreVictima,
            string nombreAtacanteOriginal)
        {
            // 🖥️ --- ESTE BLOQUE SOLO SE EJECUTA EN EL HOST/SERVIDOR ---
            
            Debug.Log($"<color=#FFFF00><b>[PASO 2 - SEÑAL EN SERVIDOR]</b></color>\n" +
                      $"⚡ El Servidor recibió la orden correctamente.\n" +
                      $"⚔️ <b>Atacante reportado:</b> {nombreAtacanteOriginal}\n" +
                      $"🛡️ <b>Víctima reportada:</b> {nombreVictima}\n" +
                      $"📊 <b>Datos validados:</b> Daño a aplicar: {damage} HP | Fuerza: {force}\n" +
                      $"📢 Distribuyendo a todas las pantallas mediante RPC global...");

            // Replicamos el mensaje a todos los clientes para ver si la señal de vuelta es exitosa
            ProcesarFisicaDeGolpeEnClientesRpc(
                force, 
                inclinacion, 
                direccion, 
                durationStun, 
                dirDerecha, 
                dirArriba, 
                nombreVictima, 
                nombreAtacanteOriginal
            );
        }

        [Rpc(SendTo.Everyone)]
        private void ProcesarFisicaDeGolpeEnClientesRpc(
            float force, 
            HurtboxCharacteristics.InclinacionVertical inclinacion, 
            HurtboxCharacteristics.DireccionHorizontal direccion, 
            float durationStun, 
            Vector2 dirDerecha, 
            Vector2 dirArriba,
            string nombreVictima,
            string nombreAtacanteOriginal)
        {
            // 👥 --- ESTE BLOQUE SE EJECUTA EN TODAS LAS PANTALLAS (CLIENTES Y HOST) ---
            
            // Cada réplica de gallina en la escena evaluará este Log
            string miIdentidadLocal = _playerIdentity != null ? _playerIdentity.NombreIdentificador : "Desconocido";

            Debug.Log($"<color=#FF00FF><b>[PASO 3 - RECEPCIÓN GLOBAL RPC]</b></color>\n" +
                      $"🖥️ <b>Yo soy la entidad:</b> {miIdentidadLocal}\n" +
                      $"📣 <b>Anuncio de red:</b> {nombreAtacanteOriginal} golpeó a {nombreVictima}.\n" +
                      $"❓ <b>¿Me corresponde reaccionar?:</b> {(miIdentidadLocal == nombreVictima ? "<color=green><b>SÍ (Soy la Víctima)</b></color>" : "<color=red>No (Ignorar)</color>")}");
        }
    }
}
