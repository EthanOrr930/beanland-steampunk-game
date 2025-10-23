using UnityEngine;
using UnityEngine.SceneManagement;

public class EspressoMachine : MonoBehaviour
{
    [Header("Range Settings")]
    public float range = 3f; // Range within which player must be to complete level
    public LayerMask playerLayerMask = 1; // Default layer
    
    [Header("Visual Settings")]
    public GameObject confettiParticlePrefab; // Assign confetti particle prefab in inspector
    public float confettiLifetime = 3f; // How long the confetti effect lasts
    
    [Header("Fade Settings")]
    public float fadeDuration = 2f; // Duration of the fade effect
    
    [Header("Bob Animation Settings")]
    public float bobHeight = 0.5f; // Height of the bobbing motion
    public float bobSpeed = 2f; // Speed of the bobbing animation
    
    private Transform playerTransform;
    private bool hasTriggeredCompletion = false; // Prevent multiple completion triggers
    private bool isPlayerInRange = false;
    private Vector3 initialPosition; // Store the initial position for bobbing
    
    void Start()
    {
        // Store the initial position for bobbing animation
        initialPosition = transform.position;
        
        // Find player (you can also assign this in inspector)
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                Debug.Log("EspressoMachine: Player found and assigned");
            }
            else
            {
                Debug.LogWarning("EspressoMachine: No player found with 'Player' tag!");
            }
        }
        
        Debug.Log($"EspressoMachine: Range = {range}");
    }
    
    void Update()
    {
        CheckPlayerInRange();
        UpdateBobAnimation();
    }
    
    void UpdateBobAnimation()
    {
        // Calculate the bobbing offset using a sine wave
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        
        // Apply the bobbing to the Y position while keeping X and Z unchanged
        transform.position = new Vector3(
            initialPosition.x,
            initialPosition.y + bobOffset,
            initialPosition.z
        );
    }
    
    void CheckPlayerInRange()
    {
        if (playerTransform == null) 
        {
            Debug.Log("EspressoMachine: Player transform is null!");
            return;
        }
        
        // Calculate distance to player using the initial position for consistent range checking
        float distanceToPlayer = Vector3.Distance(initialPosition, playerTransform.position);
        bool wasInRange = isPlayerInRange;
        
        // Check if player is within range
        isPlayerInRange = distanceToPlayer <= range;
        
        // Debug logging
        if (wasInRange != isPlayerInRange)
        {
            Debug.Log($"EspressoMachine: Player range changed! In range: {isPlayerInRange}, Distance: {distanceToPlayer:F2}, Range: {range}");
        }
        
        // Only trigger completion if player is in range and we haven't already triggered it
        if (isPlayerInRange && !hasTriggeredCompletion)
        {
            TriggerLevelCompletion();
        }
    }
    
    void TriggerLevelCompletion()
    {
        hasTriggeredCompletion = true;
        
        // Get the PlayerController component
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Zero the player's velocity to stop them
            Rigidbody2D playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
            if (playerRigidbody != null)
            {
                playerRigidbody.linearVelocity = Vector2.zero;
                playerRigidbody.angularVelocity = 0f;
                Debug.Log("EspressoMachine: Player velocity zeroed!");
            }
            
            // Spawn confetti particle effect
            SpawnConfettiParticle();
            
            // Find and notify LevelManager
            LevelManager levelManager = FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                Debug.Log("EspressoMachine: Notifying LevelManager of completion!");
                levelManager.CompleteLevel();
            }
            else
            {
                // Fallback to PlayerController method if no LevelManager
                Debug.LogWarning("EspressoMachine: No LevelManager found, using PlayerController fallback");
                playerController.CompleteLevel();
            }
            
            Debug.Log("EspressoMachine: Level completion triggered!");
        }
        else
        {
            Debug.LogWarning("EspressoMachine: PlayerController component not found on player!");
        }
    }
    
    void SpawnConfettiParticle()
    {
        if (confettiParticlePrefab != null)
        {
            // Instantiate confetti particle at player position
            GameObject confettiParticle = Instantiate(confettiParticlePrefab, playerTransform.position, Quaternion.identity);
            
            // Destroy the particle after specified lifetime
            Destroy(confettiParticle, confettiLifetime);
            
            Debug.Log("EspressoMachine: Confetti particle spawned!");
        }
        else
        {
            Debug.LogWarning("EspressoMachine: No confetti particle prefab assigned!");
        }
    }
    
    // Visual debugging
    void OnDrawGizmosSelected()
    {
        // Draw range circle using initial position for consistent visualization
        Vector3 gizmoPosition = Application.isPlaying ? initialPosition : transform.position;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gizmoPosition, range);
        
        // Draw direction indicator to player if player exists
        if (playerTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(gizmoPosition, playerTransform.position);
        }
    }
    
    // Public method to set player reference
    public void SetPlayer(Transform player)
    {
        playerTransform = player;
    }
    
    // Public method to reset completion trigger (useful for respawning)
    public void ResetCompletionTrigger()
    {
        hasTriggeredCompletion = false;
    }
    
    // Public getter for completion state
    public bool HasTriggeredCompletion()
    {
        return hasTriggeredCompletion;
    }
    
}
