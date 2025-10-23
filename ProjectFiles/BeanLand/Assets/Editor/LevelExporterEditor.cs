using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelExporter))]
public class LevelExporterEditor : Editor
{
    private SerializedProperty levelNameProp;
    private SerializedProperty objectsToExportProp;
    private SerializedProperty playerObjectProp;
    private SerializedProperty exportPathProp;
    private SerializedProperty prettyPrintJsonProp;
    private SerializedProperty autoFindPlayerProp;
    private SerializedProperty showDebugLogsProp;
    
    private bool showSettings = false;
    
    private void OnEnable()
    {
        levelNameProp = serializedObject.FindProperty("levelName");
        objectsToExportProp = serializedObject.FindProperty("objectsToExport");
        playerObjectProp = serializedObject.FindProperty("playerObject");
        exportPathProp = serializedObject.FindProperty("exportPath");
        prettyPrintJsonProp = serializedObject.FindProperty("prettyPrintJson");
        autoFindPlayerProp = serializedObject.FindProperty("autoFindPlayer");
        showDebugLogsProp = serializedObject.FindProperty("showDebugLogs");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        LevelExporter exporter = (LevelExporter)target;
        
        // Title
        EditorGUILayout.Space(10);
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 14;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Level Exporter", titleStyle);
        EditorGUILayout.Space(5);
        
        // Info box
        EditorGUILayout.HelpBox("Drag GameObjects into the list below, then click Export to save the level as JSON.", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Level Name
        EditorGUILayout.LabelField("Level Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(levelNameProp, new GUIContent("Level Name"));
        EditorGUILayout.Space(10);
        
        // Objects to Export
        EditorGUILayout.LabelField("Objects to Export", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Drag and drop GameObjects here (walls, platforms, buttons, etc.)", MessageType.None);
        EditorGUILayout.PropertyField(objectsToExportProp, true);
        
        // Object count
        int objectCount = objectsToExportProp.arraySize;
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Total Objects: {objectCount}", EditorStyles.miniLabel);
        EditorGUILayout.Space(10);
        
        // Player
        EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(playerObjectProp, new GUIContent("Player Object"));
        EditorGUILayout.PropertyField(autoFindPlayerProp, new GUIContent("Auto-Find Player", "Automatically find player by tag if not assigned"));
        EditorGUILayout.Space(10);
        
        // Settings foldout
        showSettings = EditorGUILayout.Foldout(showSettings, "Export Settings", true, EditorStyles.foldoutHeader);
        if (showSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(exportPathProp, new GUIContent("Export Path"));
            EditorGUILayout.PropertyField(prettyPrintJsonProp, new GUIContent("Pretty Print JSON"));
            EditorGUILayout.PropertyField(showDebugLogsProp, new GUIContent("Show Debug Logs"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.Space(10);
        
        // Export button
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fixedHeight = 40;
        
        bool canExport = !string.IsNullOrEmpty(levelNameProp.stringValue) && objectCount > 0;
        GUI.enabled = canExport;
        
        if (GUILayout.Button("📦 Export Level", buttonStyle))
        {
            exporter.ExportLevel();
        }
        
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
        
        // Warning if nothing to export
        if (objectCount == 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("⚠ No objects to export! Add objects to the list above.", MessageType.Warning);
        }
        
        if (string.IsNullOrEmpty(levelNameProp.stringValue))
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("⚠ Level name is empty!", MessageType.Warning);
        }
        
        EditorGUILayout.Space(10);
        
        // Info about what will be exported
        if (objectCount > 0)
        {
            EditorGUILayout.LabelField("Export Summary:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"• {objectCount} GameObject(s)", EditorStyles.miniLabel);
            
            // Count buttons
            int buttonCount = 0;
            int springJointCount = 0;
            for (int i = 0; i < objectCount; i++)
            {
                GameObject obj = objectsToExportProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (obj != null)
                {
                    if (obj.GetComponent<ButtonController>() != null) buttonCount++;
                    if (obj.GetComponent<SpringJoint2D>() != null) springJointCount++;
                }
            }
            
            if (buttonCount > 0)
                EditorGUILayout.LabelField($"• {buttonCount} Button(s) with connections", EditorStyles.miniLabel);
            if (springJointCount > 0)
                EditorGUILayout.LabelField($"• {springJointCount} Spring Joint(s)", EditorStyles.miniLabel);
            
            EditorGUILayout.LabelField("• Player position & settings", EditorStyles.miniLabel);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}

