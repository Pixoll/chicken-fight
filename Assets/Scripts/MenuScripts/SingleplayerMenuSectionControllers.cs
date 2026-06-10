using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuScripts {
    public class SingleplayerControllers : MonoBehaviour {
        public void LaunchTrainMode() {
            SceneManager.LoadScene("TrainMode");
        }
    }
}
