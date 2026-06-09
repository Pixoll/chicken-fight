using MultiplayerScripts;
using UnityEngine;

public class TrainModeSceneController : MonoBehaviour
{
    private void Start()
    {
        if (GameplayNetworkManager.Instance != null)
        {
            GameplayNetworkManager.Instance.CreateHost();
                
            // Opcional: Debug para verificar la IP en consola
            string ip = GameplayNetworkManager.Instance.GetServerAddress();
            Debug.Log($"[TrainScene] Servidor corriendo internamente en la dirección: {ip}");
        }
    }
}