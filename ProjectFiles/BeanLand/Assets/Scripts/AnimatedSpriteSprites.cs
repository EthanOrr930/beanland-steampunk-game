using UnityEngine;

public class AnimatedSpriteSprites : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("List of Sprites to animate through")]
    public Sprite[] animationFrames;
    
    [Tooltip("Time per frame for the animation (in seconds)")]
    [Range(0.01f, 1f)]
    public float animationSpeed = 0.1f;
    
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
    
    [Tooltip("Whether the animation is currently animating")]
    [SerializeField]
    private bool isAnimating = false;
    
    [Header("Sprite Renderer")]
    [Tooltip("SpriteRenderer component to use. If null, will try to find one on this GameObject")]
    public SpriteRenderer spriteRenderer;
    
    // Private variables
    private float animationTimer = 0f;
    private int totalFrames;
    private bool hasPlayedFullAnimation = false;
    private bool hasPlayedAnimationOnce = false;
    
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
    
    void Update()
    {
        AnimateSprites();
    }
    
    void InitializeAnimation()
    {
        if (animationFrames == null || animationFrames.Length == 0)
        {
            Debug.LogWarning("AnimatedSpriteSprites: No animation frames assigned!");
            return;
        }
        
        totalFrames = animationFrames.Length;
        
        // Randomize start frame if enabled
        if (randomizeStartFrame)
        {
            currentFrameIndex = Random.Range(0, totalFrames);
        }
        
        // Set initial frame
        UpdateSprite();
    }
    
    void AnimateSprites()
    {
        if (animationFrames.Length == 0) return;
        
        // If we've already played the animation once and not looping, don't play it again
        if (hasPlayedAnimationOnce && !loop)
        {
            return;
        }
        
        // If not playing and we haven't played full animation, show sprite[0] and reset
        if (!isPlaying && !isAnimating)
        {
            if (hasPlayedFullAnimation && !loop)
            {
                hasPlayedFullAnimation = false;
                currentFrameIndex = 0;
                spriteRenderer.sprite = animationFrames[0];
            }
            return;
        }
        
        // If we just started playing, start animation
        if (!isAnimating && isPlaying)
        {
            isAnimating = true;
            hasPlayedFullAnimation = false;
            currentFrameIndex = 0;
            animationTimer = 0f;
        }
        
        // Only animate if we have more than one sprite and we're animating
        if (animationFrames.Length > 1 && isAnimating)
        {
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                currentFrameIndex++;
                
                // If we've gone through all sprites
                if (currentFrameIndex >= animationFrames.Length)
                {
                    if (loop)
                    {
                        currentFrameIndex = 0; // Loop back to start
                    }
                    else
                    {
                        currentFrameIndex = 0;
                        hasPlayedFullAnimation = true;
                        isAnimating = false;
                        hasPlayedAnimationOnce = true; // Mark that we've played the animation once
                        OnAnimationComplete?.Invoke();
                    }
                }
                
                UpdateSprite();
            }
        }
    }
    
    void UpdateSprite()
    {
        if (spriteRenderer == null || animationFrames == null || currentFrameIndex >= animationFrames.Length)
            return;
        
        // Set the sprite directly
        Sprite currentSprite = animationFrames[currentFrameIndex];
        if (currentSprite != null)
        {
            spriteRenderer.sprite = currentSprite;
            OnFrameChanged?.Invoke(currentFrameIndex);
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
            Debug.LogWarning("AnimatedSpriteSprites: Cannot play - no animation frames assigned!");
            return;
        }
        
        if (isPlaying) return; // Already playing
        
        isPlaying = true;
        isAnimating = false; // Will be set to true in AnimateSprites
        hasPlayedAnimationOnce = false;
        hasPlayedFullAnimation = false;
    }
    
    /// <summary>
    /// Stop the animation
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        isAnimating = false;
    }
    
    /// <summary>
    /// Pause the animation (not used in this system, kept for compatibility)
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
    }
    
    /// <summary>
    /// Resume the animation (not used in this system, kept for compatibility)
    /// </summary>
    public void Resume()
    {
        isPlaying = true;
    }
    
    /// <summary>
    /// Restart the animation from the beginning
    /// </summary>
    public void Restart()
    {
        Stop();
        currentFrameIndex = 0;
        hasPlayedAnimationOnce = false;
        hasPlayedFullAnimation = false;
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
    /// Set the animation speed (time per frame)
    /// </summary>
    /// <param name="newAnimationSpeed">New animation speed in seconds per frame</param>
    public void SetAnimationSpeed(float newAnimationSpeed)
    {
        animationSpeed = Mathf.Clamp(newAnimationSpeed, 0.01f, 1f);
    }
    
    /// <summary>
    /// Set new animation frames
    /// </summary>
    /// <param name="newFrames">Array of new Sprite frames</param>
    public void SetAnimationFrames(Sprite[] newFrames)
    {
        Stop();
        animationFrames = newFrames;
        InitializeAnimation();
    }
    
    /// <summary>
    /// Reset animation state (useful for respawning or restarting)
    /// </summary>
    public void ResetAnimation()
    {
        hasPlayedAnimationOnce = false;
        hasPlayedFullAnimation = false;
        isAnimating = false;
        currentFrameIndex = 0;
        animationTimer = 0f;
        UpdateSprite();
    }
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Whether the animation is currently playing
    /// </summary>
    public bool IsPlaying => isPlaying;
    
    /// <summary>
    /// Whether the animation is currently animating
    /// </summary>
    public bool IsAnimating => isAnimating;
    
    /// <summary>
    /// Current frame index
    /// </summary>
    public int CurrentFrameIndex => currentFrameIndex;
    
    /// <summary>
    /// Total number of frames in the animation
    /// </summary>
    public int TotalFrames => totalFrames;
    
    /// <summary>
    /// Current animation speed (time per frame)
    /// </summary>
    public float AnimationSpeed => animationSpeed;
    
    /// <summary>
    /// Animation progress as a value between 0 and 1
    /// </summary>
    public float AnimationProgress => totalFrames > 0 ? (float)currentFrameIndex / totalFrames : 0f;
    
    /// <summary>
    /// Whether the animation has played once
    /// </summary>
    public bool HasPlayedOnce => hasPlayedAnimationOnce;
    
    #endregion
    
    #region Editor Helpers
    
    void OnValidate()
    {
        // Clamp animation speed when it changes in editor
        animationSpeed = Mathf.Clamp(animationSpeed, 0.01f, 1f);
    }
    
    #endregion
}
