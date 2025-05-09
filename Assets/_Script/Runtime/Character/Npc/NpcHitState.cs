using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fsm.State;
using System.Linq;
using UnityEngine;

public class NpcHitStateType
{
    public const string None = "HT_None";
    public const string HitLow = "HT_Low";
    public const string HitHard = "HT_Hard";
    public const string Custom = "HT_Custom";

    public const string Air = "HT_Air";
}

public class HitStateData
{
    public HitData hitData;
}

public class NpcHitState_None : FsmState
{
    public NpcHitState_None() : base(NpcHitStateType.None) { }

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


public class NpcHitState_HitHard : FsmState
{
    new NpcCharacterActorBase owner;
    HitAnimatorSetting animationSetting;
    HitStateData stateData;
    HitData hitData;
    Vector3 hitDirection;
    public NpcHitState_HitHard() : base(NpcHitStateType.HitHard) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        // Data Setting
        owner = GetOwner<NpcCharacterActorBase>();
        owner.IsHit = true;
        stateData = layer.param as HitStateData;
        hitData = stateData.hitData;
        animationSetting = owner.scriptableAnimatorSetting.hitAnimatorSetting;

        hitDirection = owner.baseObject.transform.position - stateData.hitData.ownerCharacter.baseObject.transform.position;
        hitDirection.Normalize();

        // Animation Setting;
        {
            owner.legsAnimator.CrossFadeActive(true);

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
    }

    public override async UniTask Exit()
    {
        await base.Exit();

        owner.legsAnimator.CrossFadeActive(false);
        owner.IsHit = false;
    }

    public override void Update()
    {
        base.Update(); 

        if (owner.animator.IsPlayedOverTime(currentAnimationTag, 0.85f))
        {
            layer.ChangeStateNow(NpcHitStateType.None);
            return;
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
        layer.ChangeStateNow(NpcHitStateType.None);
    }
}

public class NpcHitState_Custom : FsmState
{
    new NpcCharacterActorBase owner;
    HitStateData hitStateData;
    HitData hitData;
    Vector3 attackerPosition;

    public NpcHitState_Custom() : base(NpcHitStateType.Custom) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        // Data Setting
        owner = GetOwner<NpcCharacterActorBase>();
        owner.IsHit = true;
        hitStateData = layer.param as HitStateData;
        hitData = hitStateData.hitData;

        // Transform Setting
        attackerPosition = hitStateData.hitData.ownerCharacter.baseObject.transform.position;
        owner.baseObject.transform.LookAt_Y(attackerPosition);

        // Animation Setting
        {
            currentAnimationTag = "Hit";
            var hitAnimationClip = hitData.customHitSetting.animationClip;
            owner.animator.applyRootMotion = hitData.customHitSetting.useRootMotion;
            owner.animator.SetAnimationClip("CustomHit", hitAnimationClip);
            owner.animator.CrossFadeInFixedTime("CustomHit", 0.15f);
            owner.animator.SetNormalizeTime("CustomHit", hitData.customHitSetting.playNormalizeTime.start);

            await owner.animator.WaitMustTransitionCompleteAsync("Hit");
        }

        // Knockback Setting 
        if (hitData.knockbackSetting.useKnockback)
        {
            var knockbackSetting = hitData.knockbackSetting;
            var ownerPosition = owner.baseObject.transform.position;
            if(knockbackSetting.knockbackType == HitboxKnockbackType.Fly)
            {
                var knockbackAmount = knockbackSetting.knockbackAmount;
                var knockbackXZOffet = (knockbackSetting.useAttackerDirection 
                    ? hitData.ownerCharacter.baseObject.transform.rotation 
                    : owner.baseObject.transform.rotation) * new Vector3(knockbackAmount.x, 0, knockbackAmount.z);

                var endPosition = ownerPosition + knockbackXZOffet;
                owner.baseObject.transform.DOJump(endPosition, knockbackAmount.y, 1, knockbackSetting.duration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                {
                    CameraManager.Instance.ShakeCamera(new CameraShakeData()
                    {
                        positionIntensity = 0.5f,
                        rotationIntensity = 0.5f,
                        zoomInAmount = 0f,
                        frequency = 5,
                    }, 0.5f).Forget();
                });
            }
        }

        // BuffDebuff Event Setting
        {

        }
    }

    public override async UniTask Exit()
    {
        await base.Exit();

        // BuffDebuff Event Setting
        {
            var stateExitBuffDebuffEvents = hitData.buffDebuffEvents.Where(e => e.buffdebuffEnterTime == BuffDebuffEnterTimeType.ExitState).ToList();
            
            foreach(var e in stateExitBuffDebuffEvents)
            {
                e.Run(owner).Forget();
            }
        }

        // Data Setting
        owner.IsHit = false;
    }

    public override void Update()
    {
        base.Update();

        if (owner.IsSturn)
            return;

        if (owner.animator.IsPlayedOverTime(currentAnimationTag, 0.99f))
        {
            HitExit();
        }

        if (hitData.customHitSetting.lookAtAttacker) 
        {
            owner.baseObject.transform.LookAt_Y(attackerPosition, 360); 
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
        HitExit();
    }

    private void HitExit()
    {
        if (hitData.hitEndMotionType == HitboxHitEndMotionType.LieDown_Up)
        {
            layer.ChangeStateNow(NpcHitStateType.None);
            return;
        }

        layer.ChangeStateNow(NpcHitStateType.None);
        return;
    }
}