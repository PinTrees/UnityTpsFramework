using UnityEngine;
using System;
using UnityEditor;


#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

[Serializable]
public class DodgeAnimatorSetting : SubScriptableAnimatorSetting
{
    [Header("Animation Clips")]
    public AnimationClip8Dir dodgeShort;
    public AnimationClip8Dir dodgeLong;
    public AnimationClip8Dir dodgeRoll;
    public AnimationClip8Dir dodgeSlide;

    [Header("Override Animation Idle Setting")]
    public bool useOverrideDodegSlideIdleAnimation;
    public ScriptableAnimatorAvataMaskType overrideDodgeSlideIdleAvatarType;
    public AvatarMask overrideDodgeSlideIdleAvatarMask;
    public AnimationClip overrideDodgeSlideIdle;

#if UNITY_EDITOR
    public override void _Editor_CreateAnimator(AnimatorController animatorController, AnimatorControllerLayer animatorLayer)
    {
        AnimatorControllerLayer overrideAnimatorLayer = animatorLayer;

        if (overrideAnimatorSetting != null)
        {
            overrideAnimatorSetting._Editor_CreateAnimator(animatorController, animatorLayer);

            overrideAnimatorLayer = animatorController.FindOrCreateLayer(overrideAvataMaskType.ToString(),
                overrideAvataMask);
        }

        if(useOverrideDodegSlideIdleAnimation)
        {
            var overrideIdleAnimatorLayer = animatorController.FindOrCreateLayer(overrideDodgeSlideIdleAvatarType.ToString(), overrideDodgeSlideIdleAvatarMask);
            overrideIdleAnimatorLayer.AddState(overrideDodgeSlideIdle, "DodgeSlide", "Dodge");
        }
        else
        {
            animatorController.CreateBlendTree(dodgeSlide, "DodgeSlide", "Dodge", overrideAnimatorLayer, paramXName: "dx", paramYName: "dy");
        }

        animatorController.CreateBlendTree(dodgeRoll, "DodgeRoll", "Dodge", overrideAnimatorLayer, paramXName: "dx", paramYName: "dy");
        animatorController.CreateBlendTree(dodgeShort, "DodgeShort", "Dodge", overrideAnimatorLayer, paramXName: "dx", paramYName: "dy");
        animatorController.CreateBlendTree(dodgeLong, "DodgeLong", "Dodge", overrideAnimatorLayer, paramXName: "dx", paramYName: "dy");
    }
#endif
}
