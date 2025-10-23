using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI gravityStatusText;
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private Button returnToMenuButton;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private float updateInterval = 1f;
    
    private MultiplayerManager multiplayerManager;
    private float lastUpdateTime = 0f;
    
    private void Start()
    {
        // Get multiplayer manager
        multiplayerManager = MultiplayerManager.Instance;
        if (multiplayerManager == null)
        {
            Log("MultiplayerManager not found!");
            return;
        }
        
        // Setup buttons
        if (leaveRoomButton != null)
        {
            leaveRoomButton.onClick.AddListener(LeaveRoom);
        }
        
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(ReturnToMenu);
        }
        
        // Initial update
        UpdateUI();
    }
    
    private void Update()
    {
        // Update UI periodically
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateUI();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// Update all UI elements
    /// </summary>
    private void UpdateUI()
    {
        if (multiplayerManager == null) return;
        
        // Update room code
        if (roomCodeText != null)
        {
            roomCodeText.text = $"Room: {multiplayerManager.GetCurrentRoomCode()}";
        }
        
        // Update player count
        if (playerCountText != null)
        {
            int playerCount = multiplayerManager.GetPlayerCount();
            playerCountText.text = $"Players: {playerCount}";
        }
        
        // Update gravity status
        if (gravityStatusText != null)
        {
            if (multiplayerManager.IsGravityAligned())
            {
                gravityStatusText.text = "Gravity Aligned!";
                gravityStatusText.color = Color.green;
            }
            else
            {
                gravityStatusText.text = "Align your phone with other players";
                gravityStatusText.color = Color.yellow;
            }
        }
    }
    
    /// <summary>
    /// Leave the current room
    /// </summary>
    public void LeaveRoom()
    {
        Log("Leaving room");
        if (multiplayerManager != null)
        {
            multiplayerManager.LeaveRoom();
        }
    }
    
    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMenu()
    {
        Log("Returning to main menu");
        if (multiplayerManager != null)
        {
            multiplayerManager.LeaveRoom();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[MultiplayerUI] {message}");
        }
    }
}
