using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelExporter : MonoBehaviour
{
    [Header("Level Information")]
    public string levelName = "NewLevel";
    
    [Header("Objects to Export")]
    [Tooltip("Drag and drop GameObjects here that you want to include in the level export")]
    public List<GameObject> objectsToExport = new List<GameObject>();
    
    [Header("Player")]
    [Tooltip("The player object whose position will be exported")]
    public GameObject playerObject;
    
    [Header("Export Settings")]
    public string exportPath = "Levels";
    public bool prettyPrintJson = true;
    public bool autoFindPlayer = true;
    
    [Header("Debug")]
    public bool showDebugLogs = true;
    
#if UNITY_EDITOR
    public void ExportLevel()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("LevelExporter: Level name is empty!");
            return;
        }
        
        if (objectsToExport.Count == 0)
        {
            Debug.LogError("LevelExporter: No objects to export! Drag objects into the 'Objects To Export' list.");
            return;
        }
        
        Log("Starting level export...");
        
        // Auto-find player if enabled
        if (autoFindPlayer && playerObject == null)
        {
            playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                Log("Auto-found player object");
            }
        }
        
        // Create level data
        LevelData levelData = new LevelData();
        
        // Export the objects in the list
        ExportObjects(ref levelData);
        
        // Export player transform
        ExportPlayerTransform(ref levelData);
        
        // Export button connections
        ExportButtonConnections(ref levelData);
        
        // Export spring joints
        ExportSpringJoints(ref levelData);
        
        // Export player settings
        ExportPlayerSettings(ref levelData);
        
        // Save to JSON
        SaveToJson(levelData);
        
        Log($"✓ Level '{levelName}' exported successfully!");
        Debug.Log($"<color=green>✓ Exported {levelData.prefabs.Count} objects to {levelName}.json</color>");
    }
    
    [ContextMenu("Export Current Level")]
    public void ExportCurrentLevel()
    {
        ExportLevel();
    }
    
    private void ExportObjects(ref LevelData levelData)
    {
        Log("Exporting objects...");
        
        foreach (GameObject obj in objectsToExport)
        {
            if (obj == null) continue;
            
            // Skip player object (exported separately)
            if (obj == playerObject) continue;
            
            PrefabData prefabData = new PrefabData
            {
                prefabName = obj.name,
                prefabPath = GetPrefabBaseName(obj.name),
                position = obj.transform.position,
                rotation = obj.transform.eulerAngles,
                scale = obj.transform.localScale
            };
            
            levelData.prefabs.Add(prefabData);
            Log($"  ✓ {obj.name}");
        }
        
        Log($"Exported {levelData.prefabs.Count} objects");
    }
    
    private void ExportPlayerTransform(ref LevelData levelData)
    {
        Log("Exporting player transform...");
        
        GameObject player = playerObject;
        
        // Try to auto-find if not set
        if (player == null && autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player != null)
        {
            TransformData transformData = new TransformData
            {
                objectName = player.name,
                position = player.transform.position,
                rotation = player.transform.eulerAngles,
                scale = player.transform.localScale
            };
            
            levelData.transforms.Add(transformData);
            Log($"  ✓ Player position saved");
        }
        else
        {
            Debug.LogWarning("LevelExporter: No player object found!");
        }
    }
    
    private void ExportButtonConnections(ref LevelData levelData)
    {
        Log("Exporting button connections...");
        
        // Only export buttons that are in our objects list
        foreach (GameObject obj in objectsToExport)
        {
            if (obj == null) continue;
            
            ButtonController button = obj.GetComponent<ButtonController>();
            if (button == null) continue;
            
            ButtonConnectionData connectionData = new ButtonConnectionData
            {
                buttonName = button.gameObject.name,
                targetObjectNames = new List<string>(),
                isInvertible = button.IsInvertible(),
                detectionRadius = button.detectionRadius
            };
            
            foreach (GameObject target in button.targetObjects)
            {
                if (target != null)
                {
                    connectionData.targetObjectNames.Add(target.name);
                }
            }
            
            if (connectionData.targetObjectNames.Count > 0)
            {
                levelData.buttonConnections.Add(connectionData);
                Log($"  ✓ Button: {button.gameObject.name} → {connectionData.targetObjectNames.Count} targets");
            }
        }
        
        if (levelData.buttonConnections.Count > 0)
        {
            Log($"Exported {levelData.buttonConnections.Count} button connections");
        }
    }
    
    private void ExportSpringJoints(ref LevelData levelData)
    {
        Log("Exporting spring joints...");
        
        // Only export spring joints on objects in our list
        foreach (GameObject obj in objectsToExport)
        {
            if (obj == null) continue;
            
            SpringJoint2D joint = obj.GetComponent<SpringJoint2D>();
            if (joint == null) continue;
            
            SpringJointData jointData = new SpringJointData
            {
                objectName = joint.gameObject.name,
                connectedObjectName = joint.connectedBody != null ? joint.connectedBody.gameObject.name : "",
                anchor = joint.anchor,
                connectedAnchor = joint.connectedAnchor,
                autoConfigureConnectedAnchor = joint.autoConfigureConnectedAnchor,
                distance = joint.distance,
                dampingRatio = joint.dampingRatio,
                frequency = joint.frequency,
                enableCollision = joint.enableCollision
            };
            
            levelData.springJoints.Add(jointData);
            Log($"  ✓ Spring joint: {joint.gameObject.name}");
        }
        
        if (levelData.springJoints.Count > 0)
        {
            Log($"Exported {levelData.springJoints.Count} spring joints");
        }
    }
    
    private void ExportPlayerSettings(ref LevelData levelData)
    {
        Log("Exporting player settings...");
        
        GameObject player = playerObject;
        if (player == null && autoFindPlayer)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                levelData.playerSettings = new PlayerSettingsData
                {
                    maxLaunches = playerController.maxLaunches,
                    currentLaunches = playerController.currentLaunches,
                    infiniteDashes = playerController.infiniteDashes
                };
                
                Log($"  ✓ Dashes: {playerController.maxLaunches}, Infinite: {playerController.infiniteDashes}");
                return;
            }
        }
        
        // Default settings
        levelData.playerSettings = new PlayerSettingsData
        {
            maxLaunches = 3,
            currentLaunches = 0,
            infiniteDashes = false
        };
        
        Log("  Using default player settings");
    }
    
    private void SaveToJson(LevelData levelData)
    {
        // Create export wrapper with metadata
        ExportedLevel exportData = new ExportedLevel
        {
            levelName = levelName,
            version = "1.0",
            prefabs = levelData.prefabs,
            transforms = levelData.transforms,
            buttonConnections = levelData.buttonConnections,
            springJoints = levelData.springJoints,
            playerSettings = levelData.playerSettings,
            exportTimestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds()
        };
        
        string json = JsonUtility.ToJson(exportData, prettyPrintJson);
        
        // Ensure export directory exists
        string fullPath = Path.Combine(Application.dataPath, "..", exportPath);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        
        // Save to file
        string fileName = $"{levelName}.json";
        string filePath = Path.Combine(fullPath, fileName);
        File.WriteAllText(filePath, json);
        
        Log($"Level exported to: {filePath}");
        
        // Update levels manifest
        UpdateLevelsManifest(fullPath);
        
        AssetDatabase.Refresh();
    }
    
    private void UpdateLevelsManifest(string levelsDirectory)
    {
        Log("Updating levels manifest...");
        
        // Get all level JSON files
        string[] jsonFiles = Directory.GetFiles(levelsDirectory, "*.json")
            .Where(f => !f.EndsWith("levels.json"))
            .ToArray();
        
        LevelManifest manifest = new LevelManifest();
        
        foreach (string file in jsonFiles)
        {
            string fileName = Path.GetFileName(file);
            long timestamp = File.GetLastWriteTime(file).ToFileTime();
            
            manifest.levels.Add(new LevelManifestEntry
            {
                name = fileName,
                exportTimestamp = timestamp
            });
        }
        
        // Sort by timestamp
        manifest.levels = manifest.levels.OrderBy(l => l.exportTimestamp).ToList();
        
        // Save manifest
        string manifestJson = JsonUtility.ToJson(manifest, true);
        string manifestPath = Path.Combine(levelsDirectory, "levels.json");
        File.WriteAllText(manifestPath, manifestJson);
        
        Log($"Manifest updated with {manifest.levels.Count} levels");
    }
    
    private string GetPrefabBaseName(string objectName)
    {
        // Remove instance numbers like " (1)", " (2)", etc.
        if (objectName.Contains(" ("))
        {
            int index = objectName.IndexOf(" (");
            return objectName.Substring(0, index);
        }
        
        return objectName;
    }
    
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[LevelExporter] {message}");
        }
    }
#endif
    
    [System.Serializable]
    public class ExportedLevel
    {
        public string levelName;
        public string version;
        public List<PrefabData> prefabs;
        public List<TransformData> transforms;
        public List<ButtonConnectionData> buttonConnections;
        public List<SpringJointData> springJoints;
        public PlayerSettingsData playerSettings;
        public long exportTimestamp;
    }
    
    [System.Serializable]
    public class LevelManifest
    {
        public List<LevelManifestEntry> levels = new List<LevelManifestEntry>();
    }
    
    [System.Serializable]
    public class LevelManifestEntry
    {
        public string name;
        public long exportTimestamp;
    }
}

