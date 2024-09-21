using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

// https://assetstore.unity.com/publishers/37262
using FIMSpace.FLook;

public static class FLookAnimatorEx 
{
    // 각 애니메이터 인스턴스마다 isCrossFading 상태를 추적하기 위한 딕셔너리
    private static Dictionary<int, bool> isCrossFadingDict = new();
    public static float[] lookAnimatorWeights = new float[5];
    public static float blendingSpeed = 2.5f;

    public static void Init()
    {
        isCrossFadingDict.Clear();
        lookAnimatorWeights = new float[5];
    }

    public static void CrossFadeActive(this FLookAnimator animator, bool active)
    {
        if(active)
        {
            lookAnimatorWeights[0] = 1.00f;     // 대가리
            lookAnimatorWeights[1] = 0.20f;
            lookAnimatorWeights[2] = 0.25f;
            lookAnimatorWeights[3] = 0.50f;
            lookAnimatorWeights[4] = 0.50f;
        }
        else
        {
            lookAnimatorWeights[0] = 0.0f;      // 대가리 - 인스펙터에 표시되지 않음
            lookAnimatorWeights[1] = 0.0f;
            lookAnimatorWeights[2] = 0.0f;
            lookAnimatorWeights[3] = 0.0f;
            lookAnimatorWeights[4] = 0.0f;
        }

        // 트윈이 실행중이 아닐때만 실행
        if (IsCrossFading(animator))
            return;

        SetCrossFading(animator, true);
        _ = CrossFadeDeltaTime(animator);
    }

    public static async UniTaskVoid CrossFadeDeltaTime(FLookAnimator animator)
    {
        float epsilon = 0.01f;

        while (true)
        {
            bool allWeightsConverged = true;

            for (int i = 0; i < lookAnimatorWeights.Length; ++i)
            {
                animator.LookBones[i].lookWeight = Mathf.Lerp(
                    animator.LookBones[i].lookWeight,
                    lookAnimatorWeights[i],
                    blendingSpeed * Time.deltaTime);

                if (Mathf.Abs(animator.LookBones[i].lookWeight - lookAnimatorWeights[i]) > epsilon)
                {
                    allWeightsConverged = false;
                }
            }

            if (allWeightsConverged)
                break;

            await UniTask.Yield();
        }

        SetCrossFading(animator, false);
    }

    private static bool IsCrossFading(FLookAnimator animator)
    {
        if (isCrossFadingDict.TryGetValue(animator.GetInstanceID(), out bool isCrossFading))
        {
            return isCrossFading;
        }
        return false;
    }

    private static void SetCrossFading(FLookAnimator animator, bool value)
    {
        isCrossFadingDict[animator.GetInstanceID()] = value;
    }
}
