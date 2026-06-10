using UnityEngine;

namespace GameplayScripts {
    public class PlayerInputHandler : MonoBehaviour {
        private Joystick _joystick;
        private bool _uiJumpPressed;
        private bool _uiPunchPressed;

        private void Awake() {
            _joystick = FindFirstObjectByType<Joystick>();
        }

        public float GetHorizontalInput() {
            float joystickX = _joystick.Direction.x;

            return joystickX switch {
                > 0.1f => 1f,
                < -0.1f => -1f,
                var _ => 0f
            };
        }

        public bool IsJumpPressedThisFrame() {
            if (!_uiJumpPressed) return false;

            _uiJumpPressed = false;
            return true;

        }

        public bool IsPunchPressedThisFrame() {
            if (!_uiPunchPressed) return false;

            _uiPunchPressed = false;
            return true;

        }

        public void TriggerUIJump() {
            _uiJumpPressed = true;
        }

        public void TriggerUIPunch() {
            _uiPunchPressed = true;
        }
    }
}
