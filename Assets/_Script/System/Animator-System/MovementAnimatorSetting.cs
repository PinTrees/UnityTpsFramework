using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

[Serializable]
public class MovementAnimatorSetting : ScriptableObject
{
    public AnimationClip8Dir standWalk;
    public AnimationClip8Dir standRun;
    public AnimationClip standSprint;

    [Space]
    public MovementAnimatorSetting overrideAnimatorSetting;
    public AvatarMask overrideAvataMask;


#if UNITY_EDITOR
    public void _Editor_CreateAnimator(AnimatorController animatorController, AnimatorControllerLayer animatorLayer)
    {
        animatorController.CreateBlendTree(standWalk, "StandWalk", "Walk");
        animatorController.CreateBlendTree(standRun, "StandRun", "Run");
        animatorLayer.AddState(standSprint, "StandSprint", "Sprint");
    }
#endif
}
