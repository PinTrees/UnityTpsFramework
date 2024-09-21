using System.Collections;
using UnityEngine;

public class TaskSystem : Singleton<TaskSystem>
{
    public IEnumerator RunAsync(IEnumerator c)
    {
        yield return this.StartCoroutine(c);
    }

    public static IEnumerator Run(IEnumerator c)
    {
        yield return TaskSystem.Instance.RunAsync(c);
    }
}
