using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using System.Collections.Generic;

public static class AnimatorLayerWeightEx
{
    private static Dictionary<Animator, CancellationTokenSource> cancellationTokenSources = new();
    private const float default_transition_duration = 0.35f;

    public static void Init()
    {
        cancellationTokenSources.Clear();
    }

    public static void CrossFadeLayerWeight(this Animator animator, int layerIndex, float targetWeight, float duration=default_transition_duration)
    {
        // ���� ���� �۾��� �ִٸ� ���
        if (cancellationTokenSources.ContainsKey(animator))
        {
            cancellationTokenSources[animator]?.Cancel();
            cancellationTokenSources.Remove(animator);
        }

        // ���ο� �۾��� ���� CancellationTokenSource ����
        var cts = new CancellationTokenSource();
        cancellationTokenSources[animator] = cts;
        var token = cts.Token;

        // �񵿱������� ���̾� ����ġ�� ����
        CrossFadeLayerWeightAsync(animator, layerIndex, targetWeight, duration, token).Forget();
    }

    private static async UniTaskVoid CrossFadeLayerWeightAsync(Animator animator, int layerIndex, float targetWeight, float duration, CancellationToken token)
    {
        float startWeight = animator.GetLayerWeight(layerIndex);
        float time = 0f;

        try
        {
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float currentWeight = Mathf.Lerp(startWeight, targetWeight, t);
                animator.SetLayerWeight(layerIndex, currentWeight);

                token.ThrowIfCancellationRequested();
                await UniTask.Yield(token);
            }

            // �������� ��Ȯ�� ��ǥ ����ġ�� ����
            animator.SetLayerWeight(layerIndex, targetWeight);
        }
        catch (OperationCanceledException)
        {
            // �۾��� ��ҵ� ��� ó��
            Debug.Log("CrossFadeLayerWeight operation was canceled.");
        }
        finally
        {
            cancellationTokenSources.Remove(animator);
        }
    }
}
