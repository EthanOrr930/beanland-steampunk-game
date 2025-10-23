using UnityEngine;

public class Wall : MonoBehaviour
{
    [Header("Wall Settings")]
    // Controlled directly by ButtonController
    
    private Collider2D wallCollider;
    private SpriteRenderer wallRenderer;
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeWall();
    }
    
    private void InitializeWall()
    {
        // Get the collider
        wallCollider = GetComponent<Collider2D>();
        if (wallCollider == null)
        {
            Debug.LogError($"Wall: No Collider2D found on {gameObject.name}. Wall requires a Collider2D component.");
            return;
        }
        
        // Get the sprite renderer
        wallRenderer = GetComponent<SpriteRenderer>();
        if (wallRenderer == null)
        {
            Debug.LogError($"Wall: No SpriteRenderer found on {gameObject.name}. Wall requires a SpriteRenderer component.");
            return;
        }
        
        // Start with wall enabled (visible and solid)
        wallCollider.enabled = true;
        wallRenderer.enabled = true;
        
        isInitialized = true;
        Debug.Log($"Wall: Initialized {gameObject.name}");
    }
    
    // Method called by ButtonController to control this wall
    public void SetActive(bool active)
    {
        if (!isInitialized) return;
        
        // When button is pressed (active = true), disable wall (make it passable and invisible)
        // When button is not pressed (active = false), enable wall (make it solid and visible)
        bool shouldBeEnabled = !active;
        
        if (wallCollider != null)
        {
            wallCollider.enabled = shouldBeEnabled;
        }
        
        if (wallRenderer != null)
        {
            wallRenderer.enabled = shouldBeEnabled;
        }
        
        Debug.Log($"Wall: {gameObject.name} set to {(active ? "disabled (passable/invisible)" : "enabled (solid/visible)")} by ButtonController");
    }
    
    // Public methods for manual control
    public void SetActiveManual(bool active)
    {
        if (wallCollider != null)
        {
            wallCollider.enabled = active;
        }
        
        if (wallRenderer != null)
        {
            wallRenderer.enabled = active;
        }
        
        Debug.Log($"Wall: {gameObject.name} manually set to {(active ? "active" : "inactive")}");
    }
    
    public bool IsActive()
    {
        return wallCollider != null && wallCollider.enabled;
    }
}
