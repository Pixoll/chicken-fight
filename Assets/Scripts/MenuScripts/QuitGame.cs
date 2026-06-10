using UnityEngine;

namespace MenuScripts {
    public class QuitButton : MonoBehaviour {
        public void QuitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
