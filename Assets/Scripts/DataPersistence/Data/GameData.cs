using System;
using System.Collections.Generic;

namespace DataPersistence.Data {
    [Serializable]
    public class GameData {
        private const string DefaultUsername = "Player";

        public string username;
        public List<Game> games;

        public GameData() {
            ApplyDefaults();
        }

        public bool HasDefaultUsername => username == DefaultUsername;

        public void ApplyDefaults() {
            username ??= DefaultUsername;
            games ??= new List<Game>();
        }
    }
}
