using System;
using System.IO;
using DataPersistence.Data;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Multiplayer.PlayMode;
#endif

namespace DataPersistence {
    public class FileDataHandler {
        private const string EncryptionCodeWord = "and the universe said i love you because you are love";

        private readonly string _dataFilePath;
        private readonly bool _useEncryption;

        public FileDataHandler(string dataFilePath, bool useEncryption) {
#if UNITY_EDITOR
            // only in editor, override file name by appending the tag assigned to the virtual player
            // this prevents data races for the same save file
            IReadOnlyList<string> tags = CurrentPlayer.Tags;

            if (tags.Count > 0) {
                string dir = Path.GetDirectoryName(dataFilePath);
                string name = Path.GetFileNameWithoutExtension(dataFilePath);
                string ext = Path.GetExtension(dataFilePath);
                string newName = $"{name}_{tags[0]}{ext}";

                Debug.Log(
                    $"<color=cyan>[FileDataHandler] Overriding save file name from '{name}' to '{newName}'</color>"
                );

                dataFilePath = Path.Combine(dir ?? "", $"{name}_{tags[0]}{ext}");
            }
#endif

            _dataFilePath = dataFilePath;
            _useEncryption = useEncryption;
        }

        public GameData Load() {
            var data = new GameData();

            if (!File.Exists(_dataFilePath)) {
                Debug.Log("<color=cyan>[FileDataHandler] No data found, creating new data</color>");
                return data;
            }

            try {
                using var stream = new FileStream(_dataFilePath, FileMode.Open);
                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();

                if (_useEncryption && IsEncrypted(json)) {
                    json = EncryptDecrypt(json);
                }

                data = JsonUtility.FromJson<GameData>(json);
                data.ApplyDefaults();
            } catch (Exception e) {
                Debug.LogError(e);
            }

            return data;
        }

        public bool Save(GameData data) {
            try {
                string json = JsonUtility.ToJson(data, false);

                if (_useEncryption) {
                    json = EncryptDecrypt(json);
                }

                using var stream = new FileStream(_dataFilePath, FileMode.Create);
                using var writer = new StreamWriter(stream);
                writer.Write(json);
                return true;
            } catch (Exception e) {
                Debug.LogError(e);
                return false;
            }
        }

        private static bool IsEncrypted(string data) {
            return data.Length > 0 && data[0] != '{';
        }

        private static string EncryptDecrypt(string data) {
            string newData = "";

            for (int i = 0; i < data.Length; i++) {
                newData += (char)(data[i] ^ EncryptionCodeWord[i % EncryptionCodeWord.Length]);
            }

            return newData;
        }
    }
}
