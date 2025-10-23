using UnityEngine;

public class GroundsParticle : MonoBehaviour
{
    private PlayerController playerController;
    private Transform playerTransform;
    private Rigidbody2D rb;
    private Vector3 initialPosition;
    
    // Detachment state
    private bool isDetached = false;
    private float detachmentTimer = 0f;
    private const float REATTACH_TIME = 15f; // Seconds until particle reattaches
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 1f; // Allow gravity and physics for grounds particles
    }
    
    public void Initialize(PlayerController controller, Transform player)
    {
        playerController = controller;
        playerTransform = player;
        initialPosition = transform.position;
        isDetached = false;
        detachmentTimer = 0f;
    }
    
    public void UpdateBehavior()
    {
        if (playerController == null || playerTransform == null) return;
        
        // Handle detached state
        if (isDetached)
        {
            detachmentTimer += Time.deltaTime;
            
            // Check if it's time to reattach
            if (detachmentTimer >= REATTACH_TIME)
            {
                Reattach();
            }
            
            // Detached particles just behave as physics objects, no attraction
            return;
        }
        
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // If particle is too far away, teleport it back to a closer range
        if (distanceToPlayer > playerController.teleportDistance)
        {
            TeleportToCloserRange();
            return;
        }
        
        // If particle is within stop range, resist the attractive force to prevent overshooting
        if (distanceToPlayer <= playerController.stopRange)
        {
            ResistAttractiveForce(directionToPlayer, distanceToPlayer);
            return;
        }
        
        // Apply attraction force proportional to distance
        if (distanceToPlayer > 0.1f) // Avoid division by zero
        {
            Vector3 attractionForce = directionToPlayer.normalized * playerController.attractionForce * (distanceToPlayer / playerController.maxDistance);
            rb.AddForce(attractionForce, ForceMode2D.Force);
        }
    }
    
    void TeleportToCloserRange()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        Vector3 teleportPosition = playerTransform.position + directionToPlayer.normalized * playerController.maxDistance * 0.8f;
        transform.position = teleportPosition;
        
        // Reset velocity when teleporting
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    
    void ResistAttractiveForce(Vector3 directionToPlayer, float distanceToPlayer)
    {
        // Apply normal attractive force
        Vector3 attractiveForce = directionToPlayer.normalized * playerController.attractionForce * (distanceToPlayer / playerController.maxDistance);
        rb.AddForce(attractiveForce, ForceMode2D.Force);
        
        // Clamp velocity based on distance to player
        ClampVelocityByDistance(distanceToPlayer);
    }
    
    void ClampVelocityByDistance(float distanceToPlayer)
    {
        // Calculate interpolation factor based on how close we are to the player
        // Closer to player = lower max velocity
        float distanceRatio = Mathf.Clamp01(distanceToPlayer / playerController.stopRange);
        
        // Calculate max velocity based on distance (closer = slower)
        float maxVelocity = Mathf.Lerp(playerController.velocityLimitMin, playerController.velocityLimitMax, distanceRatio);
        
        // Clamp the velocity if it exceeds the maximum for this distance
        if (rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
    }
    
    // Make sure particles count as player for collision detection
    void OnTriggerEnter2D(Collider2D other)
    {
        // This method can be used for any special collision handling if needed
        // The particle already has the "Player" tag so flamethrower/steam should detect it
    }
    
    // Detachment methods
    public void Detach()
    {
        if (isDetached) return; // Already detached
        
        isDetached = true;
        detachmentTimer = 0f;
        
        Debug.Log("GroundsParticle: Detached from player");
    }
    
    public void Reattach()
    {
        if (!isDetached) return; // Already attached
        
        isDetached = false;
        detachmentTimer = 0f;
        
        Debug.Log("GroundsParticle: Reattached to player");
    }
    
    public bool IsDetached()
    {
        return isDetached;
    }
}
