using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BiomeDataContainer
{
    [UID]
    public string uid;
    public GameObject prefab;
}

public class BiomeResource : ScriptableObject
{
    public List<BiomeDataContainer> prefabs = new();

    public Vector2 plantSize;
    public Vector3 maxScale;
    public Vector3 minScale;
    public bool useLockAspectRatio = false;


}
