using Cysharp.Threading.Tasks;
using Fsm.State;
using System.Collections;
using UnityEngine;

public class PlayerMovementStateType
{
    public const string None = "MV_None";
    public const string Idle = "MV_Idle";
    public const string Walk = "MV_Walk";
    public const string Run = "MV_Run";
}

public class PlayerMovementState_None : FsmState
{
    new PlayerCharacterActorBase owner;
    public PlayerMovementState_None() : base(PlayerMovementStateType.None) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<PlayerCharacterActorBase>();
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

public class PlayerMovementState_Idle : FsmState
{
    new PlayerCharacterActorBase owner;
    public PlayerMovementState_Idle() : base(PlayerMovementStateType.Idle) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        owner = GetOwner<PlayerCharacterActorBase>();
        owner.IsIdle = true;

        owner.animator.applyRootMotion = true;
        owner.animator.CrossFadePlay("StandIdle", 0.15f);
        owner.legsAnimator.CrossFadeActive(true);
    }

    public override async UniTask Exit()
    {
        await base.Exit();
        owner.IsIdle = false;
    }

    public override void Update()
    {
        base.Update();

        if (!owner.CanMove())
            return;

        if (owner.movementDir == Vector3.zero)
            return;

        if (Input.GetKey(KeyCode.LeftShift))
            owner.OnRun();
        else 
            owner.OnWalk();

        var target = owner.targetController.forcusTarget;
        if (target) owner.baseObject.transform.LookAt_Y(target.baseObject.transform, 360f);
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
        owner.IsWalk = true;

        currentAnimationTag = "Walk";
        owner.animator.applyRootMotion = true;
        owner.animator.CrossFadeInFixedTime("StandWalk", 0.15f);

        owner.legsAnimator.CrossFadeActive(false);
    }

    public override async UniTask Exit()
    {
        await base.Exit();
        owner.IsWalk = false;
    }

    public override void Update()
    {
        base.Update();

        if (!owner.CanMove())
            return;

        if (owner.movementDir.sqrMagnitude <= float.Epsilon)
        {
            owner.OnIdle();
            return;
        }

        if (Input.GetKey(KeyCode.LeftShift))
            owner.OnRun();

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
        owner.IsRun = false;
    }

    public override void Update()
    {
        base.Update();

        if (!owner.CanMove())
            return;

        if (owner.movementDir.sqrMagnitude <= float.Epsilon)
        {
            owner.OnIdle();
            return;
        }

        if (!Input.GetKey(KeyCode.LeftShift))
            owner.OnWalk();

        owner.baseObject.transform.LookCameraY(10f);
    }
}