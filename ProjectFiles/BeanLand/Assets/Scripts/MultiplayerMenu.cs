using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MultiplayerMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI roomCodeDisplay;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    
    private MultiplayerManager multiplayerManager;
    private bool isCreatingRoom = false;
    private bool isJoiningRoom = false;
    
    private void Start()
    {
        // Get multiplayer manager
        multiplayerManager = MultiplayerManager.Instance;
        if (multiplayerManager == null)
        {
            // Create multiplayer manager if it doesn't exist
            GameObject managerObj = new GameObject("MultiplayerManager");
            multiplayerManager = managerObj.AddComponent<MultiplayerManager>();
        }
        
        // Setup UI
        SetupUI();
        
        // Check if we're already in a room
        if (multiplayerManager.IsInRoom())
        {
            ShowRoomStatus();
        }
    }
    
    private void SetupUI()
    {
        // Setup buttons
        if (createRoomButton != null)
        {
            createRoomButton.onClick.AddListener(CreateRoom);
        }
        
        if (joinRoomButton != null)
        {
            joinRoomButton.onClick.AddListener(JoinRoom);
        }
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBack);
        }
        
        // Setup room code input
        if (roomCodeInput != null)
        {
            roomCodeInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Enter 6-character room code";
            roomCodeInput.characterLimit = 6;
            roomCodeInput.onValueChanged.AddListener(OnRoomCodeChanged);
        }
        
        // Initial UI state
        UpdateUI();
    }
    
    private void Update()
    {
        // Update room status if in a room
        if (multiplayerManager != null && multiplayerManager.IsInRoom())
        {
            UpdateRoomStatus();
        }
    }
    
    /// <summary>
    /// Create a new room
    /// </summary>
    public void CreateRoom()
    {
        if (isCreatingRoom || isJoiningRoom) return;
        
        Log("Creating new room...");
        isCreatingRoom = true;
        UpdateUI();
        
        if (multiplayerManager != null)
        {
            multiplayerManager.CreateRoom();
        }
        else
        {
            Log("MultiplayerManager not found!");
            SetStatus("Error: Multiplayer system not available", Color.red);
            isCreatingRoom = false;
            UpdateUI();
        }
    }
    
    /// <summary>
    /// Join an existing room
    /// </summary>
    public void JoinRoom()
    {
        if (isCreatingRoom || isJoiningRoom) return;
        
        string roomCode = roomCodeInput != null ? roomCodeInput.text.Trim().ToUpper() : "";
        
        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6)
        {
            SetStatus("Please enter a valid 6-character room code", Color.red);
            return;
        }
        
        Log($"Joining room: {roomCode}");
        isJoiningRoom = true;
        UpdateUI();
        
        if (multiplayerManager != null)
        {
            multiplayerManager.JoinRoom(roomCode);
        }
        else
        {
            Log("MultiplayerManager not found!");
            SetStatus("Error: Multiplayer system not available", Color.red);
            isJoiningRoom = false;
            UpdateUI();
        }
    }
    
    /// <summary>
    /// Go back to main menu
    /// </summary>
    public void GoBack()
    {
        Log("Returning to main menu");
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    /// <summary>
    /// Handle room code input changes
    /// </summary>
    private void OnRoomCodeChanged(string value)
    {
        // Convert to uppercase and limit length
        if (roomCodeInput != null)
        {
            string upperValue = value.ToUpper();
            if (upperValue != value)
            {
                roomCodeInput.text = upperValue;
            }
        }
        
        // Update join button state
        UpdateUI();
    }
    
    /// <summary>
    /// Update UI elements
    /// </summary>
    private void UpdateUI()
    {
        // Update create room button
        if (createRoomButton != null)
        {
            createRoomButton.interactable = !isCreatingRoom && !isJoiningRoom;
        }
        
        // Update join room button
        if (joinRoomButton != null)
        {
            bool canJoin = !isCreatingRoom && !isJoiningRoom && 
                          roomCodeInput != null && 
                          roomCodeInput.text.Length == 6;
            joinRoomButton.interactable = canJoin;
        }
        
        // Update back button
        if (backButton != null)
        {
            backButton.interactable = !isCreatingRoom && !isJoiningRoom;
        }
        
        // Update room code input
        if (roomCodeInput != null)
        {
            roomCodeInput.interactable = !isCreatingRoom && !isJoiningRoom;
        }
    }
    
    /// <summary>
    /// Show room status when in a room
    /// </summary>
    private void ShowRoomStatus()
    {
        if (multiplayerPanel != null)
        {
            multiplayerPanel.SetActive(false);
        }
        
        if (roomCodeDisplay != null)
        {
            roomCodeDisplay.gameObject.SetActive(true);
            roomCodeDisplay.text = $"Room Code: {multiplayerManager.GetCurrentRoomCode()}";
        }
        
        SetStatus("Connected to room", Color.green);
    }
    
    /// <summary>
    /// Update room status display
    /// </summary>
    private void UpdateRoomStatus()
    {
        if (roomCodeDisplay != null)
        {
            roomCodeDisplay.text = $"Room Code: {multiplayerManager.GetCurrentRoomCode()}";
        }
        
        // Update status based on gravity alignment
        if (multiplayerManager.IsGravityAligned())
        {
            SetStatus("Gravity aligned! Working together!", Color.green);
        }
        else
        {
            SetStatus("Tilt your phone to align gravity with other players", Color.yellow);
        }
    }
    
    /// <summary>
    /// Set status text
    /// </summary>
    private void SetStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        
        Log($"Status: {message}");
    }
    
    /// <summary>
    /// Leave the current room
    /// </summary>
    public void LeaveRoom()
    {
        if (multiplayerManager != null)
        {
            multiplayerManager.LeaveRoom();
        }
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[MultiplayerMenu] {message}");
        }
    }
}
