using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Multiplayer Settings")]
    [SerializeField] private string multiplayerSceneName = "MultiplayerDefaultLevel";
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Room Settings")]
    [SerializeField] private int roomCodeLength = 6;
    
    [Header("Gravity Alignment")]
    [SerializeField] private float alignmentThreshold = 30f; // Degrees
    [SerializeField] private float alignmentCheckInterval = 0.5f; // Seconds
    
    // Room management
    private string currentRoomCode = "";
    private bool isHost = false;
    private bool isInRoom = false;
    private List<PlayerGravityData> players = new List<PlayerGravityData>();
    private Vector3 globalGravityDirection = Vector3.down;
    private bool isGravityAligned = false;
    
    // Singleton pattern
    public static MultiplayerManager Instance { get; private set; }
    
    [System.Serializable]
    public class PlayerGravityData
    {
        public string playerId;
        public Vector3 gravityDirection;
        public float lastUpdateTime;
        public bool isActive;
        
        public PlayerGravityData(string id)
        {
            playerId = id;
            gravityDirection = Vector3.down;
            lastUpdateTime = Time.time;
            isActive = true;
        }
    }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Start gravity alignment checking
        StartCoroutine(CheckGravityAlignment());
    }
    
    /// <summary>
    /// Generate a random room code
    /// </summary>
    public string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string roomCode = "";
        
        for (int i = 0; i < roomCodeLength; i++)
        {
            roomCode += chars[UnityEngine.Random.Range(0, chars.Length)];
        }
        
        Log($"Generated room code: {roomCode}");
        return roomCode;
    }
    
    /// <summary>
    /// Create a new room and become host
    /// </summary>
    public void CreateRoom()
    {
        if (isInRoom)
        {
            Log("Already in a room, leaving current room first");
            LeaveRoom();
        }
        
        currentRoomCode = GenerateRoomCode();
        isHost = true;
        isInRoom = true;
        
        // Add self as first player
        string playerId = SystemInfo.deviceUniqueIdentifier;
        players.Clear();
        players.Add(new PlayerGravityData(playerId));
        
        // Store room code for scene loading
        PlayerPrefs.SetString("MultiplayerRoomCode", currentRoomCode);
        PlayerPrefs.SetString("MultiplayerIsHost", "true");
        PlayerPrefs.Save();
        
        Log($"Created room with code: {currentRoomCode}");
        
        // Load multiplayer scene
        LoadMultiplayerScene();
    }
    
    /// <summary>
    /// Join an existing room
    /// </summary>
    public void JoinRoom(string roomCode)
    {
        if (isInRoom)
        {
            Log("Already in a room, leaving current room first");
            LeaveRoom();
        }
        
        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != roomCodeLength)
        {
            Log($"Invalid room code: {roomCode}");
            return;
        }
        
        currentRoomCode = roomCode.ToUpper();
        isHost = false;
        isInRoom = true;
        
        // Add self as player
        string playerId = SystemInfo.deviceUniqueIdentifier;
        players.Clear();
        players.Add(new PlayerGravityData(playerId));
        
        // Store room code for scene loading
        PlayerPrefs.SetString("MultiplayerRoomCode", currentRoomCode);
        PlayerPrefs.SetString("MultiplayerIsHost", "false");
        PlayerPrefs.Save();
        
        Log($"Joining room with code: {currentRoomCode}");
        
        // Load multiplayer scene
        LoadMultiplayerScene();
    }
    
    /// <summary>
    /// Leave the current room
    /// </summary>
    public void LeaveRoom()
    {
        if (!isInRoom) return;
        
        Log($"Leaving room: {currentRoomCode}");
        
        currentRoomCode = "";
        isHost = false;
        isInRoom = false;
        players.Clear();
        isGravityAligned = false;
        
        // Clear room data
        PlayerPrefs.DeleteKey("MultiplayerRoomCode");
        PlayerPrefs.DeleteKey("MultiplayerIsHost");
        PlayerPrefs.Save();
        
        // Return to main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    /// <summary>
    /// Load the multiplayer scene
    /// </summary>
    private void LoadMultiplayerScene()
    {
        Log($"Loading multiplayer scene: {multiplayerSceneName}");
        SceneManager.LoadScene(multiplayerSceneName);
    }
    
    /// <summary>
    /// Update player gravity data
    /// </summary>
    public void UpdatePlayerGravity(Vector3 gravityDirection)
    {
        if (!isInRoom) return;
        
        string playerId = SystemInfo.deviceUniqueIdentifier;
        
        // Find or create player data
        PlayerGravityData playerData = players.Find(p => p.playerId == playerId);
        if (playerData == null)
        {
            playerData = new PlayerGravityData(playerId);
            players.Add(playerData);
        }
        
        // Update player data
        playerData.gravityDirection = gravityDirection;
        playerData.lastUpdateTime = Time.time;
        playerData.isActive = true;
        
        Log($"Updated gravity for player {playerId}: {gravityDirection}");
    }
    
    /// <summary>
    /// Check if all players' gravity directions are aligned
    /// </summary>
    private IEnumerator CheckGravityAlignment()
    {
        while (true)
        {
            yield return new WaitForSeconds(alignmentCheckInterval);
            
            if (!isInRoom || players.Count < 2) continue;
            
            CheckForGravityAlignment();
        }
    }
    
    /// <summary>
    /// Check if players' gravity directions are aligned
    /// </summary>
    private void CheckForGravityAlignment()
    {
        if (players.Count < 2) return;
        
        // Get active players (updated within last 5 seconds)
        List<PlayerGravityData> activePlayers = new List<PlayerGravityData>();
        foreach (var player in players)
        {
            if (player.isActive && (Time.time - player.lastUpdateTime) < 5f)
            {
                activePlayers.Add(player);
            }
        }
        
        if (activePlayers.Count < 2) return;
        
        // Check if all players' gravity directions are similar
        Vector3 averageDirection = Vector3.zero;
        foreach (var player in activePlayers)
        {
            averageDirection += player.gravityDirection;
        }
        averageDirection /= activePlayers.Count;
        averageDirection.Normalize();
        
        // Check if all players are aligned with the average
        bool allAligned = true;
        foreach (var player in activePlayers)
        {
            float angle = Vector3.Angle(player.gravityDirection, averageDirection);
            if (angle > alignmentThreshold)
            {
                allAligned = false;
                break;
            }
        }
        
        // Update global gravity if aligned
        if (allAligned && !isGravityAligned)
        {
            SetGlobalGravity(averageDirection);
            isGravityAligned = true;
            Log($"Gravity aligned! Setting global gravity to: {averageDirection}");
        }
        else if (!allAligned && isGravityAligned)
        {
            // Players are no longer aligned, keep current global gravity
            isGravityAligned = false;
            Log("Players no longer aligned, keeping current global gravity");
        }
    }
    
    /// <summary>
    /// Set the global gravity direction
    /// </summary>
    private void SetGlobalGravity(Vector3 direction)
    {
        globalGravityDirection = direction;
        
        // Update Unity's global gravity
        Physics2D.gravity = direction * 9.8f;
        
        // Notify all player controllers
        PlayerController[] playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (var controller in playerControllers)
        {
            if (controller != null)
            {
                // Force update gravity in player controller
                controller.SetMultiplayerGravity(direction);
            }
        }
    }
    
    /// <summary>
    /// Get the current room code
    /// </summary>
    public string GetCurrentRoomCode()
    {
        return currentRoomCode;
    }
    
    /// <summary>
    /// Check if currently in a room
    /// </summary>
    public bool IsInRoom()
    {
        return isInRoom;
    }
    
    /// <summary>
    /// Check if currently hosting
    /// </summary>
    public bool IsHost()
    {
        return isHost;
    }
    
    /// <summary>
    /// Get the number of players in the room
    /// </summary>
    public int GetPlayerCount()
    {
        return players.Count;
    }
    
    /// <summary>
    /// Check if gravity is currently aligned
    /// </summary>
    public bool IsGravityAligned()
    {
        return isGravityAligned;
    }
    
    /// <summary>
    /// Get the current global gravity direction
    /// </summary>
    public Vector3 GetGlobalGravityDirection()
    {
        return globalGravityDirection;
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[MultiplayerManager] {message}");
        }
    }
}
