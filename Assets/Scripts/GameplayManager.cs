using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameplayManager : NetworkBehaviour {
    public static GameplayManager Instance { get; private set; }

    [Header("Configuración de la Partida")] [SerializeField]
    private float matchDuration = 60f; // 1 minuto

    // Variables de red optimizadas para que todos los clientes lean el tiempo
    public NetworkVariable<float> timeRemaining = new(60f);
    public NetworkVariable<bool> isMatchOver = new();

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn() {
        if (IsServer) {
            timeRemaining.Value = matchDuration;
            isMatchOver.Value = false;
            StartCoroutine(MatchTimerRoutine());
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator MatchTimerRoutine() {
        while (timeRemaining.Value > 0 && !isMatchOver.Value) {
            yield return new WaitForSeconds(1f);

            timeRemaining.Value -= 1f;
        }

        if (timeRemaining.Value <= 0 && !isMatchOver.Value) {
            EndMatch("¡TIEMPO AGOTADO!");
        }
    }

    // El servidor evalúa de forma estricta las condiciones de victoria
    public void CheckWinConditions() {
        if (!IsServer || isMatchOver.Value) return;

        // Buscamos a todos los jugadores en la partida
        HitReceiver[] players = FindObjectsByType<HitReceiver>(FindObjectsSortMode.None);

        HitReceiver playerDead = null;
        HitReceiver playerAlive = null;

        foreach (var player in players) {
            if (player.currentHealth.Value <= 0) {
                playerDead = player;
            } else {
                playerAlive = player;
            }
        }

        // Si alguien se quedó sin vida, termina el juego
        if (playerDead != null) {
            string victoryMessage = "¡PARTIDA TERMINADA!";

            if (players.Length > 1 && playerAlive != null) {
                victoryMessage = $"¡JUGADOR {playerAlive.OwnerClientId + 1} GANA!";
            }

            EndMatch(victoryMessage);
        }
    }

    private void EndMatch(string message) {
        isMatchOver.Value = true;
        Debug.Log($"[GAME OVER] {message}");

        // Aquí notificamos a la interfaz de todos los clientes mediante un RPC
        NotifyMatchEndClientRpc(message);
    }

    [Rpc(SendTo.Everyone)]
    private void NotifyMatchEndClientRpc(string endMessage) {
        GameplayUI.Instance?.ShowVictoryScreen(endMessage);
    }
}
