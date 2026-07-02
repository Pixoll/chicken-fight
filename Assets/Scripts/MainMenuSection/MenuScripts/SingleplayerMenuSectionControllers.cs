using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenuSection.MenuScripts {
    public class SingleplayerControllers : MonoBehaviour {
        public void LaunchTrainMode() {
            SceneManager.LoadScene("TrainMode");
        }
    }
}
