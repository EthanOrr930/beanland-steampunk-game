using UnityEngine;

public class Grinder : MonoBehaviour
{
    [Header("Grinder Settings")]
    public float range = 5f;
    public float angle = 45f; // Half angle of the triangular range
    public Vector2 rangeOffset = Vector2.zero; // Offset for the range position
    public LayerMask playerLayerMask = 1; // Default layer
    
    [Header("Grinding Settings")]
    public float grindForce = 15f;
    public float grindDuration = 1f; // How long the grinding animation lasts
    public Transform exitPoint; // The transform.up of the first child object for exit direction
    
    [Header("Animation Settings")]
    public Sprite[] grindSprites;
    public float animationSpeed = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentSpriteIndex = 0;
    private Transform playerTransform;
    private bool isPlayerInRange = false;
    private bool isAnimating = false;
    private bool hasPlayedFullAnimation = false;
    private bool hasTriggeredGrind = false; // Prevent multiple grind triggers
    private bool isGrinding = false; // Track if currently grinding
    private float grindTimer = 0f; // Timer for grind duration
    private PlayerController playerController;
    private bool wasPlayerFrozen = false; // Track if player was frozen before grinding
    private bool wasPlayerSpriteEnabled = false; // Track if player sprite was enabled before grinding
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set initial sprite if available
        if (grindSprites.Length > 0)
        {
            spriteRenderer.sprite = grindSprites[0];
            Debug.Log($"Grinder: Initialized with {grindSprites.Length} sprites");
        }
        else
        {
            Debug.LogWarning("Grinder: No sprites assigned!");
        }
        
        spriteRenderer.color = Color.gray;
        
        // Find player (you can also assign this in inspector)
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player.GetComponent<PlayerController>();
                Debug.Log("Grinder: Player found and assigned");
            }
            else
            {
                Debug.LogWarning("Grinder: No player found with 'Player' tag!");
            }
        }
        
        // Set up exit point if not assigned
        if (exitPoint == null && transform.childCount > 0)
        {
            exitPoint = transform.GetChild(0);
            Debug.Log("Grinder: Using first child as exit point");
        }
        
        Debug.Log($"Grinder: Range = {range}, Angle = {angle}, Grind Force = {grindForce}");
    }
    
    void Update()
    {
        AnimateSprites();
        CheckAndTriggerGrind();
        UpdateGrinding();
    }
    
    void AnimateSprites()
    {
        if (grindSprites.Length == 0) return;
        
        // If player is not in range and we haven't played full animation, show sprite[0] and reset
        if (!isPlayerInRange && !isAnimating)
        {
            if (hasPlayedFullAnimation)
            {
                hasPlayedFullAnimation = false;
                currentSpriteIndex = 0;
                spriteRenderer.sprite = grindSprites[0];
            }
            return;
        }
        
        // If player just entered range, start animation
        if (!isAnimating && isPlayerInRange && !isGrinding)
        {
            isAnimating = true;
            hasPlayedFullAnimation = false;
            currentSpriteIndex = 0;
            animationTimer = 0f;
            Debug.Log("Grinder animation started!");
        }
        
        // Only animate if we have more than one sprite and we're animating
        if (grindSprites.Length > 1 && isAnimating)
        {
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                currentSpriteIndex++;
                
                // If we've gone through all sprites, mark as complete and return to sprite[0]
                if (currentSpriteIndex >= grindSprites.Length)
                {
                    currentSpriteIndex = 0;
                    hasPlayedFullAnimation = true;
                    isAnimating = false;
                    Debug.Log("Grinder animation completed!");
                }
                
                spriteRenderer.sprite = grindSprites[currentSpriteIndex];
            }
        }
    }
    
    void CheckAndTriggerGrind()
    {
        if (playerTransform == null || isGrinding) 
        {
            return;
        }
        
        // Calculate the range center relative to the grinder's position
        Vector3 rotatedOffset = transform.TransformDirection((Vector3)rangeOffset);
        Vector3 grindPosition = transform.position + rotatedOffset;
        Vector3 playerPosition = playerTransform.position;
        Vector3 directionToPlayer = playerPosition - grindPosition;
        
        // Check if player is within range
        float distanceToPlayer = directionToPlayer.magnitude;
        bool wasInRange = isPlayerInRange;
        
        // Check if player is within the triangular range (angle check) - pointing in grinder's up direction
        Vector3 forwardDirection = transform.up;
        float angleToPlayer = Vector3.Angle(forwardDirection, directionToPlayer);
        bool isInAngleRange = angleToPlayer <= angle;
        
        // Player is in range only if both distance and angle conditions are met
        isPlayerInRange = distanceToPlayer <= range && isInAngleRange;
        
        // Debug logging
        if (wasInRange != isPlayerInRange)
        {
            Debug.Log($"Grinder: Player range changed! In range: {isPlayerInRange}, Distance: {distanceToPlayer:F2}, Range: {range}, Angle: {angleToPlayer:F1}, Max Angle: {angle}");
        }
        
        if (!isPlayerInRange) return;
        
        // Trigger grind if player is in range, we haven't already triggered it, and player is in WholeBean state
        if (!hasTriggeredGrind && playerController != null && playerController.GetCurrentState() == PlayerState.WholeBean)
        {
            StartGrinding();
        }
    }
    
    void StartGrinding()
    {
        hasTriggeredGrind = true;
        isGrinding = true;
        grindTimer = 0f;
        
        // Freeze the player during grinding
        if (playerController != null)
        {
            // Store original state
            wasPlayerFrozen = (playerController.GetComponent<Rigidbody2D>().bodyType == RigidbodyType2D.Kinematic);
            wasPlayerSpriteEnabled = playerController.GetComponent<SpriteRenderer>().enabled;
            
            // Freeze the player
            Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.bodyType = RigidbodyType2D.Kinematic;
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
            }
            
            // Disable player sprite during grinding
            SpriteRenderer playerSprite = playerController.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                playerSprite.enabled = false;
            }
            
            Debug.Log("Grinder: Player frozen and sprite disabled!");
        }
        
        // Start screen shake during grinding with custom settings
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            CameraController cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController != null)
            {
                cameraController.TriggerScreenShake(0.05f, grindDuration);
            }
        }
        
        Debug.Log("Grinder: Grinding started!");
    }
    
    void UpdateGrinding()
    {
        if (!isGrinding) return;
        
        grindTimer += Time.deltaTime;
        
        // If grinding duration is complete, launch the player
        if (grindTimer >= grindDuration)
        {
            LaunchPlayer();
            isGrinding = false;
        }
    }
    
    void LaunchPlayer()
    {
        if (playerTransform == null || playerController == null) return;
        
        // Set beGrounds to true to switch to grounds state
        playerController.beGrounds = true;
        
        // Unfreeze the player and restore sprite
        Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        SpriteRenderer playerSprite = playerController.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            playerSprite.enabled = wasPlayerSpriteEnabled;
        }
        
        // Calculate exit direction (transform.up of first child or transform.right as fallback)
        Vector3 exitDirection = Vector3.right; // Default direction
        if (exitPoint != null)
        {
            exitDirection = exitPoint.up;
        }
        else if (transform.childCount > 0)
        {
            exitDirection = transform.GetChild(0).up;
        }
        
        // Apply launch force to all grounds particles
        Rigidbody2D[] particleRigidbodies = playerTransform.GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rb in particleRigidbodies)
        {
            if (rb != null)
            {
                rb.AddForce(exitDirection * grindForce, ForceMode2D.Impulse);
            }
        }
        
        // Also apply force to the main player rigidbody if it exists
        if (playerRb != null)
        {
            playerRb.AddForce(exitDirection * grindForce, ForceMode2D.Impulse);
        }
        
        Debug.Log($"Grinder: Player launched with force {grindForce} in direction {exitDirection}!");
    }
    
    
    
    // Visual debugging
    void OnDrawGizmosSelected()
    {
        // Calculate the range center relative to the grinder's position
        Vector3 rotatedOffset = transform.TransformDirection((Vector3)rangeOffset);
        Vector3 rangeCenter = transform.position + rotatedOffset;
        
        // Draw triangular range (red zone) - pointing in the grinder's up direction
        Gizmos.color = Color.red;
        Vector3 forwardDirection = transform.up;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, angle) * forwardDirection * range;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -angle) * forwardDirection * range;
        
        Gizmos.DrawLine(rangeCenter, rangeCenter + leftBoundary);
        Gizmos.DrawLine(rangeCenter, rangeCenter + rightBoundary);
        Gizmos.DrawLine(rangeCenter + leftBoundary, rangeCenter + rightBoundary);
        
        // Draw offset line from grinder to range center
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, rangeCenter);
        
        // Draw direction indicator
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(rangeCenter, forwardDirection * range * 0.5f);
        
        // Draw exit direction
        Gizmos.color = Color.green;
        Vector3 exitDirection = Vector3.right; // Default
        if (exitPoint != null)
        {
            exitDirection = exitPoint.up;
        }
        else if (transform.childCount > 0)
        {
            exitDirection = transform.GetChild(0).up;
        }
        Gizmos.DrawRay(rangeCenter, exitDirection * range * 0.5f);
    }
    
    // Public method to set player reference
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }
    }
    
    // Public method to update grinder settings
    public void UpdateGrinderSettings(float newRange, float newAngle, Vector2 newOffset, float newGrindForce)
    {
        range = newRange;
        angle = newAngle;
        rangeOffset = newOffset;
        grindForce = newGrindForce;
    }
    
    // Public method to reset grind trigger (useful for respawning)
    public void ResetGrindTrigger()
    {
        hasTriggeredGrind = false;
        isGrinding = false;
        grindTimer = 0f;
        
        // Restore player state if they were frozen
        if (playerController != null)
        {
            Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.bodyType = RigidbodyType2D.Dynamic;
            }
            
            SpriteRenderer playerSprite = playerController.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                playerSprite.enabled = wasPlayerSpriteEnabled;
            }
        }
    }
    
    // Public method to set exit point
    public void SetExitPoint(Transform exit)
    {
        exitPoint = exit;
    }
}
