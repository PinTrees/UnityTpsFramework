using System;
using UnityEngine;

[Serializable]
public class DefenseAnimatorSetting : SubScriptableAnimatorSetting
{
    [Header("Defense Animaiton Setting")]
    public bool useBaseAnimation;
    public AnimationClip guardAnimation;
    public AnimationClip parryAnimation;


}
