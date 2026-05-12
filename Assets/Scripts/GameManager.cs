using UnityEngine;

public class GameManager : MonoBehaviour {
    private void Awake() {
        // First, allow the screen to auto-rotate between Left and Right landscape
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        // Strictly forbid portrait modes
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // Apply the auto-rotation setting (which will default to Landscape Left)
        Screen.orientation = ScreenOrientation.AutoRotation;

        // If you NEVER want it to flip when they turn their phone, you can delete 
        // the code above and just use this one line instead:
        // Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
}
