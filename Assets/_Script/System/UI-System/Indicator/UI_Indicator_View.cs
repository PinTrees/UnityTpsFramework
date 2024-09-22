using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UI_Indicator_View : UIViewBase
{
    public UpdateType updateType;
    public List<UIIndicatorBase> indicators = new();

    // System Value
    bool _updateLock = false;


    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    protected override void Update()
    {
        base.Update();

        if (updateType == UpdateType.Normal)
            UpdateIndicators();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (updateType == UpdateType.Fixed)
            UpdateIndicators();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (updateType == UpdateType.Late)
            UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        try
        {
            for (int i = 0; i < indicators.Count; ++i)
                indicators[i].UpdateTargetIndicator();
        }
        catch { }
    }

    public void AddIndicator(UIIndicatorBase indicator)
    {
        _updateLock = true;

        indicator.transform.SetParent(baseObject.transform, true);
        indicator.transform.localScale = Vector3.one;
        indicators.Add(indicator);

        _updateLock = false;
    }

    public void RemoveIndicator(UIIndicatorBase indicator)
    {
        _updateLock = true;

        indicators.Remove(indicator);

        _updateLock = false;
    }
}
