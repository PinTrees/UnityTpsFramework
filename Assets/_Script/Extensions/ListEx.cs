using System.Collections.Generic;
using UnityEngine;

public static class ListEx
{
    public static T GetRandomElement<T>(this List<T> list) where T : class
    {
        if (list == null) return null;
        if (list.Count <= 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    public static T PopRandomElement<T>(this List<T> list) where T : class
    {
        if (list == null) return null;
        if (list.Count <= 0) return null;
        var element = list[Random.Range(0, list.Count)];
        list.Remove(element);
        return element;
    }
}
