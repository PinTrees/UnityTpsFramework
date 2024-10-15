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
        // ���� �½�ũ�� ���� ���̸� ���
        if (chromaticAberrationTasks.TryGetValue(chromaticAberration, out CancellationTokenSource cancellationTokenSource))
        {
            cancellationTokenSource.Cancel(); // ���� �½�ũ ���
            cancellationTokenSource.Dispose(); // �޸� ����
        }

        // �� �½�ũ�� ���� CancellationTokenSource ����
        cancellationTokenSource = new CancellationTokenSource();
        chromaticAberrationTasks[chromaticAberration] = cancellationTokenSource;

        // ũ�ν� ���̵� ����
        try
        {
            chromaticAberration.active = true;

            float initialIntensity = chromaticAberration.intensity.value; // ���� ����
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    return; // ��ҵǸ� �ٷ� ����
                }

                // �ð��� ���� Lerp(���� ����) ���
                float newIntensity = Mathf.Lerp(initialIntensity, targetIntensity, timeElapsed / duration);
                chromaticAberration.intensity.value = newIntensity;

                timeElapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationTokenSource.Token);
            }

            // ���� ���� ����
            chromaticAberration.intensity.value = targetIntensity;
        }
        catch (OperationCanceledException)
        {
            // ��� �� ó���� ������ �ִٸ� �߰�
        }
        finally
        {
            chromaticAberrationTasks.Remove(chromaticAberration); // �۾��� ���� �� ��Ͽ��� ����
            cancellationTokenSource.Dispose(); // ���ҽ� ����
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
