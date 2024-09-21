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
        // 이전 작업이 있다면 취소
        cts?.Cancel();
        cts = new CancellationTokenSource();
        var token = cts.Token;

        var startTimeScale = Time.timeScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 시간이 경과됨에 따라 타임스케일 변경
            elapsedTime += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(startTimeScale, targetTimeScale, elapsedTime / duration);

            // 토큰이 취소되었는지 확인
            if (token.IsCancellationRequested)
            {
                return; // 작업이 취소되면 종료
            }

            await UniTask.Yield();
        }

        // 정확하게 목표 타임스케일로 설정
        Time.timeScale = targetTimeScale;
    }
}
