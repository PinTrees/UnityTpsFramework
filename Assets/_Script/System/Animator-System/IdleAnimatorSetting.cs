using UnityEngine;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

[Serializable]
public class IdleAnimatorSetting : SubScriptableAnimatorSetting
{
    [Header("Idle Animation Setting")]
    public AnimationClip standIdle;
    public AnimationClip crouchIdle;
    public AnimationClip crawlIdle;

    public List<AnimationClip> standIdles = new(); 

#if UNITY_EDITOR
    public override void _Editor_CreateAnimator(AnimatorController animatorController, AnimatorControllerLayer animatorLayer) 
    {
        animatorLayer.AddState(standIdle, "StandIdle", "Idle");
        animatorLayer.AddState(crouchIdle, "CrouchIdle", "Idle");
        animatorLayer.AddState(crawlIdle, "CrawlIdle", "Idle");
    }
#endif
}
