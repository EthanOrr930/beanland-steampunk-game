using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ParallaxBackground script that uses existing 2D sprite GameObjects as layers.
/// The Z position of each GameObject determines its distance from the camera for parallax effect.
/// Simply assign existing GameObjects to the parallaxLayers list and they will automatically
/// be used as parallax layers based on their Z positions.
/// </summary>

[System.Serializable]
public class ParallaxLayer
{
    [Header("Layer Settings")]
    public string layerName = "Layer";
    [Tooltip("Assign an existing GameObject with a SpriteRenderer component. The Z position will be used for parallax distance calculation.")]
    public GameObject layerObject; // Reference to existing GameObject
    public Color tint = Color.white;
    public Vector2 scale = Vector2.one; // Custom scale for this layer
    
    [Header("Movement Settings")]
    public bool moveWithX = true;
    public bool moveWithY = true;
    public float xMultiplier = 1f;
    public float yMultiplier = 1f;
    
    
    // Internal variables
    [HideInInspector]
    public SpriteRenderer spriteRenderer;
    [HideInInspector]
    public Vector3 initialPosition;
    [HideInInspector]
    public Vector3 initialScale;
    [HideInInspector]
    public float distanceFromCamera; // Calculated from Z position
}

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    public List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();
    public Transform target; // Usually the camera or player
    public float baseDistance = 10f; // Reference distance for parallax calculation
    
    
    [Header("Performance Settings")]
    public bool updateInFixedUpdate = false;
    public int updateFrequency = 1; // Update every N frames
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    // Private variables
    private Vector3 lastTargetPosition;
    private int frameCounter = 0;
    private Camera cam;
    
    void Start()
    {
        // Get camera reference
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
        
        // If no target is assigned, use the camera itself
        if (target == null)
        {
            target = transform;
        }
        
        // Sort layers by distance (closer layers first)
        SortLayersByDistance();
        
        // Initialize layers
        InitializeLayers();
        
        // Store initial target position
        lastTargetPosition = target.position;
    }
    
    void SortLayersByDistance()
    {
        // Sort layers by distance (closer layers first for proper rendering order)
        parallaxLayers.Sort((a, b) => a.distanceFromCamera.CompareTo(b.distanceFromCamera));
    }
    
    void InitializeLayers()
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            SetupExistingLayerObject(layer);
        }
    }
    
    void SetupExistingLayerObject(ParallaxLayer layer)
    {
        // Validate that the layer object exists
        if (layer.layerObject == null)
        {
            Debug.LogError($"ParallaxLayer '{layer.layerName}' has no GameObject assigned!");
            return;
        }
        
        // Get or add SpriteRenderer component
        layer.spriteRenderer = layer.layerObject.GetComponent<SpriteRenderer>();
        if (layer.spriteRenderer == null)
        {
            layer.spriteRenderer = layer.layerObject.AddComponent<SpriteRenderer>();
        }
        
        // Calculate distance from camera based on Z position
        layer.distanceFromCamera = Mathf.Abs(layer.layerObject.transform.position.z - transform.position.z);
        
        // Store initial position and scale
        layer.initialPosition = layer.layerObject.transform.position;
        layer.initialScale = layer.layerObject.transform.localScale;
        
        // Set up sprite renderer properties
        SetupSpriteRenderer(layer, layer.spriteRenderer);
    }
    
    void SetupSpriteRenderer(ParallaxLayer layer, SpriteRenderer renderer)
    {
        // Apply color tint (preserve existing sprite)
        renderer.color = layer.tint;
        
        // Set sorting order based on distance (closer layers render in front)
        int layerIndex = parallaxLayers.IndexOf(layer);
        renderer.sortingOrder = -10 * (1 + layerIndex);
        
        
        // Apply custom scale if specified
        if (layer.scale != Vector2.one)
        {
            Vector3 currentScale = layer.layerObject.transform.localScale;
            layer.layerObject.transform.localScale = new Vector3(
                currentScale.x * layer.scale.x,
                currentScale.y * layer.scale.y,
                currentScale.z
            );
        }
    }
    
    
    
    void Update()
    {
        if (updateInFixedUpdate) return;
        
        frameCounter++;
        if (frameCounter >= updateFrequency)
        {
            UpdateParallax();
            frameCounter = 0;
        }
    }
    
    void FixedUpdate()
    {
        if (updateInFixedUpdate)
        {
            frameCounter++;
            if (frameCounter >= updateFrequency)
            {
                UpdateParallax();
                frameCounter = 0;
            }
        }
    }
    
    void UpdateParallax()
    {
        if (target == null) return;
        
        Vector3 currentTargetPosition = target.position;
        Vector3 deltaPosition = currentTargetPosition - lastTargetPosition;
        
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            if (layer.layerObject == null) continue;
            
            // Update distance from camera based on current Z position
            layer.distanceFromCamera = Mathf.Abs(layer.layerObject.transform.position.z - transform.position.z);
            
            // Calculate parallax factor based on distance
            float parallaxFactor = baseDistance / layer.distanceFromCamera;
            
            // Calculate movement offset
            Vector3 offset = Vector3.zero;
            
            if (layer.moveWithX)
            {
                offset.x = deltaPosition.x * parallaxFactor * layer.xMultiplier;
            }
            
            if (layer.moveWithY)
            {
                offset.y = deltaPosition.y * parallaxFactor * layer.yMultiplier;
            }
            
            // Apply offset to layer position
            layer.layerObject.transform.position += offset;
            
        }
        
        lastTargetPosition = currentTargetPosition;
    }
    
    
    
    
    // Public methods for runtime control
    public void AddLayer(ParallaxLayer newLayer)
    {
        parallaxLayers.Add(newLayer);
        SortLayersByDistance();
        SetupExistingLayerObject(newLayer);
    }
    
    public void RemoveLayer(int index)
    {
        if (index >= 0 && index < parallaxLayers.Count)
        {
            // Note: We don't destroy the GameObject since it's an existing object
            // The user is responsible for managing the GameObject lifecycle
            parallaxLayers.RemoveAt(index);
        }
    }
    
    public void UpdateLayerTint(int layerIndex, Color newTint)
    {
        if (layerIndex >= 0 && layerIndex < parallaxLayers.Count)
        {
            ParallaxLayer layer = parallaxLayers[layerIndex];
            layer.tint = newTint;
            
            if (layer.spriteRenderer != null)
            {
                layer.spriteRenderer.color = newTint;
            }
        }
    }
    
    public void UpdateLayerScale(int layerIndex, Vector2 newScale)
    {
        if (layerIndex >= 0 && layerIndex < parallaxLayers.Count)
        {
            ParallaxLayer layer = parallaxLayers[layerIndex];
            layer.scale = newScale;
            
            // Apply new scale to the layer
            if (layer.layerObject != null)
            {
                Vector3 currentScale = layer.layerObject.transform.localScale;
                layer.layerObject.transform.localScale = new Vector3(
                    currentScale.x * layer.scale.x,
                    currentScale.y * layer.scale.y,
                    currentScale.z
                );
            }
        }
    }
    
    
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }
    
    public void ReSortLayers()
    {
        SortLayersByDistance();
        
        // Update sorting orders for all existing layers
        for (int i = 0; i < parallaxLayers.Count; i++)
        {
            ParallaxLayer layer = parallaxLayers[i];
            if (layer.spriteRenderer != null)
            {
                layer.spriteRenderer.sortingOrder = -10 * (1 + i);
            }
        }
    }
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (showDebugInfo && parallaxLayers != null)
        {
            Gizmos.color = Color.cyan;
            
            foreach (ParallaxLayer layer in parallaxLayers)
            {
                if (layer.layerObject != null)
                {
                    Vector3 pos = layer.layerObject.transform.position;
                    
                    // Draw wireframe of layer bounds
                    if (cam != null)
                    {
                        float cameraHeight = 2.0f * cam.orthographicSize;
                        float cameraWidth = cameraHeight * cam.aspect;
                        Gizmos.DrawWireCube(pos, new Vector3(cameraWidth, cameraHeight, 0.1f));
                    }
                    
                    // Draw distance indicator
                    Gizmos.DrawLine(transform.position, pos);
                }
            }
        }
    }
}
