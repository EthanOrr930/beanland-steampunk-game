using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public List<PrefabData> prefabs = new List<PrefabData>();
    public List<TransformData> transforms = new List<TransformData>();
    public List<ButtonConnectionData> buttonConnections = new List<ButtonConnectionData>();
    public List<SpringJointData> springJoints = new List<SpringJointData>();
    public PlayerSettingsData playerSettings;
}

[System.Serializable]
public class PrefabData
{
    public string prefabName;
    public string prefabPath;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

[System.Serializable]
public class TransformData
{
    public string objectName;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

[System.Serializable]
public class ButtonConnectionData
{
    public string buttonName;
    public List<string> targetObjectNames = new List<string>();
    public bool isInvertible;
    public float detectionRadius;
}

[System.Serializable]
public class SpringJointData
{
    public string objectName;
    public string connectedObjectName;
    public Vector2 anchor;
    public Vector2 connectedAnchor;
    public bool autoConfigureConnectedAnchor;
    public float distance;
    public float dampingRatio;
    public float frequency;
    public bool enableCollision;
}

[System.Serializable]
public class PlayerSettingsData
{
    public int maxLaunches = 3;
    public int currentLaunches = 3;
    public bool infiniteDashes = false;
}

