using UnityEngine;

namespace GameplayScripts
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private Joystick _joystick;
        private bool _uiJumpPressed;
        private bool _uiPunchPressed;

        private void Awake()
        {
            _joystick = FindFirstObjectByType<Joystick>();
        }

        public float GetHorizontalInput()
        {
            float joystickX = _joystick.Direction.x;

            if (joystickX > 0.1f)  return 1f;
            if (joystickX < -0.1f) return -1f;
            
            return 0f;
        }

        public bool IsJumpPressedThisFrame()
        {
            if (_uiJumpPressed)
            {
                _uiJumpPressed = false;
                return true;
            }
            return false;
        }

        public bool IsPunchPressedThisFrame()
        {
            if (_uiPunchPressed)
            {
                _uiPunchPressed = false;
                return true;
            }
            return false;
        }

        public void TriggerUIJump()
        {
            _uiJumpPressed = true;
        }

        public void TriggerUIPunch()
        {
            _uiPunchPressed = true;
        }
    }
}