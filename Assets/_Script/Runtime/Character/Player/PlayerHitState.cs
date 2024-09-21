using Cysharp.Threading.Tasks;
using Fsm.State;
using System.Collections;
using UnityEngine;

public class PlayerHitStateType
{
    public const string None = "None";
    public const string HitLow = "HitLow";
    public const string HitHard = "HitHard";
    public const string HitCustom = "HitCustom";
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

        //if(Input.GetKey(KeyCode.H))
        //{
        //    layer.ChangeStateNow(PlayerHitStateType.HitLow);
        //}
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

        // State Setting
        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.AttackLayer, PlayerAttackStateType.None);
        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.DodgeLayer, PlayerDodgeStateType.None);
      
        // Animation Setting;
        {
            owner.legsAnimator.CrossFadeActive(true);

            owner.animator.SetFloat("hx", 0);
            owner.animator.SetFloat("hy", 1);

            currentAnimationTag = "Hit";
            owner.animator.speed = 1;
            owner.animator.applyRootMotion = animationSetting.standHitHard.useRootMotion;
            owner.animator.CrossFadeInFixedTime("StandHitHard", 0.15f);
            owner.animator.SetNormalizeTime("StandHitHard", 0.01f);

            await owner.animator.TransitionCompleteAsync(currentAnimationTag);
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

        // Animator Setting
        owner.legsAnimator.CrossFadeActive(false);

        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
        //owner.animator.CrossFadeLayerWeight(1, 1);

        // Status Setting
        owner.IsHit = false;
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