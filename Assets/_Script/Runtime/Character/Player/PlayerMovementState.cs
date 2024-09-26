using Cysharp.Threading.Tasks;
using Fsm.State;
using System.Collections;
using UnityEngine;

public class PlayerMovementStateType
{
    public const string Idle = "MV_Idle";
    public const string Walk = "MV_Walk";
    public const string Run = "MV_Run";
}

public class PlayerMovementState_Idle : FsmState
{
    new PlayerCharacterActorBase owner;

    public PlayerMovementState_Idle() : base(PlayerMovementStateType.Idle) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        owner = GetOwner<PlayerCharacterActorBase>();

        if (owner.IsAttack) return;
        if (owner.IsHit) return;
        if (owner.IsDeath) return;

        currentAnimationTag = "Idle";
        owner.animator.applyRootMotion = true;
        owner.animator.CrossFadeInFixedTime("StandIdle", 0.15f);

        owner.legsAnimator.CrossFadeActive(true);
    }

    public override async UniTask Exit()
    {
        await base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (owner.IsJustDodge)
            return;
        if (owner.IsHit)
            return;
        if (owner.IsDodge)
            return;
        if (owner.IsAttack)
            return;

        if (owner.movementDir != Vector3.zero)
        {
            layer.ChangeStateNow(PlayerMovementStateType.Walk);
            return;
        }

        var target = owner.targetController.forcusTarget;
        if (target)
        {
            owner.baseObject.transform.LookAt_Y(target.baseObject.transform, 360f);
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
    }
}


public class PlayerMovementState_Walk : FsmState
{
    new PlayerCharacterActorBase owner;

    public PlayerMovementState_Walk() : base(PlayerMovementStateType.Walk) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        owner = GetOwner<PlayerCharacterActorBase>();

        currentAnimationTag = "Walk";
        owner.animator.applyRootMotion = true;
        owner.animator.CrossFadeInFixedTime("StandWalk", 0.15f);

        owner.legsAnimator.CrossFadeActive(false);
    }

    public override async UniTask Exit()
    {
        await base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (owner.movementDir == Vector3.zero)
        {
            layer.ChangeStateNow(PlayerMovementStateType.Idle);
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            layer.ChangeStateNow(PlayerMovementStateType.Run);
            return;
        }

        //var targetMoveDir = owner.baseObject.transform.rotation * owner.movementDir;
        //owner.baseObject.transform.position += targetMoveDir.normalized * 1.8f * Time.deltaTime;

        owner.baseObject.transform.LookCameraY(10f);
    }
}


public class PlayerMovementState_Run : FsmState
{
    new PlayerCharacterActorBase owner;

    public PlayerMovementState_Run() : base(PlayerMovementStateType.Run) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        
        owner = GetOwner<PlayerCharacterActorBase>();

        currentAnimationTag = "Run";
        owner.animator.applyRootMotion = true;
        owner.animator.CrossFadeInFixedTime("StandRun", 0.15f);

        owner.legsAnimator.CrossFadeActive(false);
    }

    public override async UniTask Exit()
    {
        await base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (owner.movementDir == Vector3.zero)
        {
            layer.ChangeStateNow(PlayerMovementStateType.Idle);
            return;
        }

        if (!Input.GetKey(KeyCode.LeftShift))
        {
            layer.ChangeStateNow(PlayerMovementStateType.Walk);
            return;
        }

        //var targetMoveDir = owner.baseObject.transform.rotation * owner.movementDir;
        //owner.baseObject.transform.position += targetMoveDir.normalized * 3.0f * Time.deltaTime;

        owner.baseObject.transform.LookCameraY(10f);
    }
}