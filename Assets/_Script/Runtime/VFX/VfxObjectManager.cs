using System.Collections.Generic;
using UnityEngine;

public class VfxObjectManager : Singleton<VfxObjectManager>
{
    public List<VfxObject> initVfxObjects = new();

    public override void Init()
    {
        base.Init();

        initVfxObjects.ForEach(e =>
        {
            ObjectPoolManager.Instance.CreatePool(e.tag, e.gameObject);
        });
    }
}
