using UnityEngine;
using UnityEngine.EventSystems;

public class GameSetup : MonoBehaviour
{
    void Start()
    {
        // Add UISetup component to automatically create the game UI
                if (FindObjectOfType<UISetup>() == null)
        {
            GameObject setupObj = new GameObject("GameSetup");
            setupObj.AddComponent<UISetup>();
        }
        
        // Set up camera for 2D
        SetupCamera();
    }
    
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }
        
        // Configure camera for 2D game
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.2f, 0.4f, 0.2f); // Dark green background
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 8f;
        mainCamera.transform.position = new Vector3(0, 0, -10);
        
        // Ensure EventSystem exists for UI interactions
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }
    }
} 