using UnityEngine;

public class MultiplayerGravityController : MonoBehaviour
{
    [Header("Multiplayer Gravity Settings")]
    [SerializeField] private bool useMultiplayerGravity = true;
    [SerializeField] private float gravityMultiplier = 9.8f;
    [SerializeField] private float smoothingFactor = 5f;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Gravity Alignment")]
    [SerializeField] private float alignmentThreshold = 30f;
    
    // Private variables
    private Vector3 playerGravityDirection = Vector3.down;
    private Vector3 smoothedGravityDirection = Vector3.down;
    private Vector3 globalGravityDirection = Vector3.down;
    private bool isGravityAligned = false;
    
    // Components
    private Rigidbody2D playerRigidbody;
    private PlayerController playerController;
    private MultiplayerManager multiplayerManager;
    
    private void Start()
    {
        // Get components
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        multiplayerManager = MultiplayerManager.Instance;
        
        // Initialize gravity
        playerGravityDirection = Vector3.down;
        smoothedGravityDirection = Vector3.down;
        globalGravityDirection = Vector3.down;
        
        Log("MultiplayerGravityController initialized");
    }
    
    private void Update()
    {
        if (!useMultiplayerGravity) return;
        
        // Update player gravity from phone orientation
        UpdatePlayerGravity();
        
        // Check for gravity alignment
        CheckGravityAlignment();
        
        // Apply gravity
        ApplyGravity();
    }
    
    /// <summary>
    /// Update player gravity from phone orientation
    /// </summary>
    private void UpdatePlayerGravity()
    {
        if (playerController == null) return;
        
        // Get gravity direction from player controller's phone orientation
        Vector3 phoneGravity = GetPhoneGravityDirection();
        
        // Update player gravity direction
        playerGravityDirection = phoneGravity;
        
        // Send to multiplayer manager
        if (multiplayerManager != null)
        {
            multiplayerManager.UpdatePlayerGravity(playerGravityDirection);
        }
        
        Log($"Player gravity updated: {playerGravityDirection}");
    }
    
    /// <summary>
    /// Get gravity direction from phone orientation
    /// </summary>
    private Vector3 GetPhoneGravityDirection()
    {
        if (playerController == null) return Vector3.down;
        
        // Get phone orientation controller
        PhoneOrientationController orientationController = playerController.GetComponent<PhoneOrientationController>();
        if (orientationController == null)
        {
            orientationController = FindFirstObjectByType<PhoneOrientationController>();
        }
        
        if (orientationController != null && orientationController.IsPhoneOrientationWorking())
        {
            // Get gravity direction from phone orientation
            Vector3 phoneGravity = orientationController.GetGravityDirection();
            
            // Convert phone gravity to world gravity
            // Phone's "down" becomes game's gravity direction
            Vector3 worldGravity = new Vector3(phoneGravity.x, phoneGravity.y, 0f);
            worldGravity.Normalize();
            
            return worldGravity;
        }
        
        // Fallback to default gravity
        return Vector3.down;
    }
    
    /// <summary>
    /// Check for gravity alignment with other players
    /// </summary>
    private void CheckGravityAlignment()
    {
        if (multiplayerManager == null) return;
        
        // Check if gravity is aligned
        bool wasAligned = isGravityAligned;
        isGravityAligned = multiplayerManager.IsGravityAligned();
        
        // Update global gravity direction if aligned
        if (isGravityAligned)
        {
            Vector3 newGlobalGravity = multiplayerManager.GetGlobalGravityDirection();
            if (newGlobalGravity != globalGravityDirection)
            {
                globalGravityDirection = newGlobalGravity;
                Log($"Global gravity updated: {globalGravityDirection}");
            }
        }
        
        // Log alignment changes
        if (isGravityAligned != wasAligned)
        {
            Log($"Gravity alignment changed: {isGravityAligned}");
        }
    }
    
    /// <summary>
    /// Apply gravity to the player
    /// </summary>
    private void ApplyGravity()
    {
        if (playerRigidbody == null) return;
        
        Vector3 targetGravity;
        
        if (isGravityAligned)
        {
            // Use global gravity when aligned
            targetGravity = globalGravityDirection * gravityMultiplier;
            Log($"Using aligned global gravity: {targetGravity}");
        }
        else
        {
            // Use player's individual gravity
            targetGravity = playerGravityDirection * gravityMultiplier;
            Log($"Using individual player gravity: {targetGravity}");
        }
        
        // Smooth the gravity transition
        Vector3 currentGravity = Physics2D.gravity;
        Vector3 smoothedGravity = Vector3.Lerp(currentGravity, targetGravity, Time.deltaTime * smoothingFactor);
        
        // Apply gravity
        Physics2D.gravity = smoothedGravity;
        
        // Debug visualization
        if (showDebugLogs)
        {
            Debug.DrawRay(transform.position, playerGravityDirection * 2f, Color.blue, 0f);
            if (isGravityAligned)
            {
                Debug.DrawRay(transform.position, globalGravityDirection * 2f, Color.green, 0f);
            }
        }
    }
    
    /// <summary>
    /// Set multiplayer gravity (called by MultiplayerManager)
    /// </summary>
    public void SetMultiplayerGravity(Vector3 direction)
    {
        globalGravityDirection = direction;
        Log($"Multiplayer gravity set to: {direction}");
    }
    
    /// <summary>
    /// Get the current player gravity direction
    /// </summary>
    public Vector3 GetPlayerGravityDirection()
    {
        return playerGravityDirection;
    }
    
    /// <summary>
    /// Get the current global gravity direction
    /// </summary>
    public Vector3 GetGlobalGravityDirection()
    {
        return globalGravityDirection;
    }
    
    /// <summary>
    /// Check if gravity is currently aligned
    /// </summary>
    public bool IsGravityAligned()
    {
        return isGravityAligned;
    }
    
    /// <summary>
    /// Enable or disable multiplayer gravity
    /// </summary>
    public void SetMultiplayerGravityEnabled(bool enabled)
    {
        useMultiplayerGravity = enabled;
        Log($"Multiplayer gravity {(enabled ? "enabled" : "disabled")}");
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[MultiplayerGravityController] {message}");
        }
    }
}
