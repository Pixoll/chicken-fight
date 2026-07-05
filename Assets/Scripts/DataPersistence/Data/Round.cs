using System;

namespace DataPersistence.Data {
    [Serializable]
    public class Round {
        public int sceneId;
        public bool won;
        public bool killedOpponent;
        public float timeTaken;
        public int damageDealt;
        public int damageTaken;
        public int hitsDealt;
        public int hitsTaken;
    }
}
