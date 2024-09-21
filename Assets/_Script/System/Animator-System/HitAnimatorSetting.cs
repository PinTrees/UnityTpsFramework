using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

[Serializable]
[CreateAssetMenu(menuName ="Scripatable/Animator/Hit")]
public class HitAnimatorSetting : SubScriptableAnimatorSetting
{
    [Header("Animation Clips")]
    public AnimationClip8Dir standHitLow;
    public AnimationClip8Dir standHitHard;

    public AnimationClip8Dir crouchHitLow;
    public AnimationClip8Dir crouchHitHard;

    public AnimationClip8Dir crawlHitLow;
    public AnimationClip8Dir crawlHitHard;


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

        animatorController.CreateBlendTree(standHitLow, "StandHitLow", "Hit", layer: overrideAnimatorLayer, paramXName: "hx", paramYName: "hy");
        animatorController.CreateBlendTree(standHitHard, "StandHitHard", "Hit", layer: overrideAnimatorLayer, paramXName: "hx", paramYName: "hy");
    }
#endif
}
