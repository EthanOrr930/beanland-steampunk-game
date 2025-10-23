using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    
    [Header("Level Loading")]
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private bool loadLevelOnStart = true;
    
    [Header("Timer")]
    [SerializeField] private bool trackLevelTime = true;
    private float levelStartTime;
    private float levelCompletionTime;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private string currentLevelName;
    private PlayerController playerController;
    private bool levelCompleted = false;
    
    private void Awake()
    {
        // Get LevelLoader component
        if (levelLoader == null)
        {
            levelLoader = GetComponent<LevelLoader>();
        }
        
        // Get current level name from PlayerPrefs
        currentLevelName = PlayerPrefs.GetString("CurrentLevelName", "");
        
        if (string.IsNullOrEmpty(currentLevelName))
        {
            Debug.LogWarning("LevelManager: No level name found in PlayerPrefs!");
        }
        else
        {
            Log($"LevelManager: Current level is '{currentLevelName}'");
        }
    }
    
    private void Start()
    {
        // Find player controller
        playerController = FindFirstObjectByType<PlayerController>();
        
        if (playerController == null)
        {
            Debug.LogWarning("LevelManager: PlayerController not found!");
        }
        
        // Load the level if requested
        if (loadLevelOnStart && !string.IsNullOrEmpty(currentLevelName))
        {
            if (levelLoader != null)
            {
                levelLoader.SetCurrentLevelName(currentLevelName);
                levelLoader.LoadLevel(currentLevelName);
            }
            else
            {
                Debug.LogError("LevelManager: LevelLoader component not found!");
            }
        }
        
        // Start the timer
        if (trackLevelTime)
        {
            levelStartTime = Time.time;
        }
    }
    
    public void CompleteLevel()
    {
        if (levelCompleted)
        {
            Log("LevelManager: Level already completed, ignoring duplicate call");
            return;
        }
        
        levelCompleted = true;
        
        // Calculate completion time
        if (trackLevelTime)
        {
            levelCompletionTime = Time.time - levelStartTime;
            Log($"LevelManager: Level completed in {levelCompletionTime:F2} seconds");
            
            // Save the completion time
            SaveLevelProgress();
        }
        
        // Notify player to fade out and return to menu
        if (playerController != null)
        {
            playerController.CompleteLevelWithFade();
        }
        else
        {
            // Fallback: Load main menu directly
            Log("LevelManager: No PlayerController found, loading main menu directly");
            LoadMainMenu();
        }
    }
    
    public void QuitToMainMenu()
    {
        Log("LevelManager: Quitting to main menu (level not completed)");
        LoadMainMenu();
    }
    
    private void SaveLevelProgress()
    {
        if (string.IsNullOrEmpty(currentLevelName))
        {
            Debug.LogWarning("LevelManager: Cannot save progress - no level name!");
            return;
        }
        
        // Mark level as played
        string playedKey = $"Level_{currentLevelName}_Played";
        PlayerPrefs.SetInt(playedKey, 1);
        
        // Save best time (only if it's better than previous or first time)
        string timeKey = $"Level_{currentLevelName}_Time";
        float previousBestTime = PlayerPrefs.GetFloat(timeKey, -1f);
        
        if (previousBestTime < 0 || levelCompletionTime < previousBestTime)
        {
            PlayerPrefs.SetFloat(timeKey, levelCompletionTime);
            Log($"LevelManager: New best time saved: {levelCompletionTime:F2}s");
        }
        else
        {
            Log($"LevelManager: Previous best time ({previousBestTime:F2}s) not beaten");
        }
        
        PlayerPrefs.Save();
        
        Log($"LevelManager: Progress saved for level '{currentLevelName}'");
    }
    
    private void LoadMainMenu()
    {
        Log($"LevelManager: Loading main menu scene '{mainMenuSceneName}'");
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public float GetCurrentLevelTime()
    {
        if (trackLevelTime)
        {
            return Time.time - levelStartTime;
        }
        return 0f;
    }
    
    public string GetCurrentLevelName()
    {
        return currentLevelName;
    }
    
    public void SetCurrentLevelName(string levelName)
    {
        currentLevelName = levelName;
        Log($"LevelManager: Level name set to '{currentLevelName}'");
    }
    
    public bool IsLevelCompleted()
    {
        return levelCompleted;
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[LevelManager] {message}");
        }
    }
    
    // Editor helper to reload current level
    [ContextMenu("Reload Current Level")]
    public void ReloadCurrentLevel()
    {
        if (levelLoader != null && !string.IsNullOrEmpty(currentLevelName))
        {
            levelCompleted = false;
            levelStartTime = Time.time;
            levelLoader.LoadLevel(currentLevelName);
        }
    }
}

