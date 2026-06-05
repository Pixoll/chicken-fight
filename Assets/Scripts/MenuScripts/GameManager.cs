using UnityEngine;

namespace MenuScripts
{
    public class GameManager : MonoBehaviour {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake() {
            Application.targetFrameRate = targetFrameRate;

            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;

            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;

            Screen.orientation = ScreenOrientation.AutoRotation;
        }
    }
}
