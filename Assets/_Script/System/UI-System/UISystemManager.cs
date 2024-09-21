using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class UISystemManager : Singleton<UISystemManager>  
{ 
    public Canvas canvas { get; private set; }
    
    public List<UIObjectBase> uiObjects = new();
    private List<UIViewBase> uiViews = new();

    // font
    [Space]
    public Font font_1;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    public override void Init()
    {
        base.Init();

        canvas = GameObject.FindAnyObjectByType<Canvas>();
        uiViews = Object.FindObjectsByType<UIViewBase>(FindObjectsSortMode.None).ToList();

        uiObjects.ForEach(e =>
        {
            ObjectPoolManager.Instance.CreatePool(e.name, e.gameObject);
        });
        //uiObjects.Clear();
    }

    #region ObjectPool
    public T Create<T>(string tag) where T : UIObjectBase
    {
        return ObjectPoolManager.Instance.GetC<T>(tag);
    }
    public void Release(GameObject uiObject)
    {
        ObjectPoolManager.Instance.Release(uiObject.name, uiObject.gameObject);
    }
    #endregion

    public T GetView<T>() where T : class
    {
        foreach (var obj in uiViews)
        {
            if (obj is T ui)
                return ui;
        }
        return null;
    }
}
