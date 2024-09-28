using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorEx
{
    public static Dictionary<Animator, bool> animatorLockedMap = new();

    public static void Init()
    {
        animatorLockedMap.Clear();
    }
    public static void CrossFadePlay(this Animator animator, string stateName, float duration)
    {
        if (animator.IsLocked())
            return;
        animator.CrossFadeInFixedTime(stateName, duration);
    }
    public static bool IsLocked(this Animator animator)
    {
        if (!animatorLockedMap.ContainsKey(animator)) return false;
        return animatorLockedMap[animator];
    }
    public static void SetLocked(this Animator animator, bool active)
    {
        animatorLockedMap[animator] = active;
    }

    public static async UniTask WaitMustTransitionCompleteAsync(this Animator animator, string state_name, string state_tag="") => 
        await animator.WaitForStateToStart(state_name, state_tag);
    //public static async UniTask TransitionCompleteAsync(this Animator animator, string state_name, string state_tag, float normalizeTime)
    //    =>  await animator.WaitForStateToEnd(state_name, state_tag, normalizeTime); 
     
    private static IEnumerator WaitForStateToStart(this Animator animator, string state_name, string state_tag="")
    {
        if (state_tag == "")
            state_tag = state_name;

        if (animator.IsPlaying(state_tag, 0))
            yield break;

        while (animator.IsLocked())
            yield return null;
        
        animator.SetLocked(true);

        animator.CrossFadeInFixedTime(state_name, 0.15f, 0);
        while (true)
        {
            if (animator.IsPlaying(state_tag, 0))
                break;
            yield return null;  
        }

        animator.SetLocked(false);
        yield break;
    }
    private static IEnumerator WaitForStateToEnd(this Animator animator, string state, float normalizeTime)
    {
        while (!animator.IsPlayedOverTime(state, normalizeTime, 0))
            yield return null;  // 코루틴으로 대기
    }


    public static IEnumerator TransitionCompleteCorutine(this Animator animator, string state, float normalizeTime = 0.15f)
    {
        while (true)
        {
            if (animator.IsPlayedInTime(state, 0, normalizeTime))
                break;

            yield return null;
        }
    }

    private static void set_animation_clip(AnimatorOverrideController controller, string stateName, AnimationClip clip)
    {
        controller[stateName] = clip;
        //Debug.Log(controller[stateName]);
    }

    public static void SetAnimationClip(this Animator animator, string stateName, AnimationClip clip)
    {
        if (animator.runtimeAnimatorController is AnimatorOverrideController overrideController)
        {
            set_animation_clip(overrideController, stateName, clip);
            //animator.runtimeAnimatorController = overrideController; 
        }
        else
        {
            //var newOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            //set_animation_clip(newOverrideController, stateName, clip);

            Debug.LogError("[Animator-Ex] Animator controller is not an AnimatorOverrideController.");
        }

        // 강제 초기화
        animator.Rebind();
        //Debug.Log($"[Animator-Ex] {stateName} motion successfully changed. {clip.name}");
    }

    public static void SetAnimationClip(this Animator animator, AnimationClip clip)
    {
        ((AnimatorOverrideController)animator.runtimeAnimatorController)["Base Layer.DefaultState"] = clip;
    }


    public static void CrossFadeAnimatorController(this Animator animator, RuntimeAnimatorController runtimeAnimatorController)
    {
        if (runtimeAnimatorController == null)
            return;

        if (animator.runtimeAnimatorController == runtimeAnimatorController)
            return;

        animator.runtimeAnimatorController = new AnimatorOverrideController(runtimeAnimatorController);
        animator.Update(Time.deltaTime);
    }

    public static void SetNormalizeTime(this Animator animator, string stateName, float point, int layerIndex = 0)
    {
        // Step 0: 오류 확인
        if (point < 0 || point > 1)
        {
            Debug.LogError("Normalized time 'point' must be between 0 and 1.");
            return;
        }

        // Step 1: 애니메이션 기초 세팅
        float preAnimationSpeed = animator.speed;
        bool preApplyRootMotion = animator.applyRootMotion;
        animator.applyRootMotion = false;
        animator.speed = 0.0001f;

        // Step 2: 애니메이터 상태를 즉시 업데이트
        animator.Play(stateName, layerIndex, point);
        animator.Update(Time.deltaTime);

        // Step 3: 재생 중지 후 복원
        animator.StopPlayback();
        animator.speed = preAnimationSpeed;
        animator.applyRootMotion = preApplyRootMotion;
    }

    // Action
    public static void Replay(this Animator animator)
    {
        float clipLength = animator.GetAnimationLenght();
        float newNormalizedTime = Mathf.Clamp01(0);
        float newTimeValue = newNormalizedTime * clipLength;
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, newTimeValue / clipLength);
    }



    // State
    public static float GetAnimationLenght(this Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }
    public static bool IsPlaying(this Animator animator, string tag, int layer=0)
    {
        return animator.GetCurrentAnimatorStateInfo(layer).IsTag(tag);
    }
    public static bool IsPlayedOverTime(this Animator animator, float normalizedTime, int layerIndex = 0)
    {
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime >= normalizedTime)
        {
            return true;
        }
        else return false;
    }
    public static bool IsPlayedOverTime(this Animator animator, string tag, float normalizedTime, int layerIndex = 0)
    {
        if (animator.GetCurrentAnimatorStateInfo(layerIndex).IsTag(tag)
         && animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime >= normalizedTime)
        {
            return true;
        }
        else return false;
    }
    public static bool IsPlayedInTime(this Animator animator, string tag, float start, float end)
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag(tag)
         && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= start
         && animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= end)
        {
            return true;
        }
        else return false;
    }
}
