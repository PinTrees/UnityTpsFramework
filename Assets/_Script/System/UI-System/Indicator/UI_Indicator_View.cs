using System.Collections.Generic;
using UnityEngine;

public class UI_Indicator_View : UIViewBase
{
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

        if (_updateLock) return;

        for (int i = 0; i < indicators.Count; ++i)
        {
            if (_updateLock) return;

            indicators[i].UpdateTargetIndicator();
        }
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
