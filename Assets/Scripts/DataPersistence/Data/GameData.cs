using System;
using System.Collections.Generic;

namespace DataPersistence.Data {
    [Serializable]
    public class GameData {
        public string username;
        public List<Game> games;

        public GameData() {
            ApplyDefaults();
        }

        public void ApplyDefaults() {
            username ??= "Player";
            games ??= new List<Game>();
        }
    }
}
