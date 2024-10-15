using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public static class PostProcessingEx
{
    private static Dictionary<ChromaticAberration, CancellationTokenSource> chromaticAberrationTasks = new();
    private static Dictionary<Vignette, CancellationTokenSource> vignetteTasks = new();

    public static void Init()
    {
        chromaticAberrationTasks.Clear();
        vignetteTasks.Clear();
    }

    public static async UniTask CrossFadeIntensity(this ChromaticAberration chromaticAberration, float targetIntensity, float duration)
    {
        // 이전 태스크가 실행 중이면 취소
        if (chromaticAberrationTasks.TryGetValue(chromaticAberration, out CancellationTokenSource cancellationTokenSource))
        {
            cancellationTokenSource.Cancel(); // 기존 태스크 취소
            cancellationTokenSource.Dispose(); // 메모리 해제
        }

        // 새 태스크를 위한 CancellationTokenSource 생성
        cancellationTokenSource = new CancellationTokenSource();
        chromaticAberrationTasks[chromaticAberration] = cancellationTokenSource;

        // 크로스 페이드 시작
        try
        {
            chromaticAberration.active = true;

            float initialIntensity = chromaticAberration.intensity.value; // 현재 강도
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return; // 취소되면 바로 종료
                }

                // 시간에 따른 Lerp(선형 보간) 계산
                float newIntensity = Mathf.Lerp(initialIntensity, targetIntensity, timeElapsed / duration);
                chromaticAberration.intensity.value = newIntensity;

                timeElapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationTokenSource.Token);
            }

            // 최종 강도 설정
            chromaticAberration.intensity.value = targetIntensity;
        }
        catch (OperationCanceledException)
        {
            // 취소 시 처리할 내용이 있다면 추가
        }
        finally
        {
            chromaticAberrationTasks.Remove(chromaticAberration); // 작업이 끝난 후 목록에서 제거
            cancellationTokenSource.Dispose(); // 리소스 해제
        }
    }
    
    public static async UniTask CrossFade(this Vignette vignette, float duration=0.35f, float targetIntensity=-1, float targetSmoothness=-1)
    {
        if (vignetteTasks.TryGetValue(vignette, out CancellationTokenSource cancellationTokenSource))
        {
            cancellationTokenSource.Cancel();   
            cancellationTokenSource.Dispose();  
        }

        cancellationTokenSource = new CancellationTokenSource();
        vignetteTasks[vignette] = cancellationTokenSource;

        try
        {
            vignette.active = true;

            float initialIntensity = vignette.intensity.value;
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }

                if (targetIntensity != -1)
                    vignette.intensity.value = Mathf.Lerp(initialIntensity, targetIntensity, timeElapsed / duration);

                timeElapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationTokenSource.Token);
            }

            if (targetIntensity != -1)
                vignette.intensity.value = targetIntensity;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            vignetteTasks.Remove(vignette);
            cancellationTokenSource.Dispose();
        }
    }
}
