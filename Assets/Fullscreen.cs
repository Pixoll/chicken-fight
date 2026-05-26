#if UNITY_EDITOR

using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class FullscreenHotkeyHandler : MonoBehaviour {
    bool makeFullscreenAtStart = false;

    // Enable fullscreen when starting game
    private void Start() {
        if (makeFullscreenAtStart) {
            FullscreenGameView.Toggle();
        }
    }

    private void Update() {
        // Toggle fullscreen when hotkey pressed
        if (Keyboard.current != null && Keyboard.current.f11Key.wasPressedThisFrame) {
            FullscreenGameView.Toggle();
        }
    }
}

// Below code from: https://gist.github.com/fnuecke/d4275087cc7969257eae0f939fac3d2f
// My Improvement: Fixed bug where stuck in fullscreen after re-compiling
public static class FullscreenGameView {
    private static readonly Type GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");

    private static readonly PropertyInfo ShowToolbarProperty = GameViewType.GetProperty(
        "showToolbar",
        BindingFlags.Instance | BindingFlags.NonPublic
    );

    private static readonly object False = false; // Only box once. This is a matter of principle.

    private static EditorWindow _instance;

    // Exit fullscreen when re-compiling game during Game session (to fix bug where can't leave fullscreen)
    static FullscreenGameView() {
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
    }

    private static void OnBeforeAssemblyReload() {
        if (_instance == null) return;
        _instance.Close();
        _instance = null;
    }

    [MenuItem("Window/General/Game (Fullscreen) %#&2", priority = 2)]
    public static void Toggle() {
        if (GameViewType == null) {
            Debug.LogError("GameView type not found.");
            return;
        }

        if (ShowToolbarProperty == null) {
            Debug.LogWarning("GameView.showToolbar property not found.");
        }

        if (_instance != null) {
            _instance.Close();
            _instance = null;
            return;
        }

        _instance = (EditorWindow)ScriptableObject.CreateInstance(GameViewType);

        ShowToolbarProperty?.SetValue(_instance, False);

        var desktopResolution = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        var fullscreenRect = new Rect(Vector2.zero, desktopResolution);
        _instance.ShowPopup();
        _instance.position = fullscreenRect;
        _instance.Focus();
    }
}

#endif