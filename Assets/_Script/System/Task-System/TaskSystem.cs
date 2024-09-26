using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;

public class TaskSystem : Singleton<TaskSystem>
{
    public static CoroutineRunner coroutineHost;

    public override void Init()
    {
        base.Init();
        if(coroutineHost)
        {
            StopAllCoroutines();
            Destroy(coroutineHost.gameObject);
        }

        var go = new GameObject();
        coroutineHost  = go.AddComponent<CoroutineRunner>();
    }

    public IEnumerator RunAsync(IEnumerator c)
    {
        yield return coroutineHost?.StartCoroutine(c);
    }

    public static IEnumerator Run(IEnumerator c)
    {
        yield return TaskSystem.Instance.RunAsync(c);
    }

    public static async UniTask UniTaskUpdate(Func<bool> action)
    {
        while(true)
        {
            await UniTask.Yield();
            if (action()) break;
        }
    }

    public static void CoroutineUpdateLost(Func<bool> action, float delay = 0, float timeout = 999, Action timeoutAction=null, Action onExit=null)
    {
        if (coroutineHost == null) return;

        if (delay > 0) coroutineHost.StartCoroutine(CoroutineUpdateWithDelay(action, delay, timeout, timeoutAction, onExit));
        else coroutineHost.StartCoroutine(CoroutineUpdate(action, timeout));
    }

    private static IEnumerator CoroutineUpdate(Func<bool> action, float timeout, Action timeoutAction=null)
    {
        float elapsedTime = 0f;
        while (!action())
        {
            yield return null;

            if (timeout > 0)
            {
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= timeout) yield break;
            }
        }
    }
    private static IEnumerator CoroutineUpdateWithDelay(Func<bool> action, float delay, float timeout, Action timeoutAction = null, Action onExit = null)
    {
        float elapsedTime = 0f;
        while (!action())
        {
            yield return new WaitForSeconds(delay);

            if (timeout > 0)
            {
                elapsedTime += delay;
                if (elapsedTime >= timeout)
                {
                    if (timeoutAction != null)
                        timeoutAction();
                    yield break; 
                }
            }
        }

        if (onExit != null)
            onExit();
    }
}
