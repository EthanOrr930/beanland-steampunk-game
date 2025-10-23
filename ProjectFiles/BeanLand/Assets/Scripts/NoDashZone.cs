using UnityEngine;

public class NoDashZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public bool isActive = true;
    public float animationSpeed = 2f;
    // Base size is 1x1, scaled by transform.localScale
    
    [Header("Visual Effects")]
    public Sprite forcefieldSprite;
    public SpriteRenderer forcefieldRenderer;
    public Material forcefieldMaterial;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    private PlayerController playerController;
    private bool wasPlayerInZone = false;
    private bool isProcessingZoneChange = false; // Prevent re-entrant calls
    private bool isInitialized = false;
    private float initializationDelay = 0.5f; // Wait before starting zone checks
    private float initTimer = 0f;
    
    void Start()
    {
        // Setup forcefield visual first
        SetupForcefieldVisual();
    }
    
    void LateUpdate()
    {
        // Delay initialization to ensure scene is fully loaded
        if (!isInitialized)
        {
            initTimer += Time.deltaTime;
            if (initTimer >= initializationDelay)
            {
                // Find player controller after delay
                playerController = FindFirstObjectByType<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogError("NoDashZone: PlayerController not found in scene!");
                }
                isInitialized = true;
            }
            return;
        }
    }
    
    void Update()
    {
        // Always animate the forcefield, even before initialization
        if (isActive && forcefieldRenderer != null)
        {
            AnimateForcefield();
        }
        
        // Only check zone after initialization is complete
        if (!isInitialized) return;
        
        // Check if player is in zone mathematically
        // Add extra safety checks to prevent issues during loading
        if (isActive && playerController != null && playerController.gameObject != null)
        {
            CheckPlayerInZone();
        }
    }
    
    void CheckPlayerInZone()
    {
        // Prevent re-entrant calls to avoid infinite recursion
        if (isProcessingZoneChange) return;
        
        bool isPlayerInZone = IsPointInZone(playerController.transform.position);
        
        // Detect state changes
        if (isPlayerInZone && !wasPlayerInZone)
        {
            // Player entered
            isProcessingZoneChange = true;
            
            if (showDebugInfo)
            {
                Debug.Log("NoDashZone: Player entered zone");
            }
            playerController.SetNoDashZone(this, true);
            
            isProcessingZoneChange = false;
        }
        else if (!isPlayerInZone && wasPlayerInZone)
        {
            // Player exited
            isProcessingZoneChange = true;
            
            if (showDebugInfo)
            {
                Debug.Log("NoDashZone: Player exited zone");
            }
            playerController.SetNoDashZone(this, false);
            
            isProcessingZoneChange = false;
        }
        
        wasPlayerInZone = isPlayerInZone;
    }
    
    bool IsPointInZone(Vector3 point)
    {
        // Calculate zone bounds based on 1x1 base size scaled by transform
        Vector3 zoneCenter = transform.position;
        Vector3 scale = transform.localScale;
        
        // Half extents of the zone (1x1 base * scale / 2)
        float halfWidth = scale.x * 0.5f;
        float halfHeight = scale.y * 0.5f;
        
        // Check if point is within bounds
        return Mathf.Abs(point.x - zoneCenter.x) <= halfWidth &&
               Mathf.Abs(point.y - zoneCenter.y) <= halfHeight;
    }
    
    void SetupForcefieldVisual()
    {
        // Get or create sprite renderer on this object
        if (forcefieldRenderer == null)
        {
            forcefieldRenderer = GetComponent<SpriteRenderer>();
            if (forcefieldRenderer == null)
            {
                forcefieldRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        // Set sprite if provided
        if (forcefieldSprite != null)
        {
            forcefieldRenderer.sprite = forcefieldSprite;
        }
        
        // Set initial color to white (material handles the actual color)
        forcefieldRenderer.color = Color.white;
        forcefieldRenderer.sortingOrder = -1; // Behind other objects
        
        // Apply forcefield material if provided
        if (forcefieldMaterial != null)
        {
            forcefieldRenderer.material = forcefieldMaterial;
        }
        
        // Respect the isActive state for visibility
        forcefieldRenderer.enabled = isActive;
    }
    
    
    void AnimateForcefield()
    {
        if (forcefieldRenderer == null) return;
        
        // The UV scrolling and alpha pulsing are now handled by the shader
        // No additional animation needed in script
    }
    
    // Public methods for external control
    public void SetActive(bool active)
    {
        isActive = active;
        if (forcefieldRenderer != null)
        {
            forcefieldRenderer.enabled = active;
        }
        
        // Only interact with player if initialized
        if (isInitialized && !active && wasPlayerInZone && playerController != null && !isProcessingZoneChange)
        {
            isProcessingZoneChange = true;
            playerController.SetNoDashZone(this, false);
            wasPlayerInZone = false;
            isProcessingZoneChange = false;
        }
    }
    
    
    public bool IsPlayerInZone()
    {
        if (!isInitialized || playerController == null) return false;
        return IsPointInZone(playerController.transform.position);
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    // Method to manually trigger zone effects (useful for scripting)
    public void TriggerZoneEffect()
    {
        if (isActive && forcefieldRenderer != null)
        {
            // Flash effect
            StartCoroutine(FlashEffect());
        }
    }
    
    System.Collections.IEnumerator FlashEffect()
    {
        Color currentColor = forcefieldRenderer.color;
        float flashDuration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float flashIntensity = Mathf.Sin((elapsed / flashDuration) * Mathf.PI);
            Color flashColor = Color.white;
            flashColor.a = currentColor.a * flashIntensity;
            forcefieldRenderer.color = flashColor;
            yield return null;
        }
        
        forcefieldRenderer.color = currentColor;
    }
    
}
