using UnityEngine;
using System.Collections;

public class AnimatedSprite : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("List of Texture2Ds to animate through")]
    public Texture2D[] animationFrames;
    
    [Tooltip("Frames per second for the animation")]
    [Range(1f, 60f)]
    public float frameRate = 12f;
    
    [Tooltip("Whether the animation should loop continuously")]
    public bool loop = true;
    
    [Tooltip("Whether to play the animation automatically on start")]
    public bool playOnStart = true;
    
    [Tooltip("Whether to randomize the starting frame")]
    public bool randomizeStartFrame = false;
    
    [Header("Animation Control")]
    [Tooltip("Current frame index (read-only in play mode)")]
    [SerializeField]
    private int currentFrameIndex = 0;
    
    [Tooltip("Whether the animation is currently playing")]
    [SerializeField]
    private bool isPlaying = false;
    
    [Tooltip("Whether the animation is paused")]
    [SerializeField]
    private bool isPaused = false;
    
    [Header("Sprite Renderer")]
    [Tooltip("SpriteRenderer component to use. If null, will try to find one on this GameObject")]
    public SpriteRenderer spriteRenderer;
    
    // Private variables
    private Coroutine animationCoroutine;
    private float frameInterval;
    private int totalFrames;
    
    // Events
    public System.Action OnAnimationComplete;
    public System.Action<int> OnFrameChanged;
    
    void Start()
    {
        // Get or add SpriteRenderer component
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        // Initialize animation
        InitializeAnimation();
        
        // Start playing if configured to do so
        if (playOnStart && animationFrames != null && animationFrames.Length > 0)
        {
            Play();
        }
    }
    
    void InitializeAnimation()
    {
        if (animationFrames == null || animationFrames.Length == 0)
        {
            Debug.LogWarning("AnimatedSprite: No animation frames assigned!");
            return;
        }
        
        totalFrames = animationFrames.Length;
        frameInterval = 1f / frameRate;
        
        // Randomize start frame if enabled
        if (randomizeStartFrame)
        {
            currentFrameIndex = Random.Range(0, totalFrames);
        }
        
        // Set initial frame
        UpdateSprite();
    }
    
    void UpdateSprite()
    {
        if (spriteRenderer == null || animationFrames == null || currentFrameIndex >= animationFrames.Length)
            return;
        
        // Convert Texture2D to Sprite
        Texture2D currentTexture = animationFrames[currentFrameIndex];
        if (currentTexture != null)
        {
            Sprite sprite = Sprite.Create(
                currentTexture,
                new Rect(0, 0, currentTexture.width, currentTexture.height),
                new Vector2(0.5f, 0.5f), // Pivot at center
                100f // Pixels per unit
            );
            
            spriteRenderer.sprite = sprite;
            OnFrameChanged?.Invoke(currentFrameIndex);
        }
    }
    
    IEnumerator Animate()
    {
        while (isPlaying)
        {
            if (!isPaused)
            {
                // Update to next frame
                currentFrameIndex++;
                
                // Check if we've reached the end
                if (currentFrameIndex >= totalFrames)
                {
                    if (loop)
                    {
                        currentFrameIndex = 0; // Loop back to start
                    }
                    else
                    {
                        // Animation complete
                        isPlaying = false;
                        OnAnimationComplete?.Invoke();
                        yield break;
                    }
                }
                
                // Update the sprite
                UpdateSprite();
            }
            
            // Wait for next frame
            yield return new WaitForSeconds(frameInterval);
        }
    }
    
    #region Public Methods
    
    /// <summary>
    /// Start playing the animation
    /// </summary>
    public void Play()
    {
        if (animationFrames == null || animationFrames.Length == 0)
        {
            Debug.LogWarning("AnimatedSprite: Cannot play - no animation frames assigned!");
            return;
        }
        
        if (isPlaying) return; // Already playing
        
        isPlaying = true;
        isPaused = false;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        animationCoroutine = StartCoroutine(Animate());
    }
    
    /// <summary>
    /// Stop the animation
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        isPaused = false;
        
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
    
    /// <summary>
    /// Pause the animation
    /// </summary>
    public void Pause()
    {
        isPaused = true;
    }
    
    /// <summary>
    /// Resume the animation
    /// </summary>
    public void Resume()
    {
        isPaused = false;
    }
    
    /// <summary>
    /// Restart the animation from the beginning
    /// </summary>
    public void Restart()
    {
        Stop();
        currentFrameIndex = 0;
        UpdateSprite();
        Play();
    }
    
    /// <summary>
    /// Set the animation to a specific frame
    /// </summary>
    /// <param name="frameIndex">Frame index to set to</param>
    public void SetFrame(int frameIndex)
    {
        if (animationFrames == null || frameIndex < 0 || frameIndex >= animationFrames.Length)
            return;
        
        currentFrameIndex = frameIndex;
        UpdateSprite();
    }
    
    /// <summary>
    /// Set the frame rate of the animation
    /// </summary>
    /// <param name="newFrameRate">New frame rate in frames per second</param>
    public void SetFrameRate(float newFrameRate)
    {
        frameRate = Mathf.Clamp(newFrameRate, 1f, 60f);
        frameInterval = 1f / frameRate;
    }
    
    /// <summary>
    /// Set new animation frames
    /// </summary>
    /// <param name="newFrames">Array of new Texture2D frames</param>
    public void SetAnimationFrames(Texture2D[] newFrames)
    {
        Stop();
        animationFrames = newFrames;
        InitializeAnimation();
    }
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Whether the animation is currently playing
    /// </summary>
    public bool IsPlaying => isPlaying;
    
    /// <summary>
    /// Whether the animation is currently paused
    /// </summary>
    public bool IsPaused => isPaused;
    
    /// <summary>
    /// Current frame index
    /// </summary>
    public int CurrentFrameIndex => currentFrameIndex;
    
    /// <summary>
    /// Total number of frames in the animation
    /// </summary>
    public int TotalFrames => totalFrames;
    
    /// <summary>
    /// Current frame rate
    /// </summary>
    public float FrameRate => frameRate;
    
    /// <summary>
    /// Animation progress as a value between 0 and 1
    /// </summary>
    public float AnimationProgress => totalFrames > 0 ? (float)currentFrameIndex / totalFrames : 0f;
    
    #endregion
    
    #region Editor Helpers
    
    void OnValidate()
    {
        // Update frame interval when frame rate changes in editor
        if (Application.isPlaying)
        {
            frameInterval = 1f / frameRate;
        }
    }
    
    #endregion
}
