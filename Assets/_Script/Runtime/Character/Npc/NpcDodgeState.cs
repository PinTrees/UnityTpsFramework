using Cysharp.Threading.Tasks;
using Fsm.State;
using System.Collections;
using UnityEngine;

public class NpcDodgeStateType
{
    public const string None = "None";
    public const string ConfrontingDodge = "ConfrontingDodge";
}

public class NpcDodgeState_None : FsmState
{
    public NpcDodgeState_None() : base(NpcDodgeStateType.None) { }

    public override async UniTask Enter()
    {
        await base.Enter();
    }

    public override async UniTask Exit()
    {
        await base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}


public class NpcDodgeState_ConfrontingDodge : FsmState
{
    new NpcCharacterActorBase owner;
    DodgeAnimatorSetting dodgeAnimatorSetting;
    public NpcDodgeState_ConfrontingDodge() : base(NpcDodgeStateType.ConfrontingDodge) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<NpcCharacterActorBase>();
        owner.IsDodge = true;
        dodgeAnimatorSetting = owner.scriptableAnimatorSetting.dodgeAnimatorSetting;
        owner.navMeshAgent.ResetPath();

        // Animation Setting
        {
            owner.animator.SetFloat("dx", 0);
            owner.animator.SetFloat("dy", -1);

            currentAnimationTag = "Dodge";
            owner.animator.applyRootMotion = true;
            owner.animator.speed = 1.2f;
            owner.animator.CrossFadeInFixedTime("DodgeLong", 0.15f);
            owner.animator.SetNormalizeTime("DodgeLong", 0.01f);
            await owner.animator.TransitionCompleteAsync(currentAnimationTag);
        }
    }

    public override async UniTask Exit()
    {
        await base.Exit();
        owner.IsDodge = false;
        owner.animator.speed = 1;
        owner.animator.applyRootMotion = false;
    }

    public override void Update()
    {
        base.Update();

        if(owner.animator.IsPlayedOverTime(currentAnimationTag, 0.85f))
        {
            layer.ChangeStateNow(NpcDodgeStateType.None);
            return;
        }

        var target = owner.targetController.forcusTarget;
        if(target)
        {
            owner.baseObject.transform.LookAt_Y(target.baseObject.transform, 360);
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
        layer.ChangeStateNow(NpcDodgeStateType.None);
    }
}