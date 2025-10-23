 using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public enum PlayerState
{
    WholeBean,
    Grounds
}

public class PlayerController : MonoBehaviour
{
    [Header("Gravity Settings")]
    // Adjust this value to make gravity stronger or weaker.
    public float gravityMultiplier = 9.8f;
    public float gSmoothingFactor = 100f;
    public float gravityThresholdAngle = 45f; // Degrees threshold for gravity direction change
    
    [Header("Gyroscope Settings")]
    public bool useGyroscopeGravity = true;
    public bool showGravityDebugLine = true;
    public float debugLineLength = 3f;
    
    [Header("Phone Orientation")]
    public bool usePhoneOrientation = true;
    public PhoneOrientationController phoneOrientationController;
    [Header("Launch Settings")]
    public float launchForceMultiplier = 10f;
    public float maxLaunchDistance = 5f;
    public float minLaunchDistance = 0.5f;
    public float maxDragDistance = 5f; // Maximum distance the user can drag from the start position
    public float launchCooldown = 0.3f;
    public int maxLaunches = 3; // Maximum number of launches
    public int currentLaunches = 3; // Current number of launches remaining
    public bool infiniteDashes = false; // If true, player has unlimited dashes
    
    [Header("Ground Check")]
    public LayerMask groundLayerMask = 1; // Default layer
    public float groundCheckDistance = 0.1f;
    
    [Header("Game Over")]
    public float fadeDuration = 2f;
    public float fadeInDuration = 1f; // Duration for fade in effect when scene loads
    
    [Header("Fade Overlay")]
    public UnityEngine.UI.RawImage fadeOverlay; // Black overlay for fade effects
    
    [Header("Stretch Indicator")]
    public LineRenderer stretchLine;
    public Material stretchLineMaterial;
    public float minLineWidth = 0.1f;
    public float maxLineWidth = 2f;
    
    [Header("Time Slow Settings")]
    public float dragTimeScale = 0.3f; // Time scale when dragging (0.3 = 30% normal speed)
    public bool enableTimeSlow = true; // Toggle to enable/disable time slowing
    
    [Header("Player State")]
    public PlayerState currentState = PlayerState.WholeBean;
    
    [Header("Grounds State Settings")]
    public int numberOfGroundsParticles = 8;
    public GameObject groundsParticlePrefab;
    public Sprite[] groundsSprites; // List of sprites for grounds particles
    public float attractionForce = 10f;
    public float maxDistance = 3f;
    public float teleportDistance = 20f;
    public float stopRange = 1f;
    public float groundsColliderRadius = 0.15f;
    public float minimalMotionSpeed = 0.5f; // Minimum speed particles maintain
    
    [Header("Grounds Particle Velocity Limits")]
    public float velocityLimitMin = 0.5f; // Minimum velocity limit when close to player
    public float velocityLimitMax = 5f; // Maximum velocity limit at edge of stopRange
    
    [Header("Shake to Detach Settings")]
    public float shakeThreshold = 3.0f; // Acceleration magnitude threshold to count as a shake
    public float shakeDetectionInterval = 0.1f; // How often to check for shake (in seconds)
    public float detachmentCooldown = 1.0f; // One particle per second
    
    [Header("Debug / Triggers")]
    public bool beGrounds = false; // Toggle to switch between WholeBean and Grounds
    
    [Header("Dash UI")]
    public Canvas dashUICanvas;
    public UnityEngine.UI.RawImage dashIcon;
    public TextMeshProUGUI dashCountText;
    
    [Header("Sprite Animation")]
    public Sprite[] spriteList; // List of sprites to switch between
    public float fastRollThreshold = 5f; // Speed threshold to consider "rolling fast"
    public float minSwitchInterval = 1f; // Minimum time between sprite switches
    public float maxSwitchInterval = 3f; // Maximum time between sprite switches
    
    // Private variables for gravity
    private Vector3 smoothedGravity;
    private Vector3 currentGravityDirection = Vector3.down;
    private Vector3[] gravityDirections = { 
        Vector3.down, 
        Vector3.up, 
        Vector3.left, 
        Vector3.right,
        new Vector3(-1, -1, 0).normalized,  // Down-left
        new Vector3(1, -1, 0).normalized,   // Down-right
        new Vector3(-1, 1, 0).normalized,   // Up-left
        new Vector3(1, 1, 0).normalized     // Up-right
    };
    
    // Private variables for drag and launch
    private bool isDragging = false;
    private Vector3 dragStartPosition;
    private Vector3 currentMousePosition;
    private Rigidbody2D playerRigidbody;
    private float lastLaunchTime = 0f;
    private bool isOnGround = false;
    private Vector3 lastGravityDirection;
    private bool isGameOver = false;
    
    // Private variables for sprite animation
    private SpriteRenderer spriteRenderer;
    private bool isSpriteAnimating = false;
    private Coroutine spriteAnimationCoroutine;
    
    // Private variables for time slow
    private float originalTimeScale = 1f;
    
    // Private variables for grounds state
    private List<GameObject> groundsParticles = new List<GameObject>();
    private CircleCollider2D mainCollider;
    private float originalColliderRadius = 0f; // Store original radius to restore later
    private bool isGroundsInitialized = false;
    private int originalPlayerLayer = 0; // Store original layer to restore later
    
    // Private variables for NoDashZone - support multiple zones
    private List<NoDashZone> activeNoDashZones = new List<NoDashZone>();
    private bool isInNoDashZone = false;
    
    // Private variables for fade effects
    private bool isFadingIn = false;
    
    // Private variables for shake detection
    private Vector3 lastAcceleration;
    private float lastShakeCheckTime = 0f;
    private float lastDetachmentTime = 0f;
    
    
    void Start()
    {
        // Get the Rigidbody2D component
        playerRigidbody = GetComponent<Rigidbody2D>();
        if (playerRigidbody == null)
        {
            playerRigidbody = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Setup phone orientation controller
        if (phoneOrientationController == null)
        {
            phoneOrientationController = FindFirstObjectByType<PhoneOrientationController>();
            if (phoneOrientationController == null && usePhoneOrientation)
            {
                GameObject orientationObj = new GameObject("PhoneOrientationController");
                phoneOrientationController = orientationObj.AddComponent<PhoneOrientationController>();
            }
        }
        
        // Setup stretch line
        SetupStretchLine();
        
        // Initialize gravity direction
        lastGravityDirection = Vector3.down;
        
        // Store original player layer
        originalPlayerLayer = gameObject.layer;
        
        // Ensure layers 8 and 9 don't collide with each other
        Physics2D.IgnoreLayerCollision(8, 9, true);
        
        // Initialize dash UI
        InitializeDashUI();
        
        // Setup fade overlay and start fade in effect
        SetupFadeOverlay();
        StartCoroutine(FadeInEffect());
        
        // Initialize shake detection
        if (usePhoneOrientation && phoneOrientationController != null)
        {
            lastAcceleration = phoneOrientationController.GetGravityDirection();
        }
        else
        {
            lastAcceleration = Input.acceleration;
        }
    }

    void Update()
    {
        // Sync state with debug trigger
        if (beGrounds && currentState != PlayerState.Grounds)
        {
            SetPlayerState(PlayerState.Grounds);
        }
        else if (!beGrounds && currentState != PlayerState.WholeBean)
        {
            SetPlayerState(PlayerState.WholeBean);
        }

        HandleGravity();
        CheckGround();
        
        // Handle different states
        if (currentState == PlayerState.WholeBean)
        {
            HandleInput();
            UpdateStretchIndicator();
            CheckForFastRolling();
        }
        else if (currentState == PlayerState.Grounds)
        {
            HandleInput(); // Use the same input handling for both states
            UpdateStretchIndicator(); // Show stretch indicator in grounds too
            UpdateGroundsParticles(); // Update grounds particle behavior
            DetectShakeAndDetachParticles(); // Check for shake to detach particles
        }
    }
    
    void HandleGravity()
    {
        Vector3 targetGravityDirection = currentGravityDirection;
        
        if (useGyroscopeGravity)
        {
            Vector3 phoneAcceleration;
            
            // Use phone orientation controller if available and enabled
            if (usePhoneOrientation && phoneOrientationController != null && phoneOrientationController.IsPhoneOrientationWorking())
            {
                phoneAcceleration = phoneOrientationController.GetGravityDirection();
            }
            else
            {
                // Fallback to Unity's built-in accelerometer
                phoneAcceleration = Input.acceleration;
            }
            
            // Convert phone orientation to world gravity vector
            // Phone's "down" is when gravity points toward the bottom of the phone
            // We want the opposite - where the phone's "top" is pointing becomes "down" in the game
            Vector3 phoneTopDirection = new Vector3(phoneAcceleration.x, phoneAcceleration.y, 0f);
            
            // Find the closest gravity direction based on threshold
            Vector3 closestDirection = GetClosestGravityDirection(phoneTopDirection);
            
            // Only change gravity direction if the phone tilt exceeds the threshold
            if (Vector3.Angle(phoneTopDirection, currentGravityDirection) > gravityThresholdAngle)
            {
                targetGravityDirection = closestDirection;
            }
        }
        else
        {
            // Fallback to default downward gravity
            targetGravityDirection = Vector3.down;
        }
        
        // Update current gravity direction if it changed
        if (targetGravityDirection != currentGravityDirection)
        {
            currentGravityDirection = targetGravityDirection;
            
        }
        
        // Calculate final gravity vector
        Vector3 targetGravity = currentGravityDirection * gravityMultiplier;
        
        // Smooth the gravity transition to avoid jitter
        Vector3 gravityChange = targetGravity - smoothedGravity;
        float changeMagnitude = gravityChange.magnitude;
        
        if (changeMagnitude > 0.5f) // Sharp transition threshold
        {
            // Sharp transition - apply immediately
            smoothedGravity = targetGravity;
        }
        else
        {
            // Small change - use smooth lerping
            smoothedGravity = Vector3.Lerp(smoothedGravity, targetGravity, Time.deltaTime * gSmoothingFactor);
        }

        // Set the global 2D physics gravity
        Physics2D.gravity = smoothedGravity;
        
        // Update last gravity direction for comparison
        lastGravityDirection = currentGravityDirection;
        
        // Draw debug line to visualize gravity direction
        if (showGravityDebugLine)
        {
            Debug.DrawRay(transform.position, currentGravityDirection * debugLineLength, Color.red, 0f);
        }
    }
    
    Vector3 GetClosestGravityDirection(Vector3 inputDirection)
    {
        Vector3 closestDirection = gravityDirections[0];
        float closestAngle = Vector3.Angle(inputDirection, closestDirection);
        
        for (int i = 1; i < gravityDirections.Length; i++)
        {
            float angle = Vector3.Angle(inputDirection, gravityDirections[i]);
            if (angle < closestAngle)
            {
                closestAngle = angle;
                closestDirection = gravityDirections[i];
            }
        }
        
        return closestDirection;
    }
    
    void CheckGround()
    {
        // Raycast downward to check if player is on ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayerMask);
        isOnGround = hit.collider != null;
        
        // Debug visualization
        Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, isOnGround ? Color.green : Color.red);
    }
    
    void HandleInput()
    {
        // Don't handle input if game is paused or fading in
        if (Time.timeScale == 0f || isFadingIn)
        {
            return;
        }
        
        // Handle mouse/touch input
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
    }
    
    void StartDrag()
    {
        // Check if player has launches remaining (unless infinite dashes)
        if (!infiniteDashes && currentLaunches <= 0)
        {
            Debug.Log("PlayerController: No launches remaining!");
            return;
        }
        
        // Check if player is in any active NoDashZone
        if (isInNoDashZone && activeNoDashZones.Count > 0)
        {
            Debug.Log("PlayerController: Cannot dash in NoDashZone!");
            return;
        }
        
        // Check cooldown
        if (Time.time - lastLaunchTime < launchCooldown)
        {
            return;
        }
        
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        // Click anywhere to start slingshot - drag start is where you click
        isDragging = true;
        dragStartPosition = mouseWorldPos;
        currentMousePosition = mouseWorldPos;
    
        
        // Slow down time when dragging starts
        if (enableTimeSlow)
        {
            originalTimeScale = Time.timeScale;
            Time.timeScale = dragTimeScale;
        }
    }
    
    void UpdateDrag()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        
        // Calculate the drag vector from start position to current mouse position
        Vector3 dragVector = mouseWorldPos - dragStartPosition;
        float dragDistance = dragVector.magnitude;
        
        // Clamp the drag distance to maxDragDistance
        if (dragDistance > maxDragDistance)
        {
            // Keep the direction but limit the magnitude
            dragVector = dragVector.normalized * maxDragDistance;
            currentMousePosition = dragStartPosition + dragVector;
        }
        else
        {
            currentMousePosition = mouseWorldPos;
        }
    }
    
    void EndDrag()
    {
        if (!isDragging) return;

        // reset the player's movement when finishing dragging
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }

        LaunchPlayer();
        isDragging = false;
        lastLaunchTime = Time.time;
        
        // Restore normal time when dragging ends
        if (enableTimeSlow)
        {
            Time.timeScale = originalTimeScale;
        }
    }
    
    void LaunchPlayer()
    {
        // Consume a launch (only if not infinite dashes)
        if (!infiniteDashes)
        {
            currentLaunches--;
        }
        Debug.Log($"PlayerController: Launch used! {currentLaunches} launches remaining.");
        
        Vector3 dragVector = currentMousePosition - dragStartPosition;
        float dragDistance = dragVector.magnitude;
        
        // Clamp drag distance
        dragDistance = Mathf.Clamp(dragDistance, minLaunchDistance, maxLaunchDistance);
        
        // Calculate launch direction (opposite of drag direction)
        Vector3 launchDirection = -dragVector.normalized;
        
        // Calculate launch force based on drag distance
        float launchForce = (dragDistance / maxLaunchDistance) * launchForceMultiplier;
        
        // Apply launch force to player in both states
        if (playerRigidbody != null)
        {
            playerRigidbody.AddForce(launchDirection * launchForce, ForceMode2D.Impulse);
        }
        
        // Start sprite animation on launch
        StartSpriteAnimation();
        
        // Update dash UI
        UpdateDashUI();
    }
    
    void UpdateStretchIndicator()
    {
        if (isDragging)
        {
            // Show stretch indicator
            stretchLine.enabled = true;
            
            // Update line renderer to show connection between drag start and current mouse position
            stretchLine.SetPosition(0, dragStartPosition);
            stretchLine.SetPosition(1, currentMousePosition);
            
            // Calculate dynamic line properties based on stretch distance
            float dragDistance = Vector3.Distance(currentMousePosition, dragStartPosition);
            float normalizedDistance = Mathf.Clamp01(dragDistance / maxLaunchDistance);
            
            // Dynamic line width - wider as you stretch further
            float lineWidth = Mathf.Lerp(minLineWidth, maxLineWidth, normalizedDistance);
            stretchLine.startWidth = lineWidth;
            stretchLine.endWidth = lineWidth;
            
            // Change line color based on stretch distance
            Color lineColor = Color.Lerp(Color.green, Color.red, normalizedDistance);
            stretchLine.startColor = lineColor;
            stretchLine.endColor = lineColor;
        }
        else
        {
            // Hide stretch indicator
            stretchLine.enabled = false;
        }
    }
    
    void SetupStretchLine()
    {
        // Create or get LineRenderer component
        stretchLine = gameObject.GetComponent<LineRenderer>();
        if (stretchLine == null)
        {
            stretchLine = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure LineRenderer
        stretchLine.material = stretchLineMaterial != null ? stretchLineMaterial : new Material(Shader.Find("Sprites/Default"));
        stretchLine.startColor = Color.green;
        stretchLine.endColor = Color.green;
        stretchLine.startWidth = 0.1f;
        stretchLine.endWidth = 0.1f;
        stretchLine.positionCount = 2;
        stretchLine.useWorldSpace = true;
        stretchLine.enabled = false;
    }
    
    
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
    
    void CheckForFastRolling()
    {
        if (playerRigidbody != null && spriteList != null && spriteList.Length > 0)
        {
            float currentSpeed = playerRigidbody.linearVelocity.magnitude;
            
            // Check if player is moving fast enough to trigger sprite animation
            if (currentSpeed >= fastRollThreshold && !isSpriteAnimating)
            {
                StartSpriteAnimation();
            }
            // Stop animation if speed drops below threshold
            else if (currentSpeed < fastRollThreshold && isSpriteAnimating)
            {
                StopSpriteAnimation();
            }
        }
    }
    
    void StartSpriteAnimation()
    {
        if (isSpriteAnimating || spriteList == null || spriteList.Length == 0)
            return;
            
        isSpriteAnimating = true;
        spriteAnimationCoroutine = StartCoroutine(AnimateSprites());
    }
    
    void StopSpriteAnimation()
    {
        if (spriteAnimationCoroutine != null)
        {
            StopCoroutine(spriteAnimationCoroutine);
            spriteAnimationCoroutine = null;
        }
        isSpriteAnimating = false;
    }
    
    System.Collections.IEnumerator AnimateSprites()
    {
        // Select 3 random sprites from the list
        int[] selectedIndices = new int[3];
        for (int i = 0; i < 3; i++)
        {
            selectedIndices[i] = Random.Range(0, spriteList.Length);
        }
        
        int currentSpriteIndex = 0;
        
        while (isSpriteAnimating)
        {
            // Switch to the next sprite
            if (spriteRenderer != null && spriteList[selectedIndices[currentSpriteIndex]] != null)
            {
                spriteRenderer.sprite = spriteList[selectedIndices[currentSpriteIndex]];
            }
            
            // Move to next sprite (cycle through the 3 selected)
            currentSpriteIndex = (currentSpriteIndex + 1) % 3;
            
            // Wait for a random interval
            float waitTime = Random.Range(minSwitchInterval, maxSwitchInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    // Public method to reset launch state (useful for respawning or resetting)
    public void ResetLaunch()
    {
        isDragging = false;
        lastLaunchTime = 0f; // Reset cooldown
        stretchLine.enabled = false;
    }
    
    // Public property to check if player is on ground
    public bool IsOnGround
    {
        get { return isOnGround; }
    }
    
    // Public Game Over function that can be called from any script
    public void GameOver()
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameOver = true;
        StartCoroutine(FadeToBlackAndRestartLevel());
    }
    
    // Public Level Completion function that can be called from any script
    public void CompleteLevel()
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameOver = true;
        
        // Use LevelManager if available (doesn't save time, just fades)
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.CompleteLevel();
        }
        else
        {
            // Fallback if no LevelManager
            StartCoroutine(FadeToBlackAndLoadMenu());
        }
    }
    
    // Public method for LevelManager to call when level is completed (after time is saved)
    public void CompleteLevelWithFade()
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameOver = true;
        StartCoroutine(FadeToBlackAndLoadMenu());
    }

    public void MainMenu()
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameOver = true;
        
        // Notify LevelManager that we're quitting (doesn't save time)
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.QuitToMainMenu();
        }
        
        // Start fade effect
        StartCoroutine(FadeToBlackAndLoadMenu());
    }
    
    private System.Collections.IEnumerator FadeToBlackAndLoadMenu()
    {
        // Unpause the game if it was paused (so fade works)
        Time.timeScale = 1f;
        
        // Create a black overlay
        GameObject fadeOverlay = new GameObject("FadeOverlay");
        fadeOverlay.transform.SetParent(Camera.main.transform);
        fadeOverlay.transform.localPosition = new Vector3(0, 0, 1); // In front of camera
        
        // Add Canvas and Image components for the fade
        Canvas fadeCanvas = fadeOverlay.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 1000; // On top of everything
        
        // Add a raw image for the black fade
        UnityEngine.UI.RawImage fadeImage = fadeOverlay.AddComponent<UnityEngine.UI.RawImage>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
        
        // Fade to black (use unscaledDeltaTime to work even if game was paused)
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        // Ensure fully black
        fadeImage.color = Color.black;
        
        // Load main menu scene
        SceneManager.LoadScene("Main Menu");
    }
    
    private System.Collections.IEnumerator FadeToBlackAndRestartLevel()
    {
        // Unpause the game if it was paused (so fade works)
        Time.timeScale = 1f;
        
        // Create a black overlay
        GameObject fadeOverlay = new GameObject("FadeOverlay");
        fadeOverlay.transform.SetParent(Camera.main.transform);
        fadeOverlay.transform.localPosition = new Vector3(0, 0, 1); // In front of camera
        
        // Add Canvas and Image components for the fade
        Canvas fadeCanvas = fadeOverlay.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 1000; // On top of everything
        
        // Add a raw image for the black fade
        UnityEngine.UI.RawImage fadeImage = fadeOverlay.AddComponent<UnityEngine.UI.RawImage>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
        
        // Fade to black (use unscaledDeltaTime to work even if game was paused)
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        // Ensure fully black
        fadeImage.color = Color.black;
        
        // Restart the current level
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Launch management methods
    public void AddLaunches(int amount)
    {
        currentLaunches = Mathf.Min(currentLaunches + amount, maxLaunches);
        Debug.Log($"PlayerController: Added {amount} launches! Total: {currentLaunches}");
        UpdateDashUI();
    }
    
    public void SetLaunches(int amount)
    {
        currentLaunches = Mathf.Clamp(amount, 0, maxLaunches);
        Debug.Log($"PlayerController: Set launches to {currentLaunches}");
        UpdateDashUI();
    }
    
    public void ResetLaunches()
    {
        currentLaunches = maxLaunches;
        Debug.Log($"PlayerController: Reset launches to {currentLaunches}");
        UpdateDashUI();
    }
    
    public int GetCurrentLaunches()
    {
        return currentLaunches;
    }
    
    public int GetMaxLaunches()
    {
        return maxLaunches;
    }
    
    public bool HasLaunches()
    {
        return currentLaunches > 0;
    }
    
    // State management methods
    public void SetPlayerState(PlayerState newState)
    {
        if (currentState == newState) return;
        
        PlayerState previousState = currentState;
        currentState = newState;
        
        OnStateChanged(previousState, newState);
    }
    
    void OnStateChanged(PlayerState previousState, PlayerState newState)
    {
        if (newState == PlayerState.Grounds)
        {
            // Switch to grounds state
            EnableGroundsState();
        }
        else if (newState == PlayerState.WholeBean)
        {
            // Switch to whole bean state
            DisableGroundsState();
        }
    }
    
    void EnableGroundsState()
    {
        // Keep player dynamic so it can be launched and affected by gravity
        if (playerRigidbody != null)
        {
            playerRigidbody.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // Get main collider and store its original radius
        mainCollider = GetComponent<CircleCollider2D>();
        if (mainCollider != null)
        {
            originalColliderRadius = mainCollider.radius;
            mainCollider.radius = groundsColliderRadius; // Shrink to 0.15
        }
        
        // Disable sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Set the player to layer 8 so it doesn't collide with particles (layer 9)
        gameObject.layer = 8;
        
        // Spawn grounds particles
        SpawnGroundsParticles();
        
        isGroundsInitialized = true;
        Debug.Log($"PlayerController: Transitioning to grounds state with {numberOfGroundsParticles} particles");
    }
    
    void DisableGroundsState()
    {
        // Player physics remain dynamic in both states
        
        // Restore main collider radius
        if (mainCollider != null)
        {
            mainCollider.radius = originalColliderRadius;
        }
        
        // Re-enable sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        // Clean up grounds state
        CleanupGroundsState();
        
        // Restore original player layer
        gameObject.layer = originalPlayerLayer;
        
        isGroundsInitialized = false;
        Debug.Log("PlayerController: Transitioning to whole bean state");
    }
    
    
    // Public methods for external state control
    public void SwitchToGrounds()
    {
        SetPlayerState(PlayerState.Grounds);
    }
    
    public void SwitchToWholeBean()
    {
        SetPlayerState(PlayerState.WholeBean);
    }
    
    public PlayerState GetCurrentState()
    {
        return currentState;
    }
    
    // Grounds state methods
    void SpawnGroundsParticles()
    {
        // Clear existing particles
        CleanupGroundsParticles();
        
        // Get the main player collider to spawn particles from
        CircleCollider2D mainCollider = GetComponent<CircleCollider2D>();
        float spawnRadius = mainCollider != null ? mainCollider.radius : 1f;
        
        for (int i = 0; i < numberOfGroundsParticles; i++)
        {
            // Calculate position in a circle around the player
            float angle = (360f / numberOfGroundsParticles) * i * Mathf.Deg2Rad;
            Vector3 spawnPosition = transform.position + new Vector3(
                Mathf.Cos(angle) * spawnRadius,
                Mathf.Sin(angle) * spawnRadius,
                0f
            );
            
            // Create particle
            GameObject particle;
            if (groundsParticlePrefab != null)
            {
                particle = Instantiate(groundsParticlePrefab, spawnPosition, Quaternion.identity);
                // Set particle layer to layer 9
                particle.layer = 9;
                // Ensure it has the Player tag
                particle.tag = "Player";
            }
            else
            {
                // Create a basic particle if no prefab is assigned
                particle = CreateBasicGroundsParticle(spawnPosition);
            }
            
        // Assign random sprite
        if (groundsSprites != null && groundsSprites.Length > 0)
        {
            SpriteRenderer particleRenderer = particle.GetComponent<SpriteRenderer>();
            if (particleRenderer != null)
            {
                particleRenderer.sprite = groundsSprites[Random.Range(0, groundsSprites.Length)];
            }
        }
            
            // Add GroundsParticle component
            GroundsParticle groundsParticleComponent = particle.AddComponent<GroundsParticle>();
            groundsParticleComponent.Initialize(this, transform);
            
            groundsParticles.Add(particle);
        }
    }
    
    GameObject CreateBasicGroundsParticle(Vector3 position)
    {
        GameObject particle = new GameObject("GroundsParticle");
        particle.transform.position = position;
        
        // Add sprite renderer
        SpriteRenderer renderer = particle.AddComponent<SpriteRenderer>();
        renderer.color = new Color(0.6f, 0.3f, 0.1f); // Brown color for grounds
        
        // Add rigidbody
        Rigidbody2D rb = particle.AddComponent<Rigidbody2D>();
        rb.gravityScale = 1f; // Allow gravity and physics for grounds particles
        
        // Add circle collider
        CircleCollider2D collider = particle.AddComponent<CircleCollider2D>();
        collider.radius = 0.2f;
        collider.isTrigger = true;
        
        // Add tag for identification
        particle.tag = "Player"; // So they count as player for flamethrower/steam
        
        // Set particle layer to layer 9
        particle.layer = 9;
        
        return particle;
    }
    
    void UpdateGroundsParticles()
    {
        if (!isGroundsInitialized) return;
        
        // Update each particle's behavior
        foreach (GameObject particle in groundsParticles)
        {
            if (particle != null)
            {
                GroundsParticle groundsParticleComponent = particle.GetComponent<GroundsParticle>();
                if (groundsParticleComponent != null)
                {
                    groundsParticleComponent.UpdateBehavior();
                }
            }
        }
    }
    
    void DetectShakeAndDetachParticles()
    {
        if (!isGroundsInitialized) return;
        
        // Check if enough time has passed since last shake check
        if (Time.time - lastShakeCheckTime < shakeDetectionInterval)
        {
            return;
        }
        
        lastShakeCheckTime = Time.time;
        
        // Get current acceleration
        Vector3 currentAcceleration;
        if (usePhoneOrientation && phoneOrientationController != null && phoneOrientationController.IsPhoneOrientationWorking())
        {
            currentAcceleration = phoneOrientationController.GetGravityDirection();
        }
        else
        {
            currentAcceleration = Input.acceleration;
        }
        
        // Calculate acceleration delta (rapid change indicates shake)
        Vector3 accelerationDelta = currentAcceleration - lastAcceleration;
        float shakeMagnitude = accelerationDelta.magnitude;
        
        // Update last acceleration for next check
        lastAcceleration = currentAcceleration;
        
        // Check if shake is strong enough
        if (shakeMagnitude > shakeThreshold)
        {
            // Check if cooldown has passed (one particle per second)
            if (Time.time - lastDetachmentTime >= detachmentCooldown)
            {
                // Try to detach a particle
                DetachOneParticle();
                lastDetachmentTime = Time.time;
            }
        }
    }
    
    void DetachOneParticle()
    {
        // Count how many particles are currently attached
        int attachedCount = 0;
        foreach (GameObject particle in groundsParticles)
        {
            if (particle != null)
            {
                GroundsParticle groundsParticleComponent = particle.GetComponent<GroundsParticle>();
                if (groundsParticleComponent != null && !groundsParticleComponent.IsDetached())
                {
                    attachedCount++;
                }
            }
        }
        
        // Must keep at least one particle attached
        if (attachedCount <= 1)
        {
            Debug.Log("PlayerController: Cannot detach last particle!");
            return;
        }
        
        // Find first attached particle and detach it
        foreach (GameObject particle in groundsParticles)
        {
            if (particle != null)
            {
                GroundsParticle groundsParticleComponent = particle.GetComponent<GroundsParticle>();
                if (groundsParticleComponent != null && !groundsParticleComponent.IsDetached())
                {
                    groundsParticleComponent.Detach();
                    Debug.Log($"PlayerController: Shake detected! Detached particle. {attachedCount - 1} attached remaining.");
                    return;
                }
            }
        }
    }
    
    void CleanupGroundsState()
    {
        // Clean up particles
        CleanupGroundsParticles();
    }
    
    void CleanupGroundsParticles()
    {
        foreach (GameObject particle in groundsParticles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        groundsParticles.Clear();
    }
    
    // NoDashZone integration methods - supports multiple zones
    public void SetNoDashZone(NoDashZone zone, bool inZone)
    {
        if (zone == null) return; // Safety check
        
        if (inZone)
        {
            // Add zone to active zones if not already present
            if (!activeNoDashZones.Contains(zone))
            {
                activeNoDashZones.Add(zone);
                Debug.Log($"PlayerController: Entered NoDashZone - dashing disabled (active zones: {activeNoDashZones.Count})");
            }
            isInNoDashZone = true;
        }
        else
        {
            // Remove zone from active zones
            activeNoDashZones.Remove(zone);
            
            // Only enable dashing if no zones remain
            isInNoDashZone = activeNoDashZones.Count > 0;
            
            if (!isInNoDashZone)
            {
                Debug.Log("PlayerController: Exited all NoDashZones - dashing enabled");
            }
            else
            {
                Debug.Log($"PlayerController: Exited NoDashZone but still in {activeNoDashZones.Count} zone(s)");
            }
        }
        
        // Update dash UI when zone status changes
        UpdateDashUI();
    }
    
    public bool IsInNoDashZone()
    {
        return isInNoDashZone;
    }
    
    public NoDashZone GetCurrentNoDashZone()
    {
        // Return any active zone (for backward compatibility)
        // Returns null if no zones are active
        foreach (NoDashZone zone in activeNoDashZones)
        {
            if (zone != null && zone.IsActive())
            {
                return zone;
            }
        }
        return null;
    }
    
    // Get all active NoDashZones
    public List<NoDashZone> GetAllActiveNoDashZones()
    {
        return activeNoDashZones;
    }
    
    // Removed bridge methods to avoid Unity WebGL memory issues

    // Dash UI Methods
    void InitializeDashUI()
    {
        if (dashUICanvas != null && dashIcon != null && dashCountText != null)
        {
            UpdateDashUI();
        }
        else
        {
            Debug.LogWarning("PlayerController: Dash UI components not properly assigned!");
        }
    }
    
    void UpdateDashUI()
    {
        if (dashUICanvas == null || dashIcon == null || dashCountText == null)
            return;
        
        // Update icon color based on NoDashZone status or dash availability
        if (isInNoDashZone && activeNoDashZones.Count > 0)
        {
            // Player is in a NoDashZone - make icon red
            dashIcon.color = Color.red;
        }
        else if (!infiniteDashes && currentLaunches <= 0)
        {
            // Player is out of dashes - make icon red
            dashIcon.color = Color.red;
        }
        else
        {
            // Player can dash - make icon white
            dashIcon.color = Color.white;
        }
        
        // Update dash count text
        if (infiniteDashes)
        {
            // Show nothing for infinite dashes
            dashCountText.text = "";
        }
        else
        {
            // Show current dash count
            dashCountText.text = currentLaunches.ToString();
        }
    }
    
    private void SetupFadeOverlay()
    {
        // Create fade overlay if not assigned
        if (fadeOverlay == null)
        {
            // Create a black overlay canvas
            GameObject overlayObj = new GameObject("FadeOverlay");
            overlayObj.transform.SetParent(transform);
            
            Canvas canvas = overlayObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000; // On top of everything
            
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(overlayObj.transform);
            
            fadeOverlay = imageObj.AddComponent<UnityEngine.UI.RawImage>();
            fadeOverlay.color = Color.black; // Start with black overlay
            
            // Make it cover the entire screen
            RectTransform rectTransform = fadeOverlay.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
    
    // Fade in effect when scene loads - waits for level loading to complete
    private System.Collections.IEnumerator FadeInEffect()
    {
        if (fadeOverlay == null) yield break;
        
        isFadingIn = true;
        
        // Keep screen completely black during loading
        fadeOverlay.color = Color.black;
        
        // Freeze player velocity (but don't change body type to avoid WASM issues)
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.angularVelocity = 0f;
        }
        
        // Wait for level loading to complete
        LevelLoader levelLoader = FindFirstObjectByType<LevelLoader>();
        if (levelLoader != null)
        {
            Debug.Log("PlayerController: Waiting for level to load...");
            
            // Wait until level loading is complete with safety check
            int maxWaitFrames = 300; // 5 seconds at 60fps
            int waitedFrames = 0;
            while (levelLoader != null && levelLoader.IsLoading() && waitedFrames < maxWaitFrames)
            {
                // Keep screen black and player frozen
                fadeOverlay.color = Color.black;
                if (playerRigidbody != null)
                {
                    playerRigidbody.linearVelocity = Vector2.zero;
                    playerRigidbody.angularVelocity = 0f;
                }
                yield return null;
                waitedFrames++;
            }
            
            Debug.Log("PlayerController: Level loading completed");
        }
        
        // Wait an additional frame to ensure all transforms are applied
        // This ensures player position is fully updated before we fade out
        yield return new WaitForSeconds(0.2f);
        
        Debug.Log("PlayerController: Starting fade out from black");
        
        // Now fade out the black overlay
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeInDuration));
            fadeOverlay.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        
        // Ensure overlay is fully transparent
        fadeOverlay.color = new Color(0f, 0f, 0f, 0f);
        
        isFadingIn = false;
        Debug.Log("PlayerController: Fade in effect completed");
    }
    
    // Public method to manually trigger fade in (useful for level transitions)
    public void TriggerFadeIn()
    {
        if (!isFadingIn)
        {
            StartCoroutine(FadeInEffect());
        }
    }
    
    // Public method to fade out the player (useful before level transitions)
    public void FadeOutPlayer()
    {
        StartCoroutine(FadeOutEffect());
    }
    
    private System.Collections.IEnumerator FadeOutEffect()
    {
        if (fadeOverlay == null) yield break;
        
        // Fade in the black overlay
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            fadeOverlay.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        
        // Ensure overlay is fully black
        fadeOverlay.color = Color.black;
        
        Debug.Log("PlayerController: Fade out effect completed");
    }
}