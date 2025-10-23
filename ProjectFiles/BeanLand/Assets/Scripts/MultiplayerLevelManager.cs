using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerLevelManager : MonoBehaviour
{
    [Header("Multiplayer Settings")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string multiplayerMenuSceneName = "MultiplayerMenu";
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Level Settings")]
    [SerializeField] private bool loadLevelOnStart = true;
    [SerializeField] private string defaultLevelName = "MultiplayerDefaultLevel";
    
    private MultiplayerManager multiplayerManager;
    private LevelLoader levelLoader;
    private string currentLevelName;
    
    private void Start()
    {
        // Get multiplayer manager
        multiplayerManager = MultiplayerManager.Instance;
        if (multiplayerManager == null)
        {
            Log("MultiplayerManager not found, creating one");
            GameObject managerObj = new GameObject("MultiplayerManager");
            multiplayerManager = managerObj.AddComponent<MultiplayerManager>();
        }
        
        // Get level loader
        levelLoader = GetComponent<LevelLoader>();
        if (levelLoader == null)
        {
            levelLoader = gameObject.AddComponent<LevelLoader>();
        }
        
        // Check if we're in a multiplayer room
        if (!multiplayerManager.IsInRoom())
        {
            Log("Not in a multiplayer room, returning to main menu");
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }
        
        // Get room code
        string roomCode = multiplayerManager.GetCurrentRoomCode();
        Log($"In multiplayer room: {roomCode}");
        
        // Load the default multiplayer level
        if (loadLevelOnStart)
        {
            LoadMultiplayerLevel();
        }
    }
    
    /// <summary>
    /// Load the multiplayer level
    /// </summary>
    private void LoadMultiplayerLevel()
    {
        Log($"Loading multiplayer level: {defaultLevelName}");
        
        // Set the level name for the level loader
        if (levelLoader != null)
        {
            levelLoader.SetCurrentLevelName(defaultLevelName);
            levelLoader.LoadLevel(defaultLevelName);
        }
        else
        {
            Log("LevelLoader not found!");
        }
    }
    
    /// <summary>
    /// Return to multiplayer menu
    /// </summary>
    public void ReturnToMultiplayerMenu()
    {
        Log("Returning to multiplayer menu");
        SceneManager.LoadScene(multiplayerMenuSceneName);
    }
    
    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        Log("Returning to main menu");
        
        // Leave the room
        if (multiplayerManager != null)
        {
            multiplayerManager.LeaveRoom();
        }
        
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    /// <summary>
    /// Get the current room code
    /// </summary>
    public string GetCurrentRoomCode()
    {
        return multiplayerManager != null ? multiplayerManager.GetCurrentRoomCode() : "";
    }
    
    /// <summary>
    /// Check if gravity is aligned
    /// </summary>
    public bool IsGravityAligned()
    {
        return multiplayerManager != null && multiplayerManager.IsGravityAligned();
    }
    
    /// <summary>
    /// Get the number of players in the room
    /// </summary>
    public int GetPlayerCount()
    {
        return multiplayerManager != null ? multiplayerManager.GetPlayerCount() : 0;
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[MultiplayerLevelManager] {message}");
        }
    }
}
