using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PortraitGameSetup : MonoBehaviour
{
    [Header("Auto-Setup for Portrait Mode")]
    [Tooltip("Automatically configure the game for portrait mode on start")]
    public bool autoSetupPortraitMode = true;
    
    [Header("Camera Settings")]
    public Camera mainCamera;
    [Range(0.1f, 2.0f)]
    public float portraitAspectRatio = 0.625f; // 600/960 = 0.625
    
    [Header("UI Canvas Settings")]
    public Canvas mainCanvas;
    public CanvasScaler canvasScaler;
    
    [Header("Orientation Scripts")]
    public PhoneOrientationController orientationController;
    public OrientationLock orientationLock;
    
    [Header("Debug")]
    public bool showSetupInfo = true;
    
    void Start()
    {
        if (autoSetupPortraitMode)
        {
            SetupPortraitMode();
        }
    }
    
    [ContextMenu("Setup Portrait Mode")]
    public void SetupPortraitMode()
    {
        Debug.Log("PortraitGameSetup: Configuring game for portrait mode...");
        
        // Setup camera
        SetupCamera();
        
        // Setup UI
        SetupUI();
        
        // Setup orientation scripts
        SetupOrientationScripts();
        
        // Apply portrait resolution
        ApplyPortraitResolution();
        
        if (showSetupInfo)
        {
            Debug.Log("PortraitGameSetup: Portrait mode setup complete!");
            LogCurrentSettings();
        }
    }
    
    void SetupCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        if (mainCamera != null)
        {
            // Set camera to orthographic for 2D games
            mainCamera.orthographic = true;
            
            // Adjust orthographic size for portrait aspect ratio
            float targetHeight = 10f; // Adjust this based on your game
            mainCamera.orthographicSize = targetHeight;
            
            Debug.Log($"PortraitGameSetup: Camera configured - Orthographic Size: {mainCamera.orthographicSize}");
        }
        else
        {
            Debug.LogWarning("PortraitGameSetup: No camera found!");
        }
    }
    
    void SetupUI()
    {
        if (mainCanvas == null)
        {
            mainCanvas = FindFirstObjectByType<Canvas>();
        }
        
        if (mainCanvas != null)
        {
            // Set canvas to screen space overlay
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Setup canvas scaler for portrait mode
            if (canvasScaler == null)
            {
                canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
                if (canvasScaler == null)
                {
                    canvasScaler = mainCanvas.gameObject.AddComponent<CanvasScaler>();
                }
            }
            
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(600, 960); // Portrait resolution
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
            }
            
            Debug.Log("PortraitGameSetup: UI Canvas configured for portrait mode");
        }
        else
        {
            Debug.LogWarning("PortraitGameSetup: No Canvas found!");
        }
    }
    
    void SetupOrientationScripts()
    {
        // Find or create PhoneOrientationController
        if (orientationController == null)
        {
            orientationController = FindFirstObjectByType<PhoneOrientationController>();
            if (orientationController == null)
            {
                GameObject orientationObj = new GameObject("PhoneOrientationController");
                orientationController = orientationObj.AddComponent<PhoneOrientationController>();
            }
        }
        
        // Find or create OrientationLock
        if (orientationLock == null)
        {
            orientationLock = FindFirstObjectByType<OrientationLock>();
            if (orientationLock == null)
            {
                GameObject lockObj = new GameObject("OrientationLock");
                orientationLock = lockObj.AddComponent<OrientationLock>();
            }
        }
        
        // Configure orientation scripts
        if (orientationController != null)
        {
            orientationController.usePhoneOrientation = true;
            orientationController.showDebugInfo = showSetupInfo;
        }
        
        if (orientationLock != null)
        {
            orientationLock.lockToPortrait = true;
            orientationLock.showOrientationWarning = true;
            orientationLock.showDebugInfo = showSetupInfo;
        }
        
        Debug.Log("PortraitGameSetup: Orientation scripts configured");
    }
    
    void ApplyPortraitResolution()
    {
        // Only apply resolution on WebGL builds
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            int width = 600;
            int height = 960;
            
            Screen.SetResolution(width, height, true);
            Debug.Log($"PortraitGameSetup: Resolution set to {width}x{height} for portrait mode");
        }
        else
        {
            Debug.Log("PortraitGameSetup: Resolution setting skipped (not WebGL build)");
        }
    }
    
    void LogCurrentSettings()
    {
        Debug.Log("=== Portrait Game Setup Summary ===");
        Debug.Log($"Screen Resolution: {Screen.width}x{Screen.height}");
        Debug.Log($"Screen Orientation: {Screen.orientation}");
        Debug.Log($"Aspect Ratio: {(float)Screen.width / Screen.height:F3}");
        
        if (mainCamera != null)
        {
            Debug.Log($"Camera Orthographic: {mainCamera.orthographic}");
            Debug.Log($"Camera Size: {mainCamera.orthographicSize}");
        }
        
        if (mainCanvas != null && canvasScaler != null)
        {
            Debug.Log($"Canvas Reference Resolution: {canvasScaler.referenceResolution}");
            Debug.Log($"Canvas Scale Mode: {canvasScaler.uiScaleMode}");
        }
        
        Debug.Log("=====================================");
    }
    
    void OnGUI()
    {
        if (showSetupInfo && Application.platform == RuntimePlatform.WebGLPlayer)
        {
            GUI.Box(new Rect(10, 350, 300, 150), "Portrait Setup Debug");
            
            GUI.Label(new Rect(20, 375, 280, 20), $"Resolution: {Screen.width}x{Screen.height}");
            GUI.Label(new Rect(20, 395, 280, 20), $"Aspect: {(float)Screen.width / Screen.height:F2}");
            GUI.Label(new Rect(20, 415, 280, 20), $"Orientation: {Screen.orientation}");
            
            if (mainCamera != null)
            {
                GUI.Label(new Rect(20, 435, 280, 20), $"Camera Size: {mainCamera.orthographicSize:F1}");
            }
            
            if (GUI.Button(new Rect(20, 455, 100, 30), "Re-setup"))
            {
                SetupPortraitMode();
            }
            
            if (GUI.Button(new Rect(130, 455, 100, 30), "Log Info"))
            {
                LogCurrentSettings();
            }
        }
    }
}
