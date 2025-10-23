using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;

public class MainMenu : MonoBehaviour
{
    [Header("Level Card Management")]
    [SerializeField] private GameObject levelCardPrefab;
    [SerializeField] private Transform levelCardContainer;
    [SerializeField] private int cardsPerPage = 6;
    
    [Header("Card Positioning")]
    [SerializeField] private Vector3[] cardPositions = new Vector3[]
    {
        new Vector3(-210, 150, 0),   // Row 1, Column 1
        new Vector3(0, 150, 0),      // Row 1, Column 2
        new Vector3(210, 150, 0),    // Row 1, Column 3
        new Vector3(-210, -230, 0),  // Row 2, Column 1
        new Vector3(0, -230, 0),     // Row 2, Column 2
        new Vector3(210, -230, 0)    // Row 2, Column 3
    };
    [SerializeField] private Vector3 cardScale = new Vector3(3.5f, 3.5f, 3.5f);
    
    [Header("Navigation")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private TextMeshProUGUI pageNumberText;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private string baseLevelSceneName = "BaseLevel";
    [SerializeField] private string levelsManifestUrl = "https://beanbox.netlify.app/Levels/levels.json";
    
    [Header("Multiplayer")]
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private string multiplayerSceneName = "MultiplayerMenu";
    
    [Header("Debug")]
    [SerializeField] private List<string> availableLevels = new List<string>();
    [SerializeField] private int currentPage = 0;
    [SerializeField] private int totalPages = 0;
    
    private List<LevelCard> levelCards = new List<LevelCard>();
    private List<LevelInfo> levelInfos = new List<LevelInfo>();
    private bool isUpdatingNavigation = false;
    
    [System.Serializable]
    public class LevelInfo
    {
        public string levelName;
        public long exportTimestamp;
        public bool hasBeenPlayed;
        public float bestTime;
        
        public LevelInfo(string name, long timestamp, bool played, float time)
        {
            levelName = name;
            exportTimestamp = timestamp;
            hasBeenPlayed = played;
            bestTime = time;
        }
    }
    
    [System.Serializable]
    public class LevelManifestEntry
    {
        public string name;
        public long exportTimestamp;
    }
    
    [System.Serializable]
    public class LevelManifest
    {
        public List<LevelManifestEntry> levels = new List<LevelManifestEntry>();
    }
    
    private void Start()
    {
        InitializeMenu();
    }
    
    private void InitializeMenu()
    {
        Log("MainMenu: Initializing...");
        
        // Validate required components
        if (levelCardPrefab == null)
        {
            Debug.LogError("MainMenu: Level Card Prefab is not assigned!");
            return;
        }
        
        if (levelCardContainer == null)
        {
            Debug.LogError("MainMenu: Level Card Container is not assigned!");
            return;
        }
        
        // Setup navigation buttons
        if (leftArrowButton != null)
            leftArrowButton.onClick.AddListener(PreviousPage);
        
        if (rightArrowButton != null)
            rightArrowButton.onClick.AddListener(NextPage);
        
        // Setup multiplayer button
        if (multiplayerButton != null)
            multiplayerButton.onClick.AddListener(OpenMultiplayer);
        
        // Load available levels and setup cards
        StartCoroutine(LoadAvailableLevels());
    }
    
    private IEnumerator LoadAvailableLevels()
    {
        Log("MainMenu: Loading available levels...");
        
        availableLevels.Clear();
        
        // Always fetch levels from the website
        yield return StartCoroutine(FetchLevelsFromWeb());
        
        // Load level information
        LoadLevelInformation();
        
        // Sort levels by export timestamp (oldest first)
        SortLevelsByTimestamp();
        
        // Calculate pages
        CalculatePages();
        
        // Find first page with new levels
        FindFirstNewLevelPage();
        
        // Setup level cards (with frame delay)
        yield return StartCoroutine(SetupLevelCards());
        
        // Update navigation
        UpdateNavigation();
    }
    
    private IEnumerator FetchLevelsFromWeb()
    {
        Log($"MainMenu: Fetching levels from {levelsManifestUrl}");
        Debug.Log($"<color=cyan>[MainMenu] WebGL Build - Fetching manifest: {levelsManifestUrl}</color>");
        Debug.Log($"<color=cyan>[MainMenu] Platform: {Application.platform}</color>");
        
        using (UnityWebRequest request = UnityWebRequest.Get(levelsManifestUrl))
        {
            request.timeout = 30;
            
            yield return request.SendWebRequest();
            
            Debug.Log($"<color=cyan>[MainMenu] Manifest request completed. Result: {request.result}</color>");
            Debug.Log($"<color=cyan>[MainMenu] Response Code: {request.responseCode}</color>");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                Debug.Log($"<color=cyan>[MainMenu] Received levels JSON length: {json.Length} characters</color>");
                Log($"MainMenu: Received levels JSON: {json}");
                
                try
                {
                    LevelManifest manifest = JsonUtility.FromJson<LevelManifest>(json);
                    
                    if (manifest != null && manifest.levels != null)
                    {
                        Debug.Log($"<color=green>[MainMenu] Successfully parsed manifest with {manifest.levels.Count} levels</color>");
                        
                        foreach (LevelManifestEntry entry in manifest.levels)
                        {
                            // Strip .json extension if present
                            string levelName = entry.name;
                            if (levelName.EndsWith(".json"))
                            {
                                levelName = levelName.Substring(0, levelName.Length - 5);
                            }
                            
                            availableLevels.Add(levelName);
                            
                            // Cache the timestamp in PlayerPrefs
                            string timestampKey = $"Level_{levelName}_Timestamp";
                            PlayerPrefs.SetString(timestampKey, entry.exportTimestamp.ToString());
                            
                            Log($"MainMenu: Added level '{levelName}' (timestamp: {entry.exportTimestamp})");
                        }
                        
                        PlayerPrefs.Save();
                        Log($"MainMenu: Successfully loaded {availableLevels.Count} levels from web");
                    }
                    else
                    {
                        Debug.LogError("MainMenu: Manifest is null or has no levels");
                        // Fallback to hardcoded list
                        availableLevels.Add("TEST");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"MainMenu: Failed to parse levels manifest: {e.Message}");
                    Debug.LogError($"MainMenu: Stack trace: {e.StackTrace}");
                    // Fallback to hardcoded list
                    availableLevels.Add("TEST");
                }
            }
            else
            {
                Debug.LogError($"MainMenu: Failed to fetch levels manifest!");
                Debug.LogError($"MainMenu: Error: {request.error}");
                Debug.LogError($"MainMenu: Response Code: {request.responseCode}");
                Debug.LogError($"MainMenu: URL: {levelsManifestUrl}");
                
                // Check for specific error types
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("[MainMenu] Connection Error - Check network connection");
                }
                else if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[MainMenu] Protocol Error - Server returned an error (possibly CORS issue)");
                }
                
                // Fallback to hardcoded list
                availableLevels.Add("TEST");
            }
        }
    }
    
    private void LoadLevelInformation()
    {
        levelInfos.Clear();
        
        foreach (string levelName in availableLevels)
        {
            bool hasBeenPlayed = PlayerPrefs.GetInt($"Level_{levelName}_Played", 0) == 1;
            float bestTime = PlayerPrefs.GetFloat($"Level_{levelName}_Time", -1f);
            
            // Get timestamp from manifest if available, otherwise use current time
            long timestamp = GetLevelTimestamp(levelName);
            
            levelInfos.Add(new LevelInfo(levelName, timestamp, hasBeenPlayed, bestTime));
        }
        
        Log($"MainMenu: Loaded information for {levelInfos.Count} levels");
    }
    
    private long GetLevelTimestamp(string levelName)
    {
        // Try to get timestamp from PlayerPrefs (cached from manifest)
        string key = $"Level_{levelName}_Timestamp";
        if (PlayerPrefs.HasKey(key))
        {
            // PlayerPrefs doesn't support long, so we stored it as string
            string timestampStr = PlayerPrefs.GetString(key, "0");
            if (long.TryParse(timestampStr, out long timestamp))
            {
                return timestamp;
            }
        }
        
        // Default to current time if not found
        return System.DateTimeOffset.Now.ToUnixTimeSeconds();
    }
    
    private void SortLevelsByTimestamp()
    {
        // Sort by export timestamp (oldest first, so newest appears on later pages)
        levelInfos.Sort((a, b) => a.exportTimestamp.CompareTo(b.exportTimestamp));
        Log("MainMenu: Sorted levels by export timestamp (oldest first)");
    }
    
    private void CalculatePages()
    {
        totalPages = Mathf.CeilToInt((float)levelInfos.Count / cardsPerPage);
        Log($"MainMenu: Calculated {totalPages} pages for {levelInfos.Count} levels");
    }
    
    private void FindFirstNewLevelPage()
    {
        // Find the first page that contains a level that hasn't been played
        for (int page = 0; page < totalPages; page++)
        {
            int startIndex = page * cardsPerPage;
            int endIndex = Mathf.Min(startIndex + cardsPerPage, levelInfos.Count);
            
            for (int i = startIndex; i < endIndex; i++)
            {
                if (!levelInfos[i].hasBeenPlayed)
                {
                    currentPage = page;
                    Log($"MainMenu: Starting on page {currentPage + 1} (first page with new levels)");
                    return;
                }
            }
        }
        
        // If all levels have been played, start on the last page
        currentPage = Mathf.Max(0, totalPages - 1);
        Log($"MainMenu: All levels played, starting on last page {currentPage + 1}");
    }
    
    private IEnumerator SetupLevelCards()
    {
        // Validate required components
        if (levelCardPrefab == null || levelCardContainer == null)
        {
            Debug.LogError("MainMenu: Cannot setup level cards - missing required components!");
            yield break;
        }
        
        // Clear existing cards
        foreach (LevelCard card in levelCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }
        levelCards.Clear();
        
        // Wait one frame for Destroy to complete
        yield return null;
        
        // Create new cards at specific positions
        for (int i = 0; i < cardsPerPage; i++)
        {
            GameObject cardObj = Instantiate(levelCardPrefab, levelCardContainer);
            if (cardObj == null)
            {
                Debug.LogError($"MainMenu: Failed to instantiate level card {i}");
                continue;
            }
            
            LevelCard card = cardObj.GetComponent<LevelCard>();
            if (card == null)
            {
                card = cardObj.AddComponent<LevelCard>();
            }
            
            // Set position and scale
            if (i < cardPositions.Length)
            {
                cardObj.transform.localPosition = cardPositions[i];
            }
            else
            {
                Debug.LogWarning($"MainMenu: No position defined for card {i}, using default");
            }
            cardObj.transform.localScale = cardScale;
            
            levelCards.Add(card);
        }
        
        // Update card data
        UpdateLevelCards();
    }
    
    private void UpdateLevelCards()
    {
        int startIndex = currentPage * cardsPerPage;
        
        for (int i = 0; i < levelCards.Count; i++)
        {
            LevelCard card = levelCards[i];
            int levelIndex = startIndex + i;
            
            if (levelIndex < levelInfos.Count)
            {
                LevelInfo levelInfo = levelInfos[levelIndex];
                card.SetupCard(levelInfo.levelName, levelInfo.hasBeenPlayed, levelInfo.bestTime);
                card.gameObject.SetActive(true);
            }
            else
            {
                // Hide empty slots
                card.gameObject.SetActive(false);
            }
        }
    }
    
    private void UpdateNavigation()
    {
        // Prevent re-entrant calls to avoid stack overflow
        if (isUpdatingNavigation) return;
        
        isUpdatingNavigation = true;
        
        // Update page number text
        if (pageNumberText != null)
        {
            pageNumberText.text = $"{currentPage + 1} / {totalPages}";
        }
        
        // Hide/show left arrow button
        if (leftArrowButton != null)
        {
            bool canGoLeft = currentPage > 0;
            leftArrowButton.interactable = canGoLeft;
            leftArrowButton.gameObject.SetActive(canGoLeft || totalPages > 1); // Show if there are multiple pages
        }
        
        // Hide/show right arrow button
        if (rightArrowButton != null)
        {
            bool canGoRight = currentPage < totalPages - 1;
            rightArrowButton.interactable = canGoRight;
            rightArrowButton.gameObject.SetActive(canGoRight || totalPages > 1); // Show if there are multiple pages
        }
        
        // Hide entire navigation panel if there's only one page
        if (totalPages <= 1)
        {
            HideNavigationPanel();
        }
        else
        {
            ShowNavigationPanel();
        }
        
        isUpdatingNavigation = false;
    }
    
    private void HideNavigationPanel()
    {
        // Hide the entire navigation panel if there's only one page
        if (leftArrowButton != null && leftArrowButton.transform.parent != null)
        {
            leftArrowButton.transform.parent.gameObject.SetActive(false);
        }
        else if (pageNumberText != null && pageNumberText.transform.parent != null)
        {
            pageNumberText.transform.parent.gameObject.SetActive(false);
        }
    }
    
    private void ShowNavigationPanel()
    {
        // Show the entire navigation panel if there are multiple pages
        if (leftArrowButton != null && leftArrowButton.transform.parent != null)
        {
            leftArrowButton.transform.parent.gameObject.SetActive(true);
        }
        else if (pageNumberText != null && pageNumberText.transform.parent != null)
        {
            pageNumberText.transform.parent.gameObject.SetActive(true);
        }
    }
    
    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateLevelCards();
            UpdateNavigation();
            Log($"MainMenu: Navigated to page {currentPage + 1}");
        }
    }
    
    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            UpdateLevelCards();
            UpdateNavigation();
            Log($"MainMenu: Navigated to page {currentPage + 1}");
        }
    }
    
    public void LoadLevel(string levelName)
    {
        Log($"MainMenu: Loading level '{levelName}'");
        
        // Store the level name for the LevelManager to use
        PlayerPrefs.SetString("CurrentLevelName", levelName);
        PlayerPrefs.Save();
        
        // Load the base level scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(baseLevelSceneName);
    }
    
    public void OpenMultiplayer()
    {
        Log("MainMenu: Opening multiplayer menu");
        UnityEngine.SceneManagement.SceneManager.LoadScene(multiplayerSceneName);
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[MainMenu] {message}");
        }
    }
}
