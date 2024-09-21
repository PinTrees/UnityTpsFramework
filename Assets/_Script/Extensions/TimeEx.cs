using Cysharp.Threading.Tasks;
using System.Collections;
using System.Threading;
using UnityEngine;

public static class TimeEx
{
    public static void Init()
    {
        cts?.Cancel(); 
    }

    public static async UniTaskVoid Stop(float duration = 0.1f)
    {
        await UniTask.Delay(100);
        Time.timeScale = 0;
        await UniTask.Delay((int)(duration * 1000), DelayType.UnscaledDeltaTime);
        Time.timeScale = 1;
    }

    private static CancellationTokenSource cts;
    public static async UniTaskVoid CrossFadeTimeScale(float targetTimeScale, float duration)
    {
        // ���� �۾��� �ִٸ� ���
        cts?.Cancel();
        cts = new CancellationTokenSource();
        var token = cts.Token;

        var startTimeScale = Time.timeScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // �ð��� ����ʿ� ���� Ÿ�ӽ����� ����
            elapsedTime += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(startTimeScale, targetTimeScale, elapsedTime / duration);

            // ��ū�� ��ҵǾ����� Ȯ��
            if (token.IsCancellationRequested)
            {
                return; // �۾��� ��ҵǸ� ����
            }

            await UniTask.Yield();
        }

        // ��Ȯ�ϰ� ��ǥ Ÿ�ӽ����Ϸ� ����
        Time.timeScale = targetTimeScale;
    }
}
