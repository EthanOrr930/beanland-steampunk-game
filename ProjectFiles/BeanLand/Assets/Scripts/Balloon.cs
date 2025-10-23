using UnityEngine;

public class Balloon : MonoBehaviour
{
    [Header("String Settings")]
    public Material StringMaterial;
    public float StringWidth = 0.05f;
    
    [Header("Particle Effect")]
    public GameObject BreakParticlePrefab;
    
    private SpringJoint2D springJoint;
    private LineRenderer lineRenderer;
    private bool jointWasConnected = true;
    
    void Start()
    {
        // Get the SpringJoint2D component attached to this GameObject
        springJoint = GetComponent<SpringJoint2D>();
        
        if (springJoint == null)
        {
            Debug.LogError("Balloon script requires a SpringJoint2D component on the same GameObject!");
            enabled = false;
            return;
        }
        
        // Create the LineRenderer for the string
        CreateStringRenderer();
        
        // Check if joint is initially connected
        jointWasConnected = springJoint.connectedBody != null;
    }
    
    void Update()
    {
        // Check if the SpringJoint2D component still exists
        if (springJoint == null)
        {
            // SpringJoint2D was removed/broken
            if (jointWasConnected)
            {
                OnJointBreak();
                jointWasConnected = false;
            }
            return;
        }
        
        // Check if the joint is still connected
        bool isCurrentlyConnected = springJoint.connectedBody != null;
        
        // If joint was connected but is now broken
        if (jointWasConnected && !isCurrentlyConnected)
        {
            OnJointBreak();
        }
        // If joint is connected, update the line
        else if (isCurrentlyConnected)
        {
            UpdateStringLine();
        }
        
        jointWasConnected = isCurrentlyConnected;
    }
    
    void CreateStringRenderer()
    {
        // Create a new GameObject for the line renderer
        GameObject lineObject = new GameObject("BalloonString");
        lineObject.transform.SetParent(transform);
        
        // Add and configure LineRenderer
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = StringMaterial;
        lineRenderer.startWidth = StringWidth;
        lineRenderer.endWidth = StringWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.sortingOrder = 1; // Make sure it renders above other objects
        
        // Initialize the line positions
        UpdateStringLine();
    }
    
    void UpdateStringLine()
    {
        if (lineRenderer == null || springJoint == null || springJoint.connectedBody == null) return;
        
        // Get anchor points in world space
        Vector3 balloonAnchor = transform.TransformPoint(springJoint.anchor);
        Vector3 connectedAnchor = springJoint.connectedBody.transform.TransformPoint(springJoint.connectedAnchor);
        
        // Set line positions
        lineRenderer.SetPosition(0, balloonAnchor);
        lineRenderer.SetPosition(1, connectedAnchor);
        
        // Show the line
        lineRenderer.enabled = true;
    }
    
    void OnJointBreak()
    {
        // Spawn particle effect if prefab is assigned
        if (BreakParticlePrefab != null)
        {
            GameObject particle = Instantiate(BreakParticlePrefab, transform.position, Quaternion.identity);
            
            // Destroy particle after 5 seconds to clean up
            Destroy(particle, 5f);
        }
        
        // Hide the line renderer
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        
        // Optional: Destroy the line renderer GameObject entirely
        // Uncomment the next line if you want to completely remove the line object
        // if (lineRenderer != null) Destroy(lineRenderer.gameObject);
    }
    
    void OnDestroy()
    {
        // Clean up the line renderer when the balloon is destroyed
        if (lineRenderer != null)
        {
            Destroy(lineRenderer.gameObject);
        }
    }
}
