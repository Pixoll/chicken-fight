using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour {
    public static GameplayUI Instance { get; private set; }

    [Header("Elementos de la UI (Asignar en Inspector)")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private TMP_Text enemyHealthText;
    
    [Header("Pantalla de Victoria/Fin")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text victoryMessageText;
    [SerializeField] private Button mainMenubutton;

    private void Awake() {
        if (Instance == null) Instance = this;
        victoryPanel?.SetActive(false);
        mainMenubutton?.onClick.AddListener(ReturnToMenu);
    }

    private void Update() {
        if (GameplayManager.Instance == null) return;

        // 1. Actualizar reloj en pantalla
        float time = GameplayManager.Instance.timeRemaining.Value;
        timerText.text = $"Tiempo: {Mathf.Max(0, Mathf.CeilToInt(time))}s";

        // 2. Buscar dinámicamente tu vida y la del rival/maniquí en la escena
        HitReceiver[] entities = FindObjectsByType<HitReceiver>(FindObjectsSortMode.None);
        
        foreach (var entity in entities) {
            if (entity.IsOwner) {
                // Eres tú
                playerHealthText.text = $"Tu Vida: {entity.currentHealth.Value}";
            } else {
                // Es el maniquí o el rival online
                enemyHealthText.text = $"Rival: {entity.currentHealth.Value}";
            }
        }
    }

    public void ShowVictoryScreen(string text) {
        if (victoryPanel != null) {
            victoryPanel.SetActive(true);
            victoryMessageText.text = text;
        }
    }

    private void ReturnToMenu() {
        // Cerramos la conexión de red limpiamente según el rol
        if (NetworkManager.Singleton != null) {
            NetworkManager.Singleton.Shutdown();
        }
        // Recargamos la escena para volver al menú principal
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}