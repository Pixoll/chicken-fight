using System;
using System.Collections.Generic;

namespace DataPersistence.Data {
    [Serializable]
    public class Game {
        public string opponentUsername;
        public List<Round> rounds;
    }
}
