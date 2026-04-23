using UnityEngine;

namespace KartGame.KartSystems
{
    /// <summary>
    /// Alternative keyboard input for second player using WASD controls
    /// Player 1: Arrow Keys (KeyboardInput.cs)
    /// Player 2: WASD + Space/C (This script)
    /// </summary>
    public class AlternateKeyboardInput : BaseInput
    {
        [Header("Keyboard Bindings")]
        [Tooltip("Key for accelerating forward (default: W)")]
        public KeyCode AccelerateKey = KeyCode.W;
        
        [Tooltip("Key for braking/reversing (default: S)")]
        public KeyCode BrakeKey = KeyCode.S;
        
        [Tooltip("Key for turning left (default: A)")]
        public KeyCode LeftTurnKey = KeyCode.A;
        
        [Tooltip("Key for turning right (default: D)")]
        public KeyCode RightTurnKey = KeyCode.D;
        
        [Tooltip("Key for nitro boost (default: Space)")]
        public KeyCode NitroKey = KeyCode.LeftShift;

        public override InputData GenerateInput()
        {
            float turnInput = 0f;
            
            // Handle turn input
            if (Input.GetKey(LeftTurnKey))
                turnInput -= 1f;
            if (Input.GetKey(RightTurnKey))
                turnInput += 1f;

            return new InputData
            {
                Accelerate = Input.GetKey(AccelerateKey),
                Brake = Input.GetKey(BrakeKey),
                TurnInput = turnInput,
                Nitro = Input.GetKey(NitroKey)
            };
        }
    }
}
