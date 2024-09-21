using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public enum ScriptableAnimatorAvataMaskType
{
    None,
    Arm,
    Upper,
    Lower,
    Head,
}

[Serializable]
public class SubScriptableAnimatorSetting : ScriptableObject
{
    public SubScriptableAnimatorSetting overrideAnimatorSetting;
    public ScriptableAnimatorAvataMaskType overrideAvataMaskType;
    public AvatarMask overrideAvataMask;

#if UNITY_EDITOR
    public virtual void _Editor_CreateAnimator(AnimatorController animatorController, AnimatorControllerLayer animatorLayer)
    {
    }
#endif
}
