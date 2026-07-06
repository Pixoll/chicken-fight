using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataPersistence.Data;
using UnityEngine;

namespace DataPersistence {
    public class DataPersistenceManager : MonoBehaviour {
        public static DataPersistenceManager Instance { get; private set; }

        [SerializeField] private string fileName;
        [SerializeField] private bool useEncryption;

        private FileDataHandler _fileDataHandler;
        private GameData _gameData;
        private List<IDataPersistence> _dataPersistenceObjects;

        public string PlayerUsername => _gameData.username;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            _fileDataHandler = new FileDataHandler(
                Path.Combine(Application.persistentDataPath, fileName),
                useEncryption
            );

            _dataPersistenceObjects = FindAllDataPersistenceObjects();

            LoadGame();
        }

        private void OnApplicationQuit() {
            SaveGame();
        }

        public void LoadGame() {
            Debug.Log("<color=cyan>[DataPersistenceManager] Loading game data</color>");
            _gameData = _fileDataHandler.Load();

            foreach (IDataPersistence dataPersistenceObject in _dataPersistenceObjects) {
                dataPersistenceObject.LoadData(_gameData);
            }
        }

        public void SaveGame() {
            Debug.Log("<color=cyan>[DataPersistenceManager] Saving game data</color>");
            _gameData ??= _fileDataHandler.Load();

            foreach (IDataPersistence dataPersistenceObject in _dataPersistenceObjects) {
                dataPersistenceObject.SaveData(ref _gameData);
            }

            _fileDataHandler.Save(_gameData);
        }

        private static List<IDataPersistence> FindAllDataPersistenceObjects() {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                // ReSharper disable once SuspiciousTypeConversion.Global
                .OfType<IDataPersistence>().ToList();
        }
    }
}
