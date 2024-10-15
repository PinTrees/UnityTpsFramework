using Cysharp.Threading.Tasks;
using Fsm.State;
using UnityEditor;
using UnityEngine;

public class NpcBodyStateType
{
    public const string Stand = "Stand";
    public const string Crouch = "Crouch";
    public const string Crawl = "Crawl";

    public const string KnockDown_Up = "KnockDown_Up";
    public const string KnockDown_Down = "KnockDown_Down";
    public const string KnockDownToStand = "KnockDownToStand";

    // Death
    public const string Death = "Death";
}

public class NpcBodyState_Stand : FsmState
{
    public NpcBodyState_Stand() : base(NpcBodyStateType.Stand) { }
}

public class NpcBodyState_Crouch : FsmState
{
    public NpcBodyState_Crouch() : base(NpcBodyStateType.Crouch) { }
}

public class NpcBodyState_Crawl : FsmState
{
    public NpcBodyState_Crawl() : base(NpcBodyStateType.Crawl) { }
}

public class NpcBodyState_KnockDown_Up : FsmState
{
    new NpcCharacterActorBase owner;
    BodyAnimatorSetting bodyAnimationSetting;
    public NpcBodyState_KnockDown_Up() : base(NpcBodyStateType.KnockDown_Up) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<NpcCharacterActorBase>();
        owner.IsKnockDown = true;
        bodyAnimationSetting = owner.scriptableAnimatorSetting.bodyAnimatorSetting;

        // Animation Setting
        {
            currentAnimationTag = "KnockDownUp";
            owner.animator.CrossFadeInFixedTime("KnockDownUp", 0.15f);
            await owner.animator.WaitMustTransitionCompleteAsync(currentAnimationTag);
        }
    }
    public override async UniTask Exit()
    {
        await base.Exit();
        owner.IsKnockDown = false;
    }

    public override void Update()
    {
        base.Update();
    }
}

public class NpcBodyState_KnockDownToStand : FsmState
{
    new NpcCharacterActorBase owner;
    BodyAnimatorSetting bodyAnimationSetting;
    public NpcBodyState_KnockDownToStand() : base(NpcBodyStateType.KnockDownToStand) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<NpcCharacterActorBase>();
        owner.IsCanNotMove = true;
        bodyAnimationSetting = owner.scriptableAnimatorSetting.bodyAnimatorSetting;

        // Animation Setting
        {
            currentAnimationTag = "KnockDownUpToStand";
            owner.animator.speed = 1.2f;
            owner.animator.CrossFadeInFixedTime("KnockDownUpToStand", 0.15f);

            //EditorGUI.Vector3Field
        }
    }
    public override async UniTask Exit()
    {
        await base.Exit();
        owner.IsCanNotMove = false;
    }

    public override void Update()
    {
        base.Update();

        if(owner.animator.IsPlayedOverTime(currentAnimationTag, 0.85f))
        {
            layer.ChangeStateNow(NpcBodyStateType.Stand);
            return;
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
        layer.ChangeStateNow(NpcBodyStateType.Stand);
    }
}

public class NpcBodyState_Death : FsmState
{
    new NpcCharacterActorBase owner;
    public NpcBodyState_Death() : base(NpcBodyStateType.Death) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<NpcCharacterActorBase>();
        owner.IsDeath = true;
        owner.navMeshAgent.ResetPath();
        owner.navMeshAgent.enabled = false;

        // Animation Setting
        {
            owner.animator.speed = 1;
            owner.animator.CrossFadeInFixedTime("Death", 0.15f);
            owner.animator.SetNormalizeTime("Death", 0.01f);
            await owner.animator.WaitMustTransitionCompleteAsync("Death");
        }

        owner.targetController.Exit();
        owner.weaponController.DropEquipOnWeapon();
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