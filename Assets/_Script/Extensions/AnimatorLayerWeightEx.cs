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
        // 진행 중인 작업이 있다면 취소
        if (cancellationTokenSources.ContainsKey(animator))
        {
            cancellationTokenSources[animator]?.Cancel();
            cancellationTokenSources.Remove(animator);
        }

        // 새로운 작업을 위한 CancellationTokenSource 생성
        var cts = new CancellationTokenSource();
        cancellationTokenSources[animator] = cts;
        var token = cts.Token;

        // 비동기적으로 레이어 가중치를 변경
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

            // 마지막에 정확히 목표 가중치로 설정
            animator.SetLayerWeight(layerIndex, targetWeight);
        }
        catch (OperationCanceledException)
        {
            // 작업이 취소된 경우 처리
            Debug.Log("CrossFadeLayerWeight operation was canceled.");
        }
        finally
        {
            cancellationTokenSources.Remove(animator);
        }
    }
}
