using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectEx
{
    public static void RemoveComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject.TryGetComponent<T>(out T component))
        {
            UnityEngine.Object.Destroy(component);
            //Debug.Log("Removed Component of type " + typeof(T));
        }
        //Debug.Log("Not found Component of type " + typeof(T));
    }

    public static T[] GetComponentsImmediateChild<T>(this GameObject parent) where T : Component
    {
        List<T> components = new List<T>();

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            Transform child = parent.transform.GetChild(i);
            T component = child.GetComponent<T>();
            if (component != null)
            {
                components.Add(component);
            }
        }

        return components.ToArray();
    }

    public static T GetComponentImmediateFirstChild<T>(this GameObject parent) where T : Component
    {
        if (parent.transform.childCount <= 0)
            return null;

        return parent.transform.GetChild(0).GetComponent<T>();
    }

    public static GameObject CreateChild(this GameObject target, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(target.transform, true);
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localScale = Vector3.one;
        return go;
    }

    public static T CreateChildWithComponent<T>(this GameObject target, string name="") where T : Component 
    {
        var go = target.CreateChild(name);
        return go.AddComponent<T>();
    }
}
