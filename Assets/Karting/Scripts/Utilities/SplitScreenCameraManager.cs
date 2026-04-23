using UnityEngine;
using Cinemachine;

namespace KartGame.KartSystems
{
    /// <summary>
    /// Manages split-screen camera setup for two-player mode.
    /// Sets up two viewports (left and right) for each player's camera.
    /// </summary>
    public class SplitScreenCameraManager : MonoBehaviour
    {
        [SerializeField]
        private CinemachineVirtualCamera player1Camera;

        [SerializeField]
        private CinemachineVirtualCamera player2Camera;

        [SerializeField]
        private Camera mainCamera;

        private Camera player1RenderTexture;
        private Camera player2RenderTexture;

        private void Start()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            SetupSplitScreen();
        }

        private void SetupSplitScreen()
        {
            if (player1Camera == null || player2Camera == null)
            {
                Debug.LogError("SplitScreenCameraManager: Both player cameras must be assigned!");
                return;
            }

            // Set up main camera to render both viewports
            if (mainCamera != null)
            {
                // Left viewport for player 1
                Rect leftViewport = new Rect(0, 0, 0.5f, 1f);
                mainCamera.rect = leftViewport;
            }

            // Create a second camera for player 2 (right side)
            GameObject player2CameraObj = new GameObject("Player2 Camera");
            player2CameraObj.transform.SetParent(mainCamera.transform.parent);
            Camera player2Cam = player2CameraObj.AddComponent<Camera>();
            player2Cam.rect = new Rect(0.5f, 0, 0.5f, 1f);
            player2Cam.depth = -1;
            player2Cam.CopyFrom(mainCamera);

            // Assign the AI layer to player 2's camera if needed
            player2Cam.cullingMask = mainCamera.cullingMask;

            player2RenderTexture = player2Cam;
        }

        public void SetPlayer1Target(Transform target)
        {
            if (player1Camera != null && player1Camera.Follow == null)
            {
                player1Camera.Follow = target;
                player1Camera.LookAt = target;
            }
        }

        public void SetPlayer2Target(Transform target)
        {
            if (player2Camera != null && player2Camera.Follow == null)
            {
                player2Camera.Follow = target;
                player2Camera.LookAt = target;
            }
        }
    }
}
