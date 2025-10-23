using UnityEngine;

public class PhoneOrientationController : MonoBehaviour
{
    [Header("Orientation Settings")]
    [Tooltip("Enable phone orientation detection")]
    public bool usePhoneOrientation = true;
    
    [Tooltip("Show debug information in console")]
    public bool showDebugInfo = true;
    
    [Tooltip("Smoothing factor for orientation changes")]
    [Range(0.1f, 10f)]
    public float smoothingFactor = 2f;
    
    [Header("Orientation Data")]
    [Tooltip("Current device orientation angle")]
    public float currentOrientation = 0f;
    
    [Tooltip("Smoothed orientation angle")]
    public float smoothedOrientation = 0f;
    
    [Tooltip("Device motion data (accelerometer)")]
    public Vector3 accelerometerData = Vector3.zero;
    
    [Tooltip("Device gyroscope data")]
    public Vector3 gyroscopeData = Vector3.zero;
    
    [Header("Orientation Events")]
    [Tooltip("Called when orientation changes")]
    public UnityEngine.Events.UnityEvent<float> OnOrientationChanged;
    
    [Tooltip("Called when device motion is detected")]
    public UnityEngine.Events.UnityEvent<Vector3> OnMotionDetected;
    
    // Private variables
    private float targetOrientation = 0f;
    private bool isOrientationSupported = false;
    private bool isMotionSupported = false;
    private float lastOrientationUpdate = 0f;
    
    // Data will be received from external website via Unity's WebGL communication
    // No external JavaScript dependencies needed
    
    void Start()
    {
        Debug.Log($"PhoneOrientationController: Starting on GameObject '{gameObject.name}' on platform: {Application.platform}");
        
        // For WebGL builds, data will be received from external website
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // Enable orientation and motion support for WebGL
            // Data will be provided by external website
            isOrientationSupported = true;
            isMotionSupported = true;
            
            Debug.Log("PhoneOrientationController: WebGL mode - waiting for external data");
        }
        else
        {
            // For non-WebGL platforms, use Unity's built-in orientation
            isOrientationSupported = true;
            isMotionSupported = true;
            
            Debug.Log("PhoneOrientationController: Using Unity's built-in orientation system");
        }
        
        // Initialize orientation
        currentOrientation = 0f;
        smoothedOrientation = 0f;
        targetOrientation = 0f;
        
        Debug.Log($"PhoneOrientationController: Initialized - Orientation: {isOrientationSupported}, Motion: {isMotionSupported}");
    }
    
    void Update()
    {
        if (!usePhoneOrientation) return;
        
        // Update orientation data
        UpdateOrientationData();
        
        // Update motion data
        UpdateMotionData();
        
        // Apply smoothing
        ApplySmoothing();
    }
    
    void UpdateOrientationData()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // For WebGL, orientation data will be set externally via SetOrientationData()
            // This method is called by external website JavaScript
            // No need to poll for data here
        }
        else
        {
            // Use Unity's built-in orientation for non-WebGL platforms
            float newOrientation = 0f;
            
            switch (Screen.orientation)
            {
                case ScreenOrientation.Portrait:
                    newOrientation = 0f;
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                    newOrientation = 180f;
                    break;
                case ScreenOrientation.LandscapeLeft:
                    newOrientation = 90f;
                    break;
                case ScreenOrientation.LandscapeRight:
                    newOrientation = 270f;
                    break;
            }
            
            if (newOrientation != currentOrientation)
            {
                currentOrientation = newOrientation;
                targetOrientation = newOrientation;
                
                if (showDebugInfo)
                {
                    Debug.Log($"PhoneOrientationController: Unity orientation changed to {currentOrientation}°");
                }
                
                // Trigger orientation change event
                OnOrientationChanged?.Invoke(currentOrientation);
            }
        }
    }
    
    void UpdateMotionData()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // For WebGL, motion data will be set externally via SetMotionData()
            // This method is called by external website JavaScript
            // No need to poll for data here
        }
        else
        {
            // Use Unity's built-in accelerometer for non-WebGL platforms
            Vector3 newAccel = Input.acceleration;
            if (newAccel != accelerometerData)
            {
                accelerometerData = newAccel;
                OnMotionDetected?.Invoke(accelerometerData);
            }
        }
    }
    
    void ApplySmoothing()
    {
        // Apply smoothing to orientation
        if (smoothingFactor > 0f)
        {
            smoothedOrientation = Mathf.Lerp(smoothedOrientation, targetOrientation, 
                Time.deltaTime * smoothingFactor);
        }
        else
        {
            smoothedOrientation = targetOrientation;
        }
    }
    
    /// <summary>
    /// Get the current device orientation angle
    /// </summary>
    /// <returns>Orientation angle in degrees</returns>
    public float GetCurrentOrientation()
    {
        return currentOrientation;
    }
    
    /// <summary>
    /// Get the smoothed device orientation angle
    /// </summary>
    /// <returns>Smoothed orientation angle in degrees</returns>
    public float GetSmoothedOrientation()
    {
        return smoothedOrientation;
    }
    
    /// <summary>
    /// Get the current accelerometer data
    /// </summary>
    /// <returns>Accelerometer data as Vector3</returns>
    public Vector3 GetAccelerometerData()
    {
        return accelerometerData;
    }
    
    /// <summary>
    /// Get the current gyroscope data
    /// </summary>
    /// <returns>Gyroscope data as Vector3</returns>
    public Vector3 GetGyroscopeData()
    {
        return gyroscopeData;
    }
    
    /// <summary>
    /// Get the gravity direction based on phone orientation
    /// This is used by PlayerController to control game gravity
    /// </summary>
    /// <returns>Gravity direction as Vector3</returns>
    public Vector3 GetGravityDirection()
    {
        // Return the accelerometer data which represents the gravity vector
        // The accelerometer data from the device shows which way is "down"
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // For WebGL, use the data from the JavaScript bridge
            return accelerometerData;
        }
        else
        {
            // For other platforms, use Unity's built-in accelerometer
            return Input.acceleration;
        }
    }
    
    /// <summary>
    /// Check if phone orientation is working and providing valid data
    /// </summary>
    /// <returns>True if phone orientation is working</returns>
    public bool IsPhoneOrientationWorking()
    {
        // Check if orientation is enabled and supported
        if (!usePhoneOrientation)
        {
            return false;
        }
        
        // Check if motion/orientation is supported on this device
        if (!isOrientationSupported && !isMotionSupported)
        {
            return false;
        }
        
        // For WebGL, check if we're actually getting data
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            // If we have any non-zero accelerometer data, it's working
            return accelerometerData.magnitude > 0.01f;
        }
        else
        {
            // For other platforms, check Unity's accelerometer
            return Input.acceleration.magnitude > 0.01f;
        }
    }
    
    /// <summary>
    /// Check if orientation is supported on this device
    /// </summary>
    /// <returns>True if orientation is supported</returns>
    public bool IsPhoneOrientationSupported()
    {
        return isOrientationSupported;
    }
    
    /// <summary>
    /// Check if motion detection is supported on this device
    /// </summary>
    /// <returns>True if motion detection is supported</returns>
    public bool IsPhoneMotionSupported()
    {
        return isMotionSupported;
    }
    
    /// <summary>
    /// Enable or disable phone orientation detection
    /// </summary>
    /// <param name="enabled">True to enable, false to disable</param>
    public void SetPhoneOrientationEnabled(bool enabled)
    {
        usePhoneOrientation = enabled;
        
        if (showDebugInfo)
        {
            Debug.Log($"PhoneOrientationController: Phone orientation {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Set orientation data from external website (WebGL only)
    /// Called by external JavaScript to send orientation data to Unity
    /// </summary>
    /// <param name="orientation">Orientation angle in degrees</param>
    public void SetOrientationData(float orientation)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            if (orientation != currentOrientation)
            {
                currentOrientation = orientation;
                targetOrientation = orientation;
                
                if (showDebugInfo)
                {
                    Debug.Log($"PhoneOrientationController: External orientation data: {currentOrientation}°");
                }
                
                // Trigger orientation change event
                OnOrientationChanged?.Invoke(currentOrientation);
            }
        }
    }
    
    /// <summary>
    /// Set motion data from external website (WebGL only)
    /// Called by external JavaScript to send accelerometer data to Unity
    /// </summary>
    /// <param name="x">Accelerometer X value</param>
    /// <param name="y">Accelerometer Y value</param>
    /// <param name="z">Accelerometer Z value</param>
    public void SetMotionData(float x, float y, float z)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            Vector3 newAccel = new Vector3(x, y, z);
            
            // Update accelerometer data
            accelerometerData = newAccel;
            OnMotionDetected?.Invoke(accelerometerData);
            
            // Only log occasionally to avoid spam (every 2 seconds)
            if (showDebugInfo && Time.time - lastOrientationUpdate > 2f)
            {
                lastOrientationUpdate = Time.time;
                Debug.Log($"[GRAVITY] Phone motion active - Accel: {accelerometerData.ToString("F2")}");
            }
        }
    }
    
    /// <summary>
    /// Set motion data from external website via string (WebGL only)
    /// Called by external JavaScript SendMessage - format: "x,y,z"
    /// </summary>
    /// <param name="motionDataString">Motion data as string "x,y,z"</param>
    public void SetMotionDataString(string motionDataString)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            try
            {
                string[] parts = motionDataString.Split(',');
                if (parts.Length == 3)
                {
                    float x = float.Parse(parts[0]);
                    float y = float.Parse(parts[1]);
                    float z = float.Parse(parts[2]);
                    
                    SetMotionData(x, y, z);
                }
                else
                {
                    Debug.LogError($"[GRAVITY] ERROR: Invalid motion data format (expected 3 parts, got {parts.Length}): {motionDataString}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GRAVITY] ERROR: Failed to parse motion data '{motionDataString}': {e.Message}");
            }
        }
    }
    
    
    /// <summary>
    /// Set both orientation and motion data from external website (WebGL only)
    /// Convenience method for setting both at once
    /// </summary>
    /// <param name="orientation">Orientation angle in degrees</param>
    /// <param name="x">Accelerometer X value</param>
    /// <param name="y">Accelerometer Y value</param>
    /// <param name="z">Accelerometer Z value</param>
    public void SetDeviceData(float orientation, float x, float y, float z)
    {
        SetOrientationData(orientation);
        SetMotionData(x, y, z);
    }
    
    
    void OnGUI()
    {
        if (showDebugInfo && Application.platform == RuntimePlatform.WebGLPlayer)
        {
            GUI.Box(new Rect(10, 10, 300, 140), "Phone Orientation Debug");
            
            GUI.Label(new Rect(20, 35, 280, 20), $"GameObject: {gameObject.name}");
            GUI.Label(new Rect(20, 55, 280, 20), $"Orientation: {currentOrientation:F1}°");
            GUI.Label(new Rect(20, 75, 280, 20), $"Smoothed: {smoothedOrientation:F1}°");
            GUI.Label(new Rect(20, 95, 280, 20), $"Accel: {accelerometerData.ToString("F2")}");
            GUI.Label(new Rect(20, 115, 280, 20), $"Gyro: {gyroscopeData.ToString("F2")}");
        }
    }
}
