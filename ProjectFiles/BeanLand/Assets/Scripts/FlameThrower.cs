using UnityEngine;

public class FlameThrower : MonoBehaviour
{
    [Header("Flame Thrower Settings")]
    public float range = 5f;
    public float angle = 45f; // Half angle of the triangular range
    public Vector2 rangeOffset = Vector2.zero; // Offset for the range position
    public LayerMask playerLayerMask = 1; // Default layer
    
    [Header("Animation Settings")]
    public Sprite[] flameSprites;
    public float animationSpeed = 0.1f;
    
    [Header("Visual Settings")]
    public Color flameColor = Color.red;
    
    [Header("Death Settings")]
    public GameObject deathParticlePrefab; // Assign death particle prefab in inspector
    public float deathParticleLifetime = 2f; // How long the particle effect lasts
    
    [Header("Activation Settings")]
    public bool isActive = true; // Controls whether the flamethrower is active
    
    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentSpriteIndex = 0;
    private Transform playerTransform;
    private bool isPlayerInRange = false;
    private bool hasTriggeredDeath = false; // Prevent multiple death triggers
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set initial sprite if available
        if (flameSprites.Length > 0)
        {
            spriteRenderer.sprite = flameSprites[0];
            Debug.Log($"FlameThrower: Initialized with {flameSprites.Length} sprites");
        }
        else
        {
            Debug.LogWarning("FlameThrower: No sprites assigned!");
        }
        
        spriteRenderer.color = flameColor;
        
        // Find player (you can also assign this in inspector)
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("FlameThrower: Player found and assigned");
            }
            else
            {
                Debug.LogWarning("FlameThrower: No player found with 'Player' tag!");
            }
        }
        
        Debug.Log($"FlameThrower: Range = {range}, Animation Speed = {animationSpeed}");
    }
    
    void Update()
    {
        // Only update if flamethrower is active
        if (!isActive) return;
        
        AnimateSprites();
        CheckAndTriggerDeath();
    }
    
    void AnimateSprites()
    {
        if (flameSprites.Length == 0) return;
        
        // If flamethrower is not active, show first sprite and stop animating
        if (!isActive)
        {
            currentSpriteIndex = 0;
            spriteRenderer.sprite = flameSprites[0];
            return;
        }
        
        // If flamethrower is active, continuously animate
        if (flameSprites.Length > 1)
        {
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                currentSpriteIndex++;
                
                // Loop back to the beginning when we reach the end
                if (currentSpriteIndex >= flameSprites.Length)
                {
                    currentSpriteIndex = 0;
                }
                
                spriteRenderer.sprite = flameSprites[currentSpriteIndex];
            }
        }
    }
    
    void CheckAndTriggerDeath()
    {
        if (playerTransform == null) 
        {
            Debug.Log("FlameThrower: Player transform is null!");
            return;
        }
        
        // Calculate the range center relative to the flamethrower's rotation
        Vector3 rotatedOffset = transform.TransformDirection((Vector3)rangeOffset);
        Vector3 flamePosition = transform.position + rotatedOffset;
        Vector3 playerPosition = playerTransform.position;
        Vector3 directionToPlayer = playerPosition - flamePosition;
        
        // Check if player is within range
        float distanceToPlayer = directionToPlayer.magnitude;
        bool wasInRange = isPlayerInRange;
        
        // Check if player is within the triangular range (angle check) - pointing in flamethrower's forward direction
        Vector3 forwardDirection = transform.right;
        float angleToPlayer = Vector3.Angle(forwardDirection, directionToPlayer);
        bool isInAngleRange = angleToPlayer <= angle;
        
        // Player is in range only if both distance and angle conditions are met
        isPlayerInRange = distanceToPlayer <= range && isInAngleRange;
        
        // Debug logging
        if (wasInRange != isPlayerInRange)
        {
            Debug.Log($"FlameThrower: Player range changed! In range: {isPlayerInRange}, Distance: {distanceToPlayer:F2}, Range: {range}, Angle: {angleToPlayer:F1}, Max Angle: {angle}");
        }
        
        // If player just left range, let animation continue if it's already playing
        if (wasInRange && !isPlayerInRange)
        {
            Debug.Log("FlameThrower: Player left range - animation will complete if playing");
        }
        
        // Only trigger death if player is in range and flamethrower is active
        if (!isPlayerInRange || !isActive) return;
        
        // Trigger death if player is in range and we haven't already triggered it
        if (!hasTriggeredDeath)
        {
            TriggerPlayerDeath();
        }
    }
    
    void TriggerPlayerDeath()
    {
        hasTriggeredDeath = true;
        
        // Get the PlayerController component
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Trigger screen shake
            TriggerScreenShake();
            
            // Hide or destroy the player
            playerTransform.gameObject.SetActive(false);
            
            // Spawn death particle effect
            SpawnDeathParticle();
            
            // Call game over
            playerController.GameOver();
            
            Debug.Log("FlameThrower: Player death triggered!");
        }
        else
        {
            Debug.LogWarning("FlameThrower: PlayerController component not found on player!");
        }
    }
    
    void TriggerScreenShake()
    {
        // Find the camera and trigger screen shake
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CameraController cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.TriggerScreenShake();
                Debug.Log("FlameThrower: Screen shake triggered!");
            }
            else
            {
                Debug.LogWarning("FlameThrower: CameraController not found on main camera!");
            }
        }
        else
        {
            Debug.LogWarning("FlameThrower: Main camera not found!");
        }
    }
    
    void SpawnDeathParticle()
    {
        if (deathParticlePrefab != null)
        {
            // Instantiate death particle at player position
            GameObject deathParticle = Instantiate(deathParticlePrefab, playerTransform.position, Quaternion.identity);
            
            // Destroy the particle after specified lifetime
            Destroy(deathParticle, deathParticleLifetime);
            
            Debug.Log("FlameThrower: Death particle spawned!");
        }
        else
        {
            Debug.LogWarning("FlameThrower: No death particle prefab assigned!");
        }
    }
    
    // Visual debugging
    void OnDrawGizmosSelected()
    {
        // Calculate the range center relative to the flamethrower's rotation
        Vector3 rotatedOffset = transform.TransformDirection((Vector3)rangeOffset);
        Vector3 rangeCenter = transform.position + rotatedOffset;
        
        // Get the flamethrower's forward direction (right in 2D)
        Vector3 forwardDirection = transform.right;
        
        // Draw triangular range (red zone) - pointing in the flamethrower's forward direction
        Gizmos.color = Color.red;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, angle) * forwardDirection * range;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -angle) * forwardDirection * range;
        
        Gizmos.DrawLine(rangeCenter, rangeCenter + leftBoundary);
        Gizmos.DrawLine(rangeCenter, rangeCenter + rightBoundary);
        Gizmos.DrawLine(rangeCenter + leftBoundary, rangeCenter + rightBoundary);
        
        // Draw direction indicator
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(rangeCenter, forwardDirection * range * 0.5f);
        
        // Draw offset line from flamethrower to range center
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, rangeCenter);
    }
    
    // Public method to set player reference
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }
    
    // Public method to update flame thrower settings
    public void UpdateFlameThrowerSettings(float newRange, float newAngle, Vector2 newOffset)
    {
        range = newRange;
        angle = newAngle;
        rangeOffset = newOffset;
    }
    
    // Public method to reset death trigger (useful for respawning)
    public void ResetDeathTrigger()
    {
        hasTriggeredDeath = false;
    }
    
    // Public method to set active state
    public void SetActive(bool active)
    {
        isActive = active;
        
        // Reset animation state when deactivated
        if (!active)
        {
            currentSpriteIndex = 0;
            animationTimer = 0f;
            
            // Show first sprite when inactive
            if (flameSprites.Length > 0)
            {
                spriteRenderer.sprite = flameSprites[0];
            }
        }
        
        Debug.Log($"FlameThrower: Set active to {isActive}");
    }
    
    // Public getter for active state
    public bool IsActive()
    {
        return isActive;
    }
}
