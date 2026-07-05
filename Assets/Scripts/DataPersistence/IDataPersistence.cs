using DataPersistence.Data;

namespace DataPersistence {
    public interface IDataPersistence {
        void LoadData(GameData gameData);

        void SaveData(ref GameData gameData);
    }
}
