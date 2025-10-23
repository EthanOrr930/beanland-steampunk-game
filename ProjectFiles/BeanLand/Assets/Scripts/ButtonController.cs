using UnityEngine;
using System.Collections.Generic;

public class ButtonController : MonoBehaviour
{
    [Header("Button Detection")]
    [Tooltip("Radius to detect physics objects")]
    public float detectionRadius = 2f;
    
    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    public Sprite pressedSprite;
    
    [Header("Target Objects")]
    [Tooltip("Objects to control when button is pressed")]
    public List<GameObject> targetObjects = new List<GameObject>();
    
    [Header("Button Behavior")]
    [Tooltip("If true, button deactivates objects when pressed and activates when unpressed")]
    public bool invertible = false;
    
    private bool isPressed = false;
    private List<Collider2D> objectsInRange = new List<Collider2D>();
    
    // Components to look for on target objects
    private readonly string[] controllableComponents = { "Steam", "FlameThrower", "Wall", "NoDashZone" };
    
    void Start()
    {
        // Get SpriteRenderer if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Setup trigger collider for detection
        SetupTriggerCollider();
        
        // Initialize visual state
        UpdateVisualState();
        
        // Set initial state of target components
        // For invertible buttons, objects should be active when button is not pressed
        SetTargetObjectsActive(false);
    }
    
    private void SetupTriggerCollider()
    {
        // Add or get existing trigger collider
        CircleCollider2D triggerCollider = GetComponent<CircleCollider2D>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        
        triggerCollider.isTrigger = true;
        triggerCollider.radius = detectionRadius;
        triggerCollider.offset = Vector2.zero;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object has a rigidbody
        if (other.attachedRigidbody != null && other.gameObject != gameObject)
        {
            if (!objectsInRange.Contains(other))
            {
                objectsInRange.Add(other);
                UpdateButtonState();
                Debug.Log($"ButtonController: Object {other.name} entered button area");
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        // Check if the object has a rigidbody
        if (other.attachedRigidbody != null && objectsInRange.Contains(other))
        {
            objectsInRange.Remove(other);
            UpdateButtonState();
            Debug.Log($"ButtonController: Object {other.name} left button area");
        }
    }
    
    private void UpdateButtonState()
    {
        bool newPressedState = objectsInRange.Count > 0;
        if (newPressedState != isPressed)
        {
            isPressed = newPressedState;
            UpdateVisualState();
            SetTargetObjectsActive(isPressed);
            
            string behavior = invertible ? "inverted" : "normal";
            Debug.Log($"ButtonController: Button {(isPressed ? "pressed" : "released")} by {objectsInRange.Count} objects ({behavior} behavior)");
        }
    }
    
    // Public methods for external control
    public void SetActive(bool active)
    {
        isPressed = active;
        UpdateVisualState();
        SetTargetObjectsActive(isPressed);
        
        string behavior = invertible ? "inverted" : "normal";
        Debug.Log($"ButtonController: Manually set to {(active ? "active" : "inactive")} ({behavior} behavior)");
    }
    
    private void SetTargetObjectsActive(bool active)
    {
        // Apply invertible logic: if invertible is true, flip the active state
        bool actualActiveState = invertible ? !active : active;
        
        foreach (GameObject targetObject in targetObjects)
        {
            if (targetObject != null)
            {
                ControlObject(targetObject, actualActiveState);
            }
        }
    }
    
    private void ControlObject(GameObject targetObject, bool active)
    {
        // Try to control specific components first
        bool foundControllableComponent = false;
        
        // Check for Wall component
        Wall wallComponent = targetObject.GetComponent<Wall>();
        if (wallComponent != null)
        {
            wallComponent.SetActive(active);
            foundControllableComponent = true;
            Debug.Log($"ButtonController: Controlled Wall on {targetObject.name}");
        }
        
        // Check for Steam component
        Steam steamComponent = targetObject.GetComponent<Steam>();
        if (steamComponent != null)
        {
            steamComponent.SetActive(active);
            foundControllableComponent = true;
            Debug.Log($"ButtonController: Controlled Steam on {targetObject.name}");
        }
        
        // Check for FlameThrower component
        FlameThrower flameComponent = targetObject.GetComponent<FlameThrower>();
        if (flameComponent != null)
        {
            flameComponent.SetActive(active);
            foundControllableComponent = true;
            Debug.Log($"ButtonController: Controlled FlameThrower on {targetObject.name}");
        }
        
        // Check for NoDashZone component
        NoDashZone noDashZoneComponent = targetObject.GetComponent<NoDashZone>();
        if (noDashZoneComponent != null)
        {
            noDashZoneComponent.SetActive(active);
            foundControllableComponent = true;
            Debug.Log($"ButtonController: Controlled NoDashZone on {targetObject.name}");
        }
        
        // If no controllable components found, enable/disable the GameObject
        if (!foundControllableComponent)
        {
            targetObject.SetActive(active);
            Debug.Log($"ButtonController: Set GameObject {targetObject.name} to {active}");
        }
    }
    
    private void UpdateVisualState()
    {
        // Update button sprite based on pressed state
        if (spriteRenderer != null)
        {
            if (isPressed && pressedSprite != null)
            {
                spriteRenderer.sprite = pressedSprite;
            }
            else if (!isPressed && normalSprite != null)
            {
                spriteRenderer.sprite = normalSprite;
            }
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = isPressed ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw connections to target objects
        if (targetObjects.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (GameObject target in targetObjects)
            {
                if (target != null)
                {
                    Gizmos.DrawLine(transform.position, target.transform.position);
                }
            }
        }
        
        // Draw connections to objects currently in range
        if (objectsInRange.Count > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (Collider2D obj in objectsInRange)
            {
                if (obj != null)
                {
                    Gizmos.DrawLine(transform.position, obj.transform.position);
                }
            }
        }
    }
    
    // Public getters
    public bool IsPressed()
    {
        return isPressed;
    }
    
    public int GetObjectsInRange()
    {
        return objectsInRange.Count;
    }
    
    public bool IsInvertible()
    {
        return invertible;
    }
    
    public void SetInvertible(bool invert)
    {
        invertible = invert;
        // Reapply current state with new invertible setting
        SetTargetObjectsActive(isPressed);
        Debug.Log($"ButtonController: Invertible behavior {(invert ? "enabled" : "disabled")}");
    }
    
    // Utility methods for managing target objects
    public void AddTargetObject(GameObject targetObject)
    {
        if (targetObject != null && !targetObjects.Contains(targetObject))
        {
            targetObjects.Add(targetObject);
            Debug.Log($"ButtonController: Added {targetObject.name} to target objects");
        }
    }
    
    public void RemoveTargetObject(GameObject targetObject)
    {
        if (targetObjects.Contains(targetObject))
        {
            targetObjects.Remove(targetObject);
            Debug.Log($"ButtonController: Removed {targetObject.name} from target objects");
        }
    }
}
