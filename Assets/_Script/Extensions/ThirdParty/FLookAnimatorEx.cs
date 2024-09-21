using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

// https://assetstore.unity.com/publishers/37262
using FIMSpace.FLook;

public static class FLookAnimatorEx 
{
    // �� �ִϸ����� �ν��Ͻ����� isCrossFading ���¸� �����ϱ� ���� ��ųʸ�
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
            lookAnimatorWeights[0] = 1.00f;     // �밡��
            lookAnimatorWeights[1] = 0.20f;
            lookAnimatorWeights[2] = 0.25f;
            lookAnimatorWeights[3] = 0.50f;
            lookAnimatorWeights[4] = 0.50f;
        }
        else
        {
            lookAnimatorWeights[0] = 0.0f;      // �밡�� - �ν����Ϳ� ǥ�õ��� ����
            lookAnimatorWeights[1] = 0.0f;
            lookAnimatorWeights[2] = 0.0f;
            lookAnimatorWeights[3] = 0.0f;
            lookAnimatorWeights[4] = 0.0f;
        }

        // Ʈ���� �������� �ƴҶ��� ����
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
