using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

// https://assetstore.unity.com/publishers/37262
using FIMSpace.FProceduralAnimation;

public static class LegsAnimatorEx
{
    private static Dictionary<LegsAnimator, bool> isCrossFadingMap = new();
    private static Dictionary<LegsAnimator, float> blendMap = new();
    public static float blendingSpeed = 3.5f;

    public static void Init()
    {
        isCrossFadingMap.Clear();
        blendMap.Clear();
    }

    public static void CrossFadeActive(this LegsAnimator animator, bool active)
    {
        if(!blendMap.ContainsKey(animator))
        {
            blendMap[animator] = 0f;
            isCrossFadingMap[animator] = false;
        }

        blendMap[animator] = active ? 1.0f : 0.00f;

        if (IsCrossFading(animator))
            return;

        SetCrossFading(animator, true);
        CrossFadeDeltaTime(animator).Forget();
    }

    public static async UniTaskVoid CrossFadeDeltaTime(LegsAnimator animator)
    {
        float epsilon = 0.01f;

        while(true)
        {
            animator.LegsAnimatorBlend = Mathf.Lerp(
                animator.LegsAnimatorBlend,
                blendMap[animator],
                blendingSpeed * Time.deltaTime);

            bool isExit = true;

            if (Mathf.Abs(animator.LegsAnimatorBlend - blendMap[animator]) > epsilon)
                isExit = false;

            if (isExit)
                break;

            await UniTask.Yield();
        }

        SetCrossFading(animator, false);
    }

    private static bool IsCrossFading(LegsAnimator animator)
    {
        if (isCrossFadingMap.TryGetValue(animator, out bool isCrossFading))
        {
            return isCrossFading;
        }
        return false;
    }

    private static void SetCrossFading(LegsAnimator animator, bool value)
    {
        isCrossFadingMap[animator] = value;
    }
}
