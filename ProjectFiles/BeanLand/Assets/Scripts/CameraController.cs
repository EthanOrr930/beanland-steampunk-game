using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target; // The player to follow
    public Vector3 offset = new Vector3(0, 0, -10); // Camera offset from target
    public float followSpeed = 5f; // How fast the camera follows
    public bool followX = true; // Whether to follow on X axis
    public bool followY = true; // Whether to follow on Y axis
    
    [Header("Smooth Following")]
    public bool useSmoothFollow = true; // Enable/disable smooth following
    public float smoothTime = 0.3f; // Time to reach target position
    
    [Header("Bounds (Optional)")]
    public bool useBounds = false; // Enable/disable camera bounds
    public float minX = -10f; // Minimum X position
    public float maxX = 10f; // Maximum X position
    public float minY = -10f; // Minimum Y position
    public float maxY = 10f; // Maximum Y position
    
    [Header("Look Ahead")]
    public bool useLookAhead = false; // Enable/disable look ahead
    public float lookAheadDistance = 2f; // How far ahead to look
    public float lookAheadSpeed = 2f; // Speed of look ahead movement
    
    [Header("Screen Shake")]
    public float shakeIntensity = 0.5f; // How strong the shake is
    public float shakeDuration = 0.3f; // How long the shake lasts
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Shake intensity over time
    
    // Private variables
    private Vector3 velocity = Vector3.zero;
    private Vector3 lookAheadOffset = Vector3.zero;
    private Vector3 lastTargetPosition;
    
    // Screen shake variables
    private bool isShaking = false;
    private float shakeTimer = 0f;
    private Vector3 shakeOffset = Vector3.zero;
    private Vector3 originalPosition;
    
    void Start()
    {
        // If no target is assigned, try to find the player by tag
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                Debug.LogWarning("CameraController: No target assigned and no GameObject with 'Player' tag found!");
            }
        }
        
        // Initialize last target position
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Calculate target position
        Vector3 targetPosition = CalculateTargetPosition();
        
        // Apply bounds if enabled
        if (useBounds)
        {
            targetPosition = ApplyBounds(targetPosition);
        }
        
        // Move camera to target position
        if (useSmoothFollow)
        {
            // Smooth following using SmoothDamp
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        else
        {
            // Direct following
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
        
        // Apply screen shake
        UpdateScreenShake();
    }
    
    Vector3 CalculateTargetPosition()
    {
        Vector3 targetPosition = target.position + offset;
        
        // Apply axis following settings
        if (!followX)
        {
            targetPosition.x = transform.position.x;
        }
        if (!followY)
        {
            targetPosition.y = transform.position.y;
        }
        
        // Apply look ahead if enabled
        if (useLookAhead)
        {
            Vector3 targetVelocity = (target.position - lastTargetPosition) / Time.deltaTime;
            Vector3 lookAheadTarget = targetVelocity.normalized * lookAheadDistance;
            
            // Smooth the look ahead offset
            lookAheadOffset = Vector3.Lerp(lookAheadOffset, lookAheadTarget, lookAheadSpeed * Time.deltaTime);
            
            targetPosition += lookAheadOffset;
        }
        
        // Update last target position
        lastTargetPosition = target.position;
        
        return targetPosition;
    }
    
    Vector3 ApplyBounds(Vector3 position)
    {
        // Clamp position within bounds
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.y = Mathf.Clamp(position.y, minY, maxY);
        
        return position;
    }
    
    // Public method to set a new target
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }
    
    // Public method to instantly move camera to target (useful for respawning)
    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 targetPosition = CalculateTargetPosition();
            if (useBounds)
            {
                targetPosition = ApplyBounds(targetPosition);
            }
            transform.position = targetPosition;
        }
    }
    
    // Public method to set camera bounds
    public void SetBounds(float minX, float maxX, float minY, float maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
        useBounds = true;
    }
    
    // Public method to disable bounds
    public void DisableBounds()
    {
        useBounds = false;
    }
    
    // Screen shake methods
    void UpdateScreenShake()
    {
        if (isShaking)
        {
            shakeTimer += Time.deltaTime;
            
            if (shakeTimer >= shakeDuration)
            {
                // Shake finished
                isShaking = false;
                shakeTimer = 0f;
                shakeOffset = Vector3.zero;
            }
            else
            {
                // Calculate shake intensity based on curve
                float normalizedTime = shakeTimer / shakeDuration;
                float currentIntensity = shakeCurve.Evaluate(normalizedTime) * shakeIntensity;
                
                // Generate random shake offset
                shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * currentIntensity,
                    Random.Range(-1f, 1f) * currentIntensity,
                    0f
                );
            }
        }
        
        // Apply shake offset to camera position
        if (isShaking)
        {
            transform.position += shakeOffset;
        }
    }
    
    // Public method to trigger screen shake
    public void TriggerScreenShake()
    {
        TriggerScreenShake(shakeIntensity, shakeDuration);
    }
    
    // Public method to trigger screen shake with custom parameters
    public void TriggerScreenShake(float intensity, float duration)
    {
        isShaking = true;
        shakeTimer = 0f;
        shakeIntensity = intensity;
        shakeDuration = duration;
        Debug.Log($"Camera: Screen shake triggered! Intensity: {intensity}, Duration: {duration}");
    }
    
    // Public method to trigger screen shake with custom parameters (doesn't override default settings)
    public void TriggerScreenShakeOnce(float intensity, float duration)
    {
        isShaking = true;
        shakeTimer = 0f;
        // Store original settings
        float originalIntensity = shakeIntensity;
        float originalDuration = shakeDuration;
        
        // Set custom settings
        shakeIntensity = intensity;
        shakeDuration = duration;
        
        Debug.Log($"Camera: Screen shake triggered once! Intensity: {intensity}, Duration: {duration}");
    }
    
    // Debug visualization for bounds
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, transform.position.z);
            Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
