using UnityEngine;

public class OrientationLock : MonoBehaviour
{
    [Header("Orientation Lock Settings")]
    [Tooltip("Lock the screen to portrait orientation")]
    public bool lockToPortrait = true;
    
    [Tooltip("Lock the screen to landscape orientation")]
    public bool lockToLandscape = false;
    
    [Tooltip("Allow upside down orientations")]
    public bool allowUpsideDown = false;
    
    [Tooltip("Show orientation warning when device is rotated")]
    public bool showOrientationWarning = true;
    
    [Tooltip("Show debug information in console")]
    public bool showDebugInfo = true;
    
    [Header("Warning UI")]
    [Tooltip("Warning message to show when orientation is wrong")]
    public string warningMessage = "Please rotate your device to portrait mode";
    
    [Tooltip("Warning message color")]
    public Color warningColor = Color.white;
    
    [Tooltip("Warning background color")]
    public Color warningBackgroundColor = new Color(0, 0, 0, 0.8f);
    
    [Header("Orientation Events")]
    [Tooltip("Called when orientation is locked")]
    public UnityEngine.Events.UnityEvent OnOrientationLocked;
    
    [Tooltip("Called when orientation warning is shown")]
    public UnityEngine.Events.UnityEvent OnOrientationWarning;
    
    // Private variables
    private ScreenOrientation targetOrientation;
    private bool isOrientationLocked = false;
    private bool showingWarning = false;
    private float warningStartTime = 0f;
    private float warningDuration = 3f; // Show warning for 3 seconds
    
    void Start()
    {
        // Set initial orientation
        SetTargetOrientation();
        
        // Apply orientation lock
        ApplyOrientationLock();
        
        if (showDebugInfo)
        {
            Debug.Log($"OrientationLock: Target orientation set to {targetOrientation}");
        }
    }
    
    void Update()
    {
        // Check current orientation and apply lock if needed
        CheckAndApplyOrientationLock();
        
        // Update warning display
        UpdateWarningDisplay();
    }
    
    void SetTargetOrientation()
    {
        if (lockToPortrait)
        {
            targetOrientation = allowUpsideDown ? 
                ScreenOrientation.Portrait : 
                ScreenOrientation.Portrait;
        }
        else if (lockToLandscape)
        {
            targetOrientation = allowUpsideDown ? 
                ScreenOrientation.LandscapeLeft : 
                ScreenOrientation.LandscapeLeft;
        }
        else
        {
            // Auto-rotate based on device orientation
            targetOrientation = ScreenOrientation.AutoRotation;
        }
    }
    
    void ApplyOrientationLock()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // For WebGL, we can't directly control orientation
            // But we can show warnings and handle it in the HTML template
            isOrientationLocked = true;
            
            if (showDebugInfo)
            {
                Debug.Log("OrientationLock: WebGL orientation lock applied (handled by HTML template)");
            }
        }
        else
        {
            // For other platforms, use Unity's orientation settings
            Screen.orientation = targetOrientation;
            isOrientationLocked = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"OrientationLock: Orientation locked to {targetOrientation}");
            }
        }
        
        OnOrientationLocked?.Invoke();
    }
    
    void CheckAndApplyOrientationLock()
    {
        if (!isOrientationLocked) return;
        
        // Check if current orientation matches target
        bool orientationCorrect = IsOrientationCorrect();
        
        if (!orientationCorrect && showOrientationWarning)
        {
            if (!showingWarning)
            {
                ShowOrientationWarning();
            }
        }
        else if (orientationCorrect && showingWarning)
        {
            HideOrientationWarning();
        }
    }
    
    bool IsOrientationCorrect()
    {
        if (lockToPortrait)
        {
            return Screen.orientation == ScreenOrientation.Portrait || 
                   (allowUpsideDown && Screen.orientation == ScreenOrientation.PortraitUpsideDown);
        }
        else if (lockToLandscape)
        {
            return Screen.orientation == ScreenOrientation.LandscapeLeft || 
                   Screen.orientation == ScreenOrientation.LandscapeRight;
        }
        
        return true; // Auto-rotate is always correct
    }
    
    void ShowOrientationWarning()
    {
        showingWarning = true;
        warningStartTime = Time.time;
        
        if (showDebugInfo)
        {
            Debug.Log("OrientationLock: Showing orientation warning");
        }
        
        OnOrientationWarning?.Invoke();
    }
    
    void HideOrientationWarning()
    {
        showingWarning = false;
        
        if (showDebugInfo)
        {
            Debug.Log("OrientationLock: Hiding orientation warning");
        }
    }
    
    void UpdateWarningDisplay()
    {
        if (showingWarning && Time.time - warningStartTime > warningDuration)
        {
            HideOrientationWarning();
        }
    }
    
    /// <summary>
    /// Set the target orientation for locking
    /// </summary>
    /// <param name="orientation">Target orientation</param>
    public void SetTargetOrientation(ScreenOrientation orientation)
    {
        targetOrientation = orientation;
        ApplyOrientationLock();
        
        if (showDebugInfo)
        {
            Debug.Log($"OrientationLock: Target orientation changed to {orientation}");
        }
    }
    
    /// <summary>
    /// Enable or disable portrait lock
    /// </summary>
    /// <param name="enabled">True to enable portrait lock</param>
    public void SetPortraitLock(bool enabled)
    {
        lockToPortrait = enabled;
        if (enabled) lockToLandscape = false;
        SetTargetOrientation();
        ApplyOrientationLock();
    }
    
    /// <summary>
    /// Enable or disable landscape lock
    /// </summary>
    /// <param name="enabled">True to enable landscape lock</param>
    public void SetLandscapeLock(bool enabled)
    {
        lockToLandscape = enabled;
        if (enabled) lockToPortrait = false;
        SetTargetOrientation();
        ApplyOrientationLock();
    }
    
    /// <summary>
    /// Enable or disable upside down orientations
    /// </summary>
    /// <param name="enabled">True to allow upside down</param>
    public void SetAllowUpsideDown(bool enabled)
    {
        allowUpsideDown = enabled;
        SetTargetOrientation();
        ApplyOrientationLock();
    }
    
    /// <summary>
    /// Enable or disable orientation warnings
    /// </summary>
    /// <param name="enabled">True to show warnings</param>
    public void SetShowOrientationWarning(bool enabled)
    {
        showOrientationWarning = enabled;
        
        if (!enabled && showingWarning)
        {
            HideOrientationWarning();
        }
    }
    
    /// <summary>
    /// Check if orientation is currently locked
    /// </summary>
    /// <returns>True if orientation is locked</returns>
    public bool IsOrientationLocked()
    {
        return isOrientationLocked;
    }
    
    /// <summary>
    /// Check if orientation warning is currently showing
    /// </summary>
    /// <returns>True if warning is showing</returns>
    public bool IsShowingWarning()
    {
        return showingWarning;
    }
    
    void OnGUI()
    {
        if (showingWarning && showOrientationWarning)
        {
            // Draw warning background
            GUI.color = warningBackgroundColor;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            
            // Draw warning message
            GUI.color = warningColor;
            GUI.skin.label.fontSize = 24;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), warningMessage);
            
            // Reset GUI color
            GUI.color = Color.white;
        }
        
        if (showDebugInfo && Application.platform == RuntimePlatform.WebGLPlayer)
        {
            GUI.Box(new Rect(10, 140, 300, 80), "Orientation Lock Debug");
            
            GUI.Label(new Rect(20, 165, 280, 20), $"Target: {targetOrientation}");
            GUI.Label(new Rect(20, 185, 280, 20), $"Current: {Screen.orientation}");
            GUI.Label(new Rect(20, 205, 280, 20), $"Warning: {(showingWarning ? "Yes" : "No")}");
        }
    }
}
