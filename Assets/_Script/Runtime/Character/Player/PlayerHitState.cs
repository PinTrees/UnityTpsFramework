using Cysharp.Threading.Tasks;
using Fsm.State;
using System.Collections;
using UnityEngine;

public class PlayerHitStateType
{
    public const string None = "HIT_None";
    public const string HitLow = "HIT_Low";
    public const string HitHard = "HIT_Hard";
    public const string HitCustom = "HIT_Custom";
}

public class PlayerHitState_None : FsmState
{
    public PlayerHitState_None() : base(PlayerHitStateType.None) { }

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

public class PlayerHitState_HitLow : FsmState
{
    new PlayerCharacterActorBase owner;
    Vector2 hitDirection;

    public PlayerHitState_HitLow() : base(PlayerHitStateType.HitLow) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<PlayerCharacterActorBase>();

        hitDirection = Random.insideUnitCircle.normalized;

        owner.animator.SetFloat("hx", hitDirection.x);
        owner.animator.SetFloat("hy", hitDirection.y);

        owner.animator.SetLayerWeight(1, 1);

        currentAnimationTag = "Hit";
        owner.animator.CrossFadeInFixedTime("StandHitLow", 0.15f);
        owner.animator.CrossFadeInFixedTime("StandHitLow", 0.15f, 1);
        owner.animator.SetNormalizeTime("StandHitLow", 0.01f, 1);
    }

    public override async UniTask Exit()
    {
        await base.Exit();

        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
        owner.animator.CrossFadeLayerWeight(1, 1);
    }

    public override void Update()
    {
        base.Update();

        if(owner.animator.IsPlayedOverTime(currentAnimationTag, 0.8f))
        {
            layer.ChangeStateNow(PlayerHitStateType.None);
            return;
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
        layer.ChangeStateNow(PlayerHitStateType.None);
    }
}

public class PlayerHitState_HitHard : FsmState
{
    new PlayerCharacterActorBase owner;
    Vector3 hitDirection;
    HitStateData hitStateData;
    HitAnimatorSetting animationSetting;

    public PlayerHitState_HitHard() : base(PlayerHitStateType.HitHard) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        owner = GetOwner<PlayerCharacterActorBase>();
        animationSetting = owner.scriptableAnimatorSetting.hitAnimatorSetting;
        hitStateData = layer.param as HitStateData;
        hitDirection = owner.baseObject.transform.position - hitStateData.hitData.ownerCharacter.baseObject.transform.position;
        hitDirection = hitDirection.normalized;

        // Animation Setting;
        {
            owner.legsAnimator.CrossFadeActive(true);

            owner.animator.SetFloat("hx", 0);
            owner.animator.SetFloat("hy", 1);

            currentAnimationTag = "Hit";
            owner.animator.speed = 1;
            owner.animator.applyRootMotion = animationSetting.standHitHard.useRootMotion;
            owner.legsAnimator.CrossFadeActive(true);
            await owner.animator.WaitMustTransitionCompleteAsync("StandHitHard", "Hit");
        }

        // Camera Setting
        CameraManager.Instance.ShakeCamera(new CameraShakeData()
        {
            positionIntensity = 0.75f,
            rotationIntensity = 0.5f,
            zoomInAmount = 0,
            frequency = 5,
        }, 0.35f).Forget();
    }

    public override async UniTask Exit()
    {
        await base.Exit();
        owner.IsHit = false;

        // Animator Setting
        owner.legsAnimator.CrossFadeActive(false);
        //owner.animator.CrossFadeLayerWeight(1, 1);

        // State Setting
        owner.OnIdle();
    }

    public override void Update()
    {
        base.Update();

        if (owner.animator.IsPlayedOverTime(currentAnimationTag, 0.8f))
        {
            layer.ChangeStateNow(PlayerHitStateType.None);
            return;
        }

        if (!owner.animator.applyRootMotion)
        {
            owner.baseObject.transform.position += hitDirection * 1.5f * Time.deltaTime;
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
        layer.ChangeStateNow(PlayerHitStateType.None);
    }
}