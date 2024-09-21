using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public class BodyAnimatorSetting : SubScriptableAnimatorSetting
{
    [Header("Body Default Animation Setting")]
    public AnimationClip stand;
    public AnimationClip crouch;
    public AnimationClip crawl;

    [Header("Body Knockdown Animation Setting")]
    public AnimationClip knockdownUp;
    public AnimationClip knockdownDown;

    [Header("Body Transition Animation Setting")]
    public AnimationClip knockdownUpToStand;

    [Header("Death Animation Setting")]
    public AnimationClip death;

#if UNITY_EDITOR
    public override void _Editor_CreateAnimator(AnimatorController animatorController, AnimatorControllerLayer animatorLayer)
    {
        animatorLayer.AddState(stand, "Stand");
        animatorLayer.AddState(crouch, "Crouch");
        animatorLayer.AddState(crawl, "Crawl");

        animatorLayer.AddState(knockdownUp, "KnockDownUp");
        animatorLayer.AddState(knockdownDown, "KnockDownDown");

        animatorLayer.AddState(knockdownUpToStand, "KnockDownUpToStand");

        animatorLayer.AddState(death, "Death");
    }
#endif
}
