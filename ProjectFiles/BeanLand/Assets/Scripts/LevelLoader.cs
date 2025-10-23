using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LevelLoader : MonoBehaviour
{
    [Header("Level Loading Settings")]
    [SerializeField] private string currentLevelName = "";
    [SerializeField] private bool loadOnStart = false;
    [SerializeField] private string defaultLevelName = "DefaultLevel";
    
    [Header("Error Handling")]
    [SerializeField] private bool redirectOnLoadFailure = true;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float errorRedirectDelay = 2f;
    
    [Header("Prefab Management")]
    [SerializeField] private Transform prefabParent;
    [SerializeField] public bool clearExistingLevel = true;
    [SerializeField] private bool preserveNonLevelObjects = true;
    
    [Header("Loading Options")]
    [SerializeField] private bool showLoadingProgress = true;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Web Loading")]
    [SerializeField] private string webLevelsUrl = "https://beanbox.netlify.app/Levels/";
    
    [Header("Debug")]
    [SerializeField] private List<GameObject> loadedObjects = new List<GameObject>();
    
    private LevelData currentLevelData;
    private bool isLoading = false;

    private void Start()
    {
        if (prefabParent == null)
        {
            prefabParent = transform;
        }
        
        if (loadOnStart && !string.IsNullOrEmpty(currentLevelName))
        {
            LoadLevel(currentLevelName);
        }
    }

    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[LevelLoader] {message}");
        }
    }

    public void LoadLevel(string levelName)
    {
        if (isLoading)
        {
            Log("Already loading a level, please wait...");
            return;
        }
        
        StartCoroutine(LoadLevelCoroutine(levelName));
    }

    private IEnumerator LoadLevelCoroutine(string levelName)
    {
        isLoading = true;
        currentLevelName = levelName;
        
        Log($"Starting to load level: {levelName}");
        
        // Clear existing level if requested
        if (clearExistingLevel)
        {
            ClearExistingLevel();
        }
        
        // Always load from web
        LevelData levelData = null;
        Log("Loading level from website...");
                yield return StartCoroutine(LoadLevelFromWeb(levelName, (result) => levelData = result));
        
        if (levelData == null)
        {
            Debug.LogError($"[LevelLoader] Failed to load level: {levelName}");
            
            if (redirectOnLoadFailure)
            {
                Debug.LogError($"[LevelLoader] Redirecting to main menu in {errorRedirectDelay} seconds...");
                
                // Wait before redirecting
                yield return new WaitForSeconds(errorRedirectDelay);
                
                // Redirect to main menu
                isLoading = false;
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                Debug.LogWarning("[LevelLoader] Redirect on failure is disabled. Staying on current scene.");
            isLoading = false;
            }
            
            yield break;
        }
        
        currentLevelData = levelData;
        
        // Load prefabs
        yield return StartCoroutine(LoadPrefabs(levelData));
        
        // Apply transforms
        yield return StartCoroutine(ApplyTransforms(levelData));
        
        // Apply button connections
        yield return StartCoroutine(ApplyButtonConnections(levelData));
        
        // Apply spring joints
        yield return StartCoroutine(ApplySpringJoints(levelData));
        
        // Apply player settings
        ApplyPlayerSettings(levelData);
        
        Log($"Level '{levelName}' loaded successfully!");
        Log($"Loaded {levelData.prefabs.Count} prefabs, applied {levelData.transforms.Count} transforms, {levelData.buttonConnections.Count} button connections, {levelData.springJoints.Count} spring joints, and player settings");
        
        // Show success message
        Debug.Log($"<color=green>Level '{levelName}' loaded successfully!</color>");
        
        isLoading = false;
    }

    private IEnumerator LoadLevelFromWeb(string levelName, System.Action<LevelData> callback)
    {
        string url = $"{webLevelsUrl}{levelName}.json";
        Log($"Loading level from web: {url}");
        Debug.Log($"<color=cyan>[LevelLoader] WebGL Build - Fetching: {url}</color>");
        Debug.Log($"<color=cyan>[LevelLoader] Platform: {Application.platform}</color>");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Add timeout for WebGL builds
            request.timeout = 30;
            
            yield return request.SendWebRequest();
            
            Debug.Log($"<color=cyan>[LevelLoader] Request completed. Result: {request.result}</color>");
            Debug.Log($"<color=cyan>[LevelLoader] Response Code: {request.responseCode}</color>");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string json = request.downloadHandler.text;
                    Debug.Log($"<color=cyan>[LevelLoader] Received JSON length: {json.Length} characters</color>");
                    Debug.Log($"<color=cyan>[LevelLoader] JSON Preview: {json.Substring(0, Mathf.Min(200, json.Length))}</color>");
                    
                    LevelData levelData = JsonUtility.FromJson<LevelData>(json);
                    
                    if (levelData != null)
                    {
                    Log($"Successfully loaded level data from web");
                        Debug.Log($"<color=green>[LevelLoader] Successfully parsed level data!</color>");
                    callback?.Invoke(levelData);
                    }
                    else
                    {
                        Debug.LogError("[LevelLoader] Level data is null after parsing!");
                        callback?.Invoke(null);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LevelLoader] Failed to parse web level data: {e.Message}");
                    Debug.LogError($"[LevelLoader] Stack trace: {e.StackTrace}");
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"[LevelLoader] Failed to load level from web!");
                Debug.LogError($"[LevelLoader] Error: {request.error}");
                Debug.LogError($"[LevelLoader] Response Code: {request.responseCode}");
                Debug.LogError($"[LevelLoader] URL: {url}");
                
                // Check for specific error types
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("[LevelLoader] Connection Error - Check network connection");
                }
                else if (request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("[LevelLoader] Protocol Error - Server returned an error (possibly CORS issue)");
                }
                else if (request.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError("[LevelLoader] Data Processing Error");
                }
                
                callback?.Invoke(null);
            }
        }
    }


    private IEnumerator LoadPrefabs(LevelData levelData)
    {
        if (showLoadingProgress)
        {
            Log($"Loading {levelData.prefabs.Count} prefabs...");
        }
        
        foreach (PrefabData prefabData in levelData.prefabs)
        {
            try
            {
                GameObject prefab = LoadPrefab(prefabData.prefabPath);
                
                if (prefab != null)
                {
                    GameObject instance = Instantiate(prefab, prefabParent);
                    instance.name = prefabData.prefabName;
                    instance.transform.position = prefabData.position;
                    instance.transform.eulerAngles = prefabData.rotation;
                    instance.transform.localScale = prefabData.scale;
                    
                    loadedObjects.Add(instance);
                    Log($"Instantiated prefab: {prefabData.prefabName}");
                }
                else
                {
                    Log($"Warning: Could not load prefab: {prefabData.prefabPath}");
                }
            }
            catch (Exception e)
            {
                Log($"Error loading prefab {prefabData.prefabName}: {e.Message}");
            }
        }
        
        yield return null; // Required for coroutine
    }

    private IEnumerator ApplyTransforms(LevelData levelData)
    {
        if (showLoadingProgress)
        {
            Log($"Applying {levelData.transforms.Count} transforms...");
        }
        
        foreach (TransformData transformData in levelData.transforms)
        {
            try
            {
                GameObject targetObject = FindObjectByName(transformData.objectName);
                
                if (targetObject != null)
                {
                    targetObject.transform.position = transformData.position;
                    targetObject.transform.eulerAngles = transformData.rotation;
                    targetObject.transform.localScale = transformData.scale;
                    
                    Log($"Applied transform to: {transformData.objectName}");
                }
                else
                {
                    Log($"Warning: Could not find object to transform: {transformData.objectName}");
                }
            }
            catch (Exception e)
            {
                Log($"Error applying transform to {transformData.objectName}: {e.Message}");
            }
        }
        
        yield return null; // Required for coroutine
    }

    private IEnumerator ApplyButtonConnections(LevelData levelData)
    {
        if (showLoadingProgress)
        {
            Log($"Applying {levelData.buttonConnections.Count} button connections...");
        }
        
        foreach (ButtonConnectionData buttonData in levelData.buttonConnections)
        {
            try
            {
                GameObject buttonObject = FindObjectByName(buttonData.buttonName);
                
                if (buttonObject != null)
                {
                    ButtonController buttonController = buttonObject.GetComponent<ButtonController>();
                    if (buttonController != null)
                    {
                        // Clear existing target objects
                        buttonController.targetObjects.Clear();
                        
                        // Add target objects by name
                        foreach (string targetName in buttonData.targetObjectNames)
                        {
                            GameObject targetObject = FindObjectByName(targetName);
                            if (targetObject != null)
                            {
                                buttonController.AddTargetObject(targetObject);
                                Log($"Connected button {buttonData.buttonName} to {targetName}");
                            }
                            else
                            {
                                Log($"Warning: Could not find target object {targetName} for button {buttonData.buttonName}");
                            }
                        }
                        
                        // Set button properties
                        buttonController.SetInvertible(buttonData.isInvertible);
                        buttonController.detectionRadius = buttonData.detectionRadius;
                        
                        Log($"Applied button connections for: {buttonData.buttonName}");
                    }
                    else
                    {
                        Log($"Warning: ButtonController component not found on {buttonData.buttonName}");
                    }
                }
                else
                {
                    Log($"Warning: Could not find button object: {buttonData.buttonName}");
                }
            }
            catch (Exception e)
            {
                Log($"Error applying button connections for {buttonData.buttonName}: {e.Message}");
            }
        }
        
        yield return null; // Required for coroutine
    }

    private IEnumerator ApplySpringJoints(LevelData levelData)
    {
        if (showLoadingProgress)
        {
            Log("Applying spring joint connections...");
        }
        
        foreach (var springJointData in levelData.springJoints)
        {
            try
            {
                // Find the object that should have the spring joint
                GameObject sourceObject = GameObject.Find(springJointData.objectName);
                if (sourceObject != null)
                {
                    // Find the connected object
                    GameObject connectedObject = null;
                    if (!string.IsNullOrEmpty(springJointData.connectedObjectName))
                    {
                        connectedObject = GameObject.Find(springJointData.connectedObjectName);
                    }
                    
                    // Get or add SpringJoint2D component
                    SpringJoint2D springJoint = sourceObject.GetComponent<SpringJoint2D>();
                    if (springJoint == null)
                    {
                        springJoint = sourceObject.AddComponent<SpringJoint2D>();
                    }
                    
                    // Configure the spring joint
                    springJoint.anchor = springJointData.anchor;
                    springJoint.connectedAnchor = springJointData.connectedAnchor;
                    springJoint.autoConfigureConnectedAnchor = springJointData.autoConfigureConnectedAnchor;
                    springJoint.distance = springJointData.distance;
                    springJoint.dampingRatio = springJointData.dampingRatio;
                    springJoint.frequency = springJointData.frequency;
                    springJoint.enableCollision = springJointData.enableCollision;
                    
                    // Connect to the target rigidbody if it exists
                    if (connectedObject != null)
                    {
                        Rigidbody2D connectedRigidbody = connectedObject.GetComponent<Rigidbody2D>();
                        if (connectedRigidbody != null)
                        {
                            springJoint.connectedBody = connectedRigidbody;
                            Log($"Applied spring joint: {springJointData.objectName} connected to {springJointData.connectedObjectName}");
                        }
                        else
                        {
                            Log($"Warning: Connected object {springJointData.connectedObjectName} has no Rigidbody2D");
                            springJoint.connectedBody = null;
                        }
                    }
                    else
                    {
                        Log($"Warning: Could not find connected object: {springJointData.connectedObjectName}");
                        springJoint.connectedBody = null;
                    }
                }
                else
                {
                    Log($"Warning: Could not find object for spring joint: {springJointData.objectName}");
                }
            }
            catch (Exception e)
            {
                Log($"Error applying spring joint for {springJointData.objectName}: {e.Message}");
            }
        }
        
        yield return null; // Required for coroutine
    }

    private void ApplyPlayerSettings(LevelData levelData)
    {
        if (levelData.playerSettings == null)
        {
            Log("No player settings in level data, using defaults");
            return;
        }
        
        // Find the PlayerController
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        
        if (playerController != null)
        {
            playerController.maxLaunches = levelData.playerSettings.maxLaunches;
            playerController.currentLaunches = levelData.playerSettings.currentLaunches;
            playerController.infiniteDashes = levelData.playerSettings.infiniteDashes;
            
            // Update dash UI to reflect new settings
            playerController.ResetLaunches(); // This will update the UI
            
            Log($"Applied player settings: maxLaunches={levelData.playerSettings.maxLaunches}, infiniteDashes={levelData.playerSettings.infiniteDashes}");
        }
        else
        {
            Log("Warning: Could not find PlayerController to apply settings");
        }
    }

    private GameObject LoadPrefab(string prefabPath)
    {
        GameObject prefab = null;
        
        Log($"Attempting to load prefab: {prefabPath}");
        
        // For WebGL builds, prefabs must be in Resources folder
        // Try to convert Assets/Prefabs/ to Resources/ path
        if (prefabPath.Contains("Assets/Prefabs/"))
        {
            // Extract just the prefab name without extension
            string[] prefabPathParts = prefabPath.Split('/');
            string prefabName = prefabPathParts[prefabPathParts.Length - 1];
            prefabName = prefabName.Replace(".prefab", "");
            
            // Try loading from Resources with just the prefab name
            prefab = Resources.Load<GameObject>(prefabName);
            if (prefab != null)
            {
                Log($"Loaded prefab from Resources: {prefabName}");
                return prefab;
            }
            
            // Try loading from Resources/Prefabs/
            prefab = Resources.Load<GameObject>($"Prefabs/{prefabName}");
            if (prefab != null)
            {
                Log($"Loaded prefab from Resources/Prefabs: {prefabName}");
                return prefab;
            }
        }
        
        // First, try to load from Resources (most common for runtime loading)
        if (!prefabPath.Contains("Assets/"))
        {
            // This is already a Resources path
            prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab != null)
            {
                Log($"Loaded prefab from Resources: {prefabPath}");
                return prefab;
            }
        }
        
        // Try to convert asset path to Resources path
        if (prefabPath.Contains("Resources/"))
        {
            string resourcePath = prefabPath.Substring(prefabPath.IndexOf("Resources/") + 10);
            resourcePath = resourcePath.Replace(".prefab", "");
            prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab != null)
            {
                Log($"Loaded prefab from Resources (converted): {resourcePath}");
                return prefab;
            }
        }
        
        // Try to load using AssetDatabase in editor
        #if UNITY_EDITOR
        if (prefabPath.Contains("Assets/"))
        {
            prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                Log($"Loaded prefab from AssetDatabase: {prefabPath}");
                return prefab;
            }
        }
        #endif
        
        // Fallback: try to find by name in scene (for non-prefab objects)
        string[] pathParts = prefabPath.Split('/');
        string objectName = pathParts[pathParts.Length - 1];
        objectName = objectName.Replace(".prefab", "");
        GameObject sceneObject = GameObject.Find(objectName);
        if (sceneObject != null)
        {
            Log($"Found scene object by name: {sceneObject.name}");
            return sceneObject;
        }
        
        Debug.LogError($"[LevelLoader] Failed to load prefab: {prefabPath}");
        Debug.LogError($"[LevelLoader] Attempted prefab name: {objectName}");
        Debug.LogError($"[LevelLoader] Make sure prefab '{objectName}' is in a Resources folder for WebGL builds!");
        Debug.LogError($"[LevelLoader] Or ensure the object exists in the base scene if it shouldn't be a prefab.");
        return null;
    }

    private GameObject FindObjectByName(string objectName)
    {
        // First check loaded objects
        foreach (GameObject obj in loadedObjects)
        {
            if (obj != null && obj.name == objectName)
            {
                return obj;
            }
        }
        
        // Then check scene objects
        GameObject sceneObject = GameObject.Find(objectName);
        if (sceneObject != null)
        {
            return sceneObject;
        }
        
        return null;
    }

    private void ClearExistingLevel()
    {
        Log("Clearing existing level objects...");
        
        // Remove loaded prefab objects
        foreach (GameObject obj in loadedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        
        loadedObjects.Clear();
        
        // Clear children of prefab parent if not preserving
        if (!preserveNonLevelObjects && prefabParent != null)
        {
            for (int i = prefabParent.childCount - 1; i >= 0; i--)
            {
                Transform child = prefabParent.GetChild(i);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        Log("Existing level cleared");
    }

    [ContextMenu("Load Default Level")]
    public void LoadDefaultLevel()
    {
        LoadLevel(defaultLevelName);
    }

    [ContextMenu("Reload Current Level")]
    public void ReloadCurrentLevel()
    {
        if (!string.IsNullOrEmpty(currentLevelName))
        {
            LoadLevel(currentLevelName);
        }
        else
        {
            Log("No current level to reload");
        }
    }

    [ContextMenu("Clear Current Level")]
    public void ClearCurrentLevel()
    {
        ClearExistingLevel();
        currentLevelName = "";
        currentLevelData = null;
        Log("Current level cleared");
    }

    [ContextMenu("List Available Levels")]
    public void ListAvailableLevels()
    {
        Log("=== Available Levels ===");
        Log("Levels are loaded from web manifest at: " + webLevelsUrl);
        Log("Check the levels.json file at the website for the complete list.");
        Log("=== End List ===");
    }

    // Public methods for external calls (e.g., from JavaScript)
    public void LoadLevelFromWeb(string levelName, string webUrl)
    {
        webLevelsUrl = webUrl;
        LoadLevel(levelName);
    }

    public string GetCurrentLevelName()
    {
        return currentLevelName;
    }

    public bool IsLoading()
    {
        return isLoading;
    }

    public int GetLoadedObjectCount()
    {
        return loadedObjects.Count;
    }

    private void OnValidate()
    {
        // Clean up null references
        loadedObjects.RemoveAll(obj => obj == null);
        
        // Ensure prefab parent is set
        if (prefabParent == null)
        {
            prefabParent = transform;
        }
    }
    
    // Public method to set the current level name (called by LevelManager)
    public void SetCurrentLevelName(string levelName)
    {
        currentLevelName = levelName;
        Log($"LevelLoader: Level name set to '{currentLevelName}'");
        
        // If loadOnStart is enabled, load the level immediately
        if (loadOnStart && !string.IsNullOrEmpty(currentLevelName))
        {
            Log($"LevelLoader: Auto-loading level '{currentLevelName}'");
            LoadLevel(currentLevelName);
        }
    }
    
}
