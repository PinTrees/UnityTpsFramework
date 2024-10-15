using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fsm.State;
using System.Collections;
using UnityEngine;
using static PlayerDodgeState_Slide;
using static UnityEngine.UI.GridLayoutGroup;

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
    HitData hitData;
    HitAnimatorSetting animationSetting;

    public PlayerHitState_HitHard() : base(PlayerHitStateType.HitHard) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        owner = GetOwner<PlayerCharacterActorBase>();
        animationSetting = owner.scriptableAnimatorSetting.hitAnimatorSetting;
        hitStateData = layer.param as HitStateData;
        hitData = hitStateData.hitData;

        hitDirection = owner.baseObject.transform.position - hitStateData.hitData.ownerCharacter.baseObject.transform.position;
        hitDirection.Normalize();

        // Animation Setting;
        {
            owner.legsAnimator.CrossFadeActive(false);

            owner.animator.SetFloat("hx", 0);
            owner.animator.SetFloat("hy", 1);

            currentAnimationTag = "Hit";
            owner.animator.speed = 1;
            owner.animator.applyRootMotion = animationSetting.standHitHard.useRootMotion;
            owner.animator.SetNormalizeTime("StandHitHard", 0.01f);
        }

        // Knockback Setting 
        if (hitData.knockbackSetting.useKnockback)
        {
            owner.animator.applyRootMotion = false;
            var knockbackSetting = hitData.knockbackSetting;
            var ownerPosition = owner.baseObject.transform.position;
            if (knockbackSetting.knockbackType == HitboxKnockbackType.Back)
            {
                var knockbackAmount = knockbackSetting.knockbackAmount;
                Vector3 knockbackXZOffet = knockbackSetting.useAttackerDirection
                   ? hitData.ownerCharacter.baseObject.transform.rotation * new Vector3(-knockbackAmount.x, 0, -knockbackAmount.z)
                   : owner.baseObject.transform.rotation * new Vector3(knockbackAmount.x, 0, knockbackAmount.z);

                var endPosition = ownerPosition + knockbackXZOffet;
                owner.baseObject.transform.DOKill();
                owner.baseObject.transform.DOMoveX(endPosition.x, knockbackSetting.duration);
                owner.baseObject.transform.DOMoveZ(endPosition.z, knockbackSetting.duration);
            }
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
        //var layerIndex = owner.animator.GetLayerIndex("Arm");
        //owner.animator.CrossFadeLayerWeight(1, 1);
    }

    public override void Update()
    {
        base.Update();

        if(Input.GetKey(KeyCode.C) && owner.movementDir != Vector3.zero)
        {
            owner.OnDodgeRoll();
            return;
        }

        if (owner.animator.IsPlayedOverTime(currentAnimationTag, 0.8f))
        {
            layer.ChangeStateNow(PlayerHitStateType.None);
            owner.OnIdle();
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
        owner.OnIdle();
    }
}