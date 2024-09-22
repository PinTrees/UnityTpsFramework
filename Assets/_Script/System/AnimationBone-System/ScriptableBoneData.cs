using System;
using System.Collections.Generic;
using UnityEngine;

public enum ScriptableBoneAxisType
{
    X,
    Y,
    Z,
    NEG_X,
    NEG_Y,
    NEG_Z
}

[Serializable]
public class ScriptableBone
{
    public ScriptableBoneAxisType upAxis;
    public ScriptableBoneAxisType forwardAxis;
    public HumanBodyBones targetBone;
}

[CreateAssetMenu(menuName = "Scriptable/BoneSystem")]
public class ScriptableBoneData : ScriptableObject
{
    public List<ScriptableBone> bones = new();
}
