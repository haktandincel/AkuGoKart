using UnityEngine;
using KartGame.KartSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KartGame.Utilities
{
    /// <summary>
    /// Helper script to quickly duplicate a kart for two-player mode setup
    /// Attach this to a GameObject and run DuplicateKartForPlayer2() from the editor
    /// </summary>
    public class TwoPlayerSetupHelper : MonoBehaviour
    {
        [Header("Two Player Setup")]
        [SerializeField]
        private ArcadeKart player1Kart;

        [SerializeField]
        private Vector3 player2SpawnOffset = new Vector3(0, 0, 5);

        [SerializeField]
        private int gamepad2JoystickNumber = 1;

        [Header("Player 2 Input Type")]
        [SerializeField]
        private bool useKeyboardForPlayer2 = false;
        [Tooltip("If false, uses GamepadInput. If true, uses AlternateKeyboardInput (WASD)")]

        public void DuplicateKartForPlayer2()
        {
            if (player1Kart == null)
            {
                Debug.LogError("Player 1 kart must be assigned!");
                return;
            }

            // Duplicate the player 1 kart
            GameObject player2KartObj = Instantiate(player1Kart.gameObject, 
                player1Kart.transform.parent);
            
            player2KartObj.name = player1Kart.gameObject.name + " (Player 2)";
            
            // Set new spawn position
            player2KartObj.transform.position = player1Kart.transform.position + player2SpawnOffset;

            // Remove existing input components
            KeyboardInput[] keyboardInputs = player2KartObj.GetComponents<KeyboardInput>();
            foreach (var ki in keyboardInputs)
            {
                DestroyImmediate(ki);
            }

            GamepadInput[] gamepadInputs = player2KartObj.GetComponents<GamepadInput>();
            foreach (var gi in gamepadInputs)
            {
                DestroyImmediate(gi);
            }

            AlternateKeyboardInput[] altKeyboardInputs = player2KartObj.GetComponents<AlternateKeyboardInput>();
            foreach (var aki in altKeyboardInputs)
            {
                DestroyImmediate(aki);
            }

            // Add the appropriate input component based on choice
            if (useKeyboardForPlayer2)
            {
                // Add AlternateKeyboardInput for WASD controls
                var altKeyboardInput = player2KartObj.AddComponent<AlternateKeyboardInput>();
                Debug.Log("Player 2 set to WASD Keyboard controls");
            }
            else
            {
                // Add GamepadInput for gamepad control
                var gamepadInput = player2KartObj.AddComponent<GamepadInput>();
                gamepadInput.JoystickNumber = gamepad2JoystickNumber;
                Debug.Log($"Player 2 set to Gamepad (Joystick {gamepad2JoystickNumber})");
            }

            // Give it a different name for clarity
            var arcadeKart = player2KartObj.GetComponent<ArcadeKart>();
            if (arcadeKart != null)
            {
                arcadeKart.gameObject.name = "Player2_Kart";
            }

            Debug.Log($"Player 2 kart created at {player2KartObj.transform.position}");
            #if UNITY_EDITOR
            Selection.activeGameObject = player2KartObj;
            #endif
        }

        [ContextMenu("Duplicate Kart for Player 2")]
        private void ContextMenuDuplicate()
        {
            DuplicateKartForPlayer2();
        }
    }
}
