using UnityEngine;

namespace KartGame.KartSystems
{
    public class GamepadInput : BaseInput
    {
        [Tooltip("Joystick index (0 for first gamepad, 1 for second gamepad, etc.)")]
        public int JoystickNumber = 0;

        [Tooltip("Axis name for horizontal input (typically 'Horizontal' or 'Xbox One X Axis Left Stick')")]
        public string HorizontalAxisName = "Horizontal";

        [Tooltip("Button name for acceleration")]
        public string AccelerateButtonName = "Accelerate";

        [Tooltip("Button name for braking")]
        public string BrakeButtonName = "Brake";

        [Tooltip("Button name for nitro boost")]
        public string NitroButtonName = "Nitro";

        private string[] joystickNames;

        private void Start()
        {
            joystickNames = Input.GetJoystickNames();
            
            if (JoystickNumber >= joystickNames.Length)
            {
                Debug.LogWarning($"Gamepad {JoystickNumber} not found. Available gamepads: {joystickNames.Length}");
            }
        }

        public override InputData GenerateInput()
        {
            // Get axis input using joystick index
            string horizontalAxisName = $"Joystick{JoystickNumber + 1}{HorizontalAxisName}";
            
            float turnInput = 0f;
            try
            {
                turnInput = Input.GetAxis(horizontalAxisName);
            }
            catch
            {
                // Fallback to regular horizontal if joystick-specific axis doesn't exist
                turnInput = Input.GetAxis(HorizontalAxisName);
            }

            return new InputData
            {
                Accelerate = GetGamepadButton(AccelerateButtonName),
                Brake = GetGamepadButton(BrakeButtonName),
                TurnInput = turnInput,
                Nitro = GetGamepadButton(NitroButtonName)
            };
        }

        private bool GetGamepadButton(string buttonName)
        {
            try
            {
                string joystickButtonName = $"Joystick{JoystickNumber + 1}{buttonName}";
                return Input.GetButton(joystickButtonName);
            }
            catch
            {
                // Fallback to regular button if joystick-specific button doesn't exist
                return Input.GetButton(buttonName);
            }
        }
    }
}
