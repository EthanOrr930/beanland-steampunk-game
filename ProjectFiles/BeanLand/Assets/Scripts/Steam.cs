using UnityEngine;

public class Steam : MonoBehaviour
{
    [Header("Steam Force Settings")]
    public float blowForce = 10f;
    public float range = 5f;
    public float angle = 45f; // Half angle of the triangular range
    public LayerMask playerLayerMask = 1; // Default layer
    
    [Header("Animation Settings")]
    public Sprite[] steamSprites;
    public float animationSpeed = 0.1f;
    
    [Header("Visual Settings")]
    public Color steamColor = Color.white;
    
    [Header("Activation Settings")]
    public bool isActive = true; // Controls whether the steam is active
    
    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentSpriteIndex = 0;
    private Transform playerTransform;
    private bool isPlayerInRange = false;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set initial sprite if available
        if (steamSprites.Length > 0)
        {
            spriteRenderer.sprite = steamSprites[0];
            Debug.Log($"Steam: Initialized with {steamSprites.Length} sprites");
        }
        else
        {
            Debug.LogWarning("Steam: No sprites assigned!");
        }
        
        spriteRenderer.color = steamColor;
        
        // Find player (you can also assign this in inspector)
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("Steam: Player found and assigned");
            }
            else
            {
                Debug.LogWarning("Steam: No player found with 'Player' tag!");
            }
        }
        
        Debug.Log($"Steam: Range = {range}, Animation Speed = {animationSpeed}");
    }
    
    void Update()
    {
        // Only update if steam is active
        if (!isActive) return;
        
        AnimateSprites();
        CheckAndApplyBlowForce();
    }
    
    void AnimateSprites()
    {
        if (steamSprites.Length == 0) return;
        
        // If steam is not active, show first sprite and stop animating
        if (!isActive)
        {
            currentSpriteIndex = 0;
            spriteRenderer.sprite = steamSprites[0];
            return;
        }
        
        // If steam is active, continuously animate
        if (steamSprites.Length > 1)
        {
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                currentSpriteIndex++;
                
                // Loop back to the beginning when we reach the end
                if (currentSpriteIndex >= steamSprites.Length)
                {
                    currentSpriteIndex = 0;
                }
                
                spriteRenderer.sprite = steamSprites[currentSpriteIndex];
            }
        }
    }
    
    void CheckAndApplyBlowForce()
    {
        if (playerTransform == null) 
        {
            Debug.Log("Steam: Player transform is null!");
            return;
        }
        
        Vector3 steamPosition = transform.position;
        Vector3 playerPosition = playerTransform.position;
        Vector3 directionToPlayer = playerPosition - steamPosition;
        
        // Check if player is within range
        float distanceToPlayer = directionToPlayer.magnitude;
        bool wasInRange = isPlayerInRange;
        
        // Check if player is within the triangular range (angle check)
        float angleToPlayer = Vector3.Angle(transform.up, directionToPlayer);
        bool isInAngleRange = angleToPlayer <= angle;
        
        // Player is in range only if both distance and angle conditions are met
        isPlayerInRange = distanceToPlayer <= range && isInAngleRange;
        
        // Debug logging
        if (wasInRange != isPlayerInRange)
        {
            Debug.Log($"Steam: Player range changed! In range: {isPlayerInRange}, Distance: {distanceToPlayer:F2}, Range: {range}, Angle: {angleToPlayer:F1}, Max Angle: {angle}");
        }
        
        // Only apply force if player is in range and steam is active
        if (!isPlayerInRange || !isActive) return;
        
        // Apply blow force
        ApplyBlowForce(directionToPlayer, distanceToPlayer);
    }
    
    void ApplyBlowForce(Vector3 direction, float distance)
    {
        // Normalize direction
        direction.Normalize();
        
        // Calculate force strength based on distance (closer = stronger)
        float distanceMultiplier = 1f - (distance / range);
        float finalForce = blowForce * distanceMultiplier;
        
        // Apply force to player
        Rigidbody2D playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
        {
            playerRigidbody.AddForce(direction * finalForce, ForceMode2D.Force);
        }
    }
    
    // Visual debugging
    void OnDrawGizmosSelected()
    {
        // Draw triangular range (red zone)
        Gizmos.color = Color.red;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, angle) * transform.up * range;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -angle) * transform.up * range;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        Gizmos.DrawLine(transform.position + leftBoundary, transform.position + rightBoundary);
        
        // Draw direction indicator
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.up * range * 0.5f);
    }
    
    // Public method to set player reference
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }
    
    // Public method to update steam settings
    public void UpdateSteamSettings(float newBlowForce, float newRange, float newAngle)
    {
        blowForce = newBlowForce;
        range = newRange;
        angle = newAngle;
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
            if (steamSprites.Length > 0)
            {
                spriteRenderer.sprite = steamSprites[0];
            }
        }
        
        Debug.Log($"Steam: Set active to {isActive}");
    }
    
    // Public getter for active state
    public bool IsActive()
    {
        return isActive;
    }
}
