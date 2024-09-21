using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

[System.Serializable]
public class ObjectPool
{
    public string uid;
    public Transform container;
    public string type;

    public GameObject origin;
    public Queue<GameObject> objects = new();


    public void Init()
    {
        origin.SetActive(false);
        container = new GameObject(uid).transform;
    }

    private void CreateObject()
    {
        GameObject spawn = GameObject.Instantiate(origin);

        spawn.name = uid;
        spawn.transform.SetParent(container);
        spawn.transform.localEulerAngles = Vector3.zero;
        spawn.transform.localPosition = Vector3.zero;

        objects.Enqueue(spawn);
    }

    public GameObject GetObject()
    {
        if (objects.Count <= 0)
        {
            CreateObject();
        }

        GameObject target = objects.Dequeue();
        target.transform.SetParent(null);

        if (!target.activeSelf)
            target.SetActive(true);

        return target;
    }

    public void Relese(GameObject target)
    {
        target.SetActive(false);
        target.transform.SetParent(container, true);

        objects.Enqueue(target);
    }
}

[System.Serializable]
public class ObjectPoolData
{
    public string tag;
    public GameObject target;
}

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    public List<ObjectPoolData> initPools = new();
    public Dictionary<string, ObjectPool> pools = new();


    public override void Init()
    {
        base.Init();

        initPools.ForEach(e =>
        {
            CreatePool(e.tag, e.target);
        });
        initPools.Clear();
    }

    public void CreatePool(string tag, GameObject target)
    {
        if(pools.ContainsKey(tag))
            return;

        ObjectPool pool = new ObjectPool()
        {
            uid = tag,
            origin = GameObject.Instantiate(target),
        };

        pool.origin.SetActive(false);

        pools[tag] = pool;
        pools[tag].Init();
    }


    public GameObject Get(string tag)
    {
        if (!pools.ContainsKey(tag))
            return null;

        return pools[tag].GetObject();
    }

    public T GetC<T>(string tag)
    {
        return Get(tag).GetComponent<T>();
    }

    public void Release(string tag, GameObject target)
    {
        if (!pools.ContainsKey(tag))
            return;

        pools[tag].Relese(target);
    }
}
