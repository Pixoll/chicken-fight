using DataPersistence;
using DataPersistence.Data;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MainMenuSection.Core {
    public class LobbyPlayerNamesSync : NetworkBehaviour, IDataPersistence {
        [Header("Player 1")] [SerializeField] private TMP_Text player1NameText;
        [SerializeField] private TMP_InputField player1NameInput;

        [Header("Player 2")] [SerializeField] private TMP_Text player2NameText;
        [SerializeField] private TMP_InputField player2NameInput;

        private string _localUsername;

        private readonly NetworkVariable<FixedString32Bytes> _player1Name = new(
            "Player 1",
            writePerm: NetworkVariableWritePermission.Server
        );

        private readonly NetworkVariable<FixedString32Bytes> _player2Name = new(
            "Player 2",
            writePerm: NetworkVariableWritePermission.Server
        );

        public override void OnNetworkSpawn() {
            _player1Name.OnValueChanged += (_, newValue) => {
                string value = newValue.ToString();
                player1NameText.text = value;
                player1NameInput.text = value;
                GameplayNetworkManager.Instance.player1Username = value;
            };

            _player2Name.OnValueChanged += (_, newValue) => {
                string value = newValue.ToString();
                player2NameText.text = value;
                player2NameInput.text = value;
                GameplayNetworkManager.Instance.player2Username = value;
            };

            string p1Name = _player1Name.Value.ToString();
            string p2Name = _player2Name.Value.ToString();

            player1NameText.text = p1Name;
            player2NameText.text = p2Name;
            player1NameInput.text = p1Name;
            player2NameInput.text = p2Name;

            if (IsClient && !string.IsNullOrEmpty(_localUsername)) {
                SubmitUsernameServerRpc(_localUsername, NetworkManager.Singleton.LocalClientId);
            }
        }

        public void SavePlayer1Name() {
            string newUsername = player1NameInput.text;

            if (string.IsNullOrEmpty(newUsername) || _player1Name.Value == newUsername) {
                player1NameInput.text = _localUsername; 
                return;
            }

            _localUsername = newUsername;
            DataPersistenceManager.Instance.SaveGame();

            SubmitUsernameServerRpc(_localUsername, NetworkManager.Singleton.LocalClientId);
        }

        public void SavePlayer2Name() {
            string newUsername = player2NameInput.text;

            if (string.IsNullOrEmpty(newUsername) || _player2Name.Value == newUsername) {
                player2NameInput.text = _localUsername; 
                return;
            }

            _localUsername = newUsername;
            DataPersistenceManager.Instance.SaveGame();

            SubmitUsernameServerRpc(_localUsername, NetworkManager.Singleton.LocalClientId);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SubmitUsernameServerRpc(string username, ulong senderId) {
            if (senderId == NetworkManager.ServerClientId) {
                _player1Name.Value = username;
            } else {
                _player2Name.Value = username;
            }
        }

        public void LoadData(GameData gameData) {
            if (gameData.HasDefaultUsername || gameData.username == _localUsername) return;

            _localUsername = gameData.username;

            if (IsClient) {
                SubmitUsernameServerRpc(_localUsername, NetworkManager.Singleton.LocalClientId);
            }
        }

        public void SaveData(ref GameData gameData) {
            if (!string.IsNullOrEmpty(_localUsername)) {
                gameData.username = _localUsername;
            }
        }
    }
}
