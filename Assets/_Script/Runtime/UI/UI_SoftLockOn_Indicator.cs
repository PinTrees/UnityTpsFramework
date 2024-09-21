using UnityEngine;

public class UI_SoftLockOn_Indicator : UIIndicatorBase
{
    protected override void OnInit()
    {
        base.OnInit();
    }

    public void Show(CharacterActorBase target)
    {
        base.Show(target.lockOnTransform.gameObject);
    }

    public override void Close()
    {
        base.Close();
    }
}
