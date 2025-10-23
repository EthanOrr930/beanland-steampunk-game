using UnityEngine;

public class GravityIndicator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Speed of rotation smoothing")]
    public float rotationSmoothing = 5f;
    
    [Tooltip("Offset angle for the indicator (0 = points down, 90 = points right, etc.)")]
    public float rotationOffset = 0f;
    
    [Tooltip("Whether to flip the rotation 180 degrees")]
    public bool flipRotation = false;
    
    [Header("Debug")]
    [Tooltip("Show debug information in console")]
    public bool showDebugInfo = false;
    
    private Vector3 lastGravityDirection = Vector3.down;
    
    void Start()
    {
        // Initialize with default downward gravity
        lastGravityDirection = Vector3.down;
        
        if (showDebugInfo)
        {
            Debug.Log("GravityIndicator: Initialized with default downward gravity");
        }
    }
    
    void Update()
    {
        UpdateRotation();
    }
    
    void UpdateRotation()
    {
        // Get the current world gravity direction
        Vector3 gravityDirection = Physics2D.gravity.normalized;
        
        // If gravity is zero (shouldn't happen but safety check)
        if (gravityDirection.magnitude < 0.001f)
        {
            gravityDirection = Vector3.down;
        }
        
        // Calculate the angle from the gravity direction
        // We want the indicator to point in the direction of gravity
        float targetAngle = Mathf.Atan2(gravityDirection.y, gravityDirection.x) * Mathf.Rad2Deg;
        
        // Apply rotation offset
        targetAngle += rotationOffset;
        
        // Apply flip if enabled
        if (flipRotation)
        {
            targetAngle += 180f;
        }
        
        // Convert to Unity's rotation (Z-axis rotation for 2D)
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
        
        // Smoothly rotate towards the target
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothing);
        
        // Debug information
        if (showDebugInfo)
        {
            // Only log when gravity direction changes significantly
            if (Vector3.Distance(gravityDirection, lastGravityDirection) > 0.1f)
            {
                Debug.Log($"GravityIndicator: Gravity direction changed to {gravityDirection}, " +
                         $"Target angle: {targetAngle:F1}°, " +
                         $"Current rotation: {transform.rotation.eulerAngles.z:F1}°");
                lastGravityDirection = gravityDirection;
            }
        }
    }
    
    /// <summary>
    /// Manually set the rotation offset
    /// </summary>
    /// <param name="offset">New rotation offset in degrees</param>
    public void SetRotationOffset(float offset)
    {
        rotationOffset = offset;
        
        if (showDebugInfo)
        {
            Debug.Log($"GravityIndicator: Rotation offset set to {offset}°");
        }
    }
    
    /// <summary>
    /// Toggle the flip rotation setting
    /// </summary>
    /// <param name="flip">Whether to flip the rotation</param>
    public void SetFlipRotation(bool flip)
    {
        flipRotation = flip;
        
        if (showDebugInfo)
        {
            Debug.Log($"GravityIndicator: Flip rotation set to {flip}");
        }
    }
    
    /// <summary>
    /// Set the rotation smoothing speed
    /// </summary>
    /// <param name="smoothing">New smoothing speed</param>
    public void SetRotationSmoothing(float smoothing)
    {
        rotationSmoothing = Mathf.Max(0.1f, smoothing); // Ensure minimum smoothing
        
        if (showDebugInfo)
        {
            Debug.Log($"GravityIndicator: Rotation smoothing set to {smoothing}");
        }
    }
    
    /// <summary>
    /// Get the current gravity direction as a normalized vector
    /// </summary>
    /// <returns>Current gravity direction</returns>
    public Vector3 GetCurrentGravityDirection()
    {
        return Physics2D.gravity.normalized;
    }
    
    /// <summary>
    /// Get the current gravity magnitude
    /// </summary>
    /// <returns>Current gravity magnitude</returns>
    public float GetCurrentGravityMagnitude()
    {
        return Physics2D.gravity.magnitude;
    }
}
