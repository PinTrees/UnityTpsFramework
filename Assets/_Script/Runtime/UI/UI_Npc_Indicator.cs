using UnityEngine;
using UnityEngine.UI;

public class UI_Npc_Indicator : UIIndicatorBase
{
    [Header("Data Setting")]
    public NpcCharacterActorBase ownerCharacter;

    [Header("UI Components Setting")]
    public Image hpBarImage;
    public Image hpDampedBarImage;


    public void Show(NpcCharacterActorBase owner)
    {
        base.Show(owner.indicatorTransform.gameObject);
        ownerCharacter = owner; 
    }

    public override void Close()
    {
        base.Close();
        ownerCharacter = null;
        UISystemManager.Instance.Release(this.gameObject);
    }

    protected override void OnInit()
    {
        base.OnInit();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (ownerCharacter == null)
            return;

        hpBarImage.fillAmount = ownerCharacter.healthController.GetHealthAmount();
        hpDampedBarImage.fillAmount = ownerCharacter.healthController.GetHealthAmountDamp();
    }
}
