using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fsm.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerDodgeStateType
{
    public const string None = "None";
    public const string Roll = "Roll";
    public const string Short = "Short";
    public const string Long = "Long";
    public const string Slide = "Slide";
    public const string JustDodge = "JustDodge";
}

public class PlayerDodgeState_None : FsmState
{
    new PlayerCharacterActorBase owner;
    public PlayerDodgeState_None() : base(PlayerDodgeStateType.None) { }

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

        if (owner.IsJustDodge) return;
        if (owner.IsHit) return;
        if (owner.IsAttack) return;

        if (owner.movementDir != Vector3.zero)
        {
            if (Input.GetKey(KeyCode.C))
            {
                owner.IsDodge = true;
                layer.ChangeStateNow(PlayerDodgeStateType.Roll);
                return;
            }

            if (Input.GetKey(KeyCode.Mouse1))
            {
                layer.ChangeStateNow(PlayerDodgeStateType.Slide, new PlayerDodgeState_Slide.StateData()
                {
                    moveDirection = owner.movementDir.normalized,// .NormalizeToBoundary(),
                });
            }
        }
    }
}

public class PlayerDodgeState_JustDodge : FsmState
{
    new PlayerCharacterActorBase owner;
    DodgeAnimatorSetting animationSetting;
    Vector3 moveDirection;
    public PlayerDodgeState_JustDodge() : base(PlayerDodgeStateType.JustDodge) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        // Data Setting
        owner = GetOwner<PlayerCharacterActorBase>();
        animationSetting = owner.scriptableAnimatorSetting.dodgeAnimatorSetting;

        if(owner.attackController.justDodgeAttackNode.attackDirectionType == AttackDirectionType.Down)
        {
            moveDirection = Random.Range(0, 2) == 0 ? Vector3.right : Vector3.left;
            var targetPosiiton = owner.attackController.justDodgeAttacker.baseObject.transform.position;
            var targetDistance = Vector3.Distance(owner.baseObject.transform.position, targetPosiiton);
            if (targetDistance < 1.5f)
                moveDirection += Vector3.back;
        }

        // Animation Setting
        {
            currentAnimationTag = "Dodge";
            owner.animator.speed = 1;
            owner.legsAnimator.CrossFadeActive(false);

            owner.animator.SetFloat("dx", moveDirection.x);
            owner.animator.SetFloat("dy", moveDirection.z);

            owner.animator.applyRootMotion = animationSetting.dodgeLong.useRootMotion;
            owner.animator.CrossFadeInFixedTime("DodgeLong", 0.15f);
            owner.animator.SetNormalizeTime("DodgeLong", 0.01f);
            await owner.animator.TransitionCompleteAsync(currentAnimationTag);
        }

        // Transform Setting
        var targetPosition = owner.attackController.justDodgeAttacker.baseObject.transform.position;
        owner.baseObject.transform.LookAt_Y(targetPosition);

        var vfxObject = VfxObject.Create("VfxSpark3");
        vfxObject.transform.position = owner.GetCenterOfMass();
        vfxObject.transform.rotation = Random.rotation;
        vfxObject.transform.localScale = Vector3.one;
        vfxObject.transform.DOScale(Vector3.one * 1.25f, 0.25f).OnComplete(() =>
        {
            vfxObject.Stop();
        });
    }

    public override async UniTask Exit()
    {
        await base.Exit();
        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);

        owner.attackController.ClearJustDodgeData();
        owner.IsJustDodge = false;
    }

    public override void Update()
    {
        base.Update();

        if(owner.animator.IsPlayedOverTime(currentAnimationTag, 0.85f))
        {
            layer.ChangeStateNow(PlayerDodgeStateType.None);
            return;
        }

        //owner.baseObject.transform.position += owner.baseObject.transform.rotation * moveDirection * 2.5f * Time.deltaTime;
        var targetTransform = owner.attackController.justDodgeAttacker.baseObject.transform;
        //owner.baseObject.transform.LookAt_Y(targetTransform, 360f);
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
        layer.ChangeStateNow(PlayerDodgeStateType.None);
    }
}

public class PlayerDodgeState_Roll : FsmState
{
    new PlayerCharacterActorBase owner;
    DodgeAnimatorSetting animatorSetting;
    Vector3 moveDirection;
    bool IsDiagonalDirection;

    public PlayerDodgeState_Roll() : base(PlayerDodgeStateType.Roll) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        // Data Parsing        
        owner = GetOwner<PlayerCharacterActorBase>();
        animatorSetting = owner.scriptableAnimatorSetting.dodgeAnimatorSetting;
        moveDirection = owner.movementDir.NormalizeToBoundary();
        IsDiagonalDirection = Mathf.Abs(moveDirection.x) > 0.1f && Mathf.Abs(moveDirection.z) > 0.1f;

        // State Setting
        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);

        // Animation Setting
        {
            owner.animator.speed = 1.2f;
            owner.animator.SetFloat("dx", moveDirection.x);
            owner.animator.SetFloat("dy", moveDirection.z);

            currentAnimationTag = "Dodge";
            owner.animator.applyRootMotion = animatorSetting.dodgeRoll.useRootMotion;
            if (IsDiagonalDirection) owner.animator.applyRootMotion = false;
            owner.animator.CrossFadeInFixedTime("DodgeRoll", 0.15f);
            owner.animator.SetNormalizeTime("DodgeRoll", 0.01f);

            if (animatorSetting.overrideAnimatorSetting != null)
            {
                owner.animator.SetLayerWeight(1, 1);
                owner.animator.CrossFadeInFixedTime("DodgeRoll", 0.15f, 1);
                owner.animator.SetNormalizeTime("DodgeRoll", 0.01f, 1);
            }

            owner.legsAnimator.CrossFadeActive(false);
            owner.lookAnimator.CrossFadeActive(false);
        }

        owner.baseObject.transform.LookCameraY();

        CameraManager.Instance.ChangeCamera("PlayerDodgeCamera", 0.35f);

        float upwardForce = owner.rb.mass * 2f;  
        owner.rb.AddForce(Vector3.up * upwardForce, ForceMode.Impulse);
    }

    public override async UniTask Exit()
    {
        await base.Exit();

        // Data Set
        TimeEx.CrossFadeTimeScale(1f, 0.15f).Forget();

        // Animator Setting
        {
            owner.animator.speed = 1;
            owner.animator.applyRootMotion = false;
            owner.animator.CrossFadeLayerWeight(1, 0);
        }

        // State Chain Setting
        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);

        // Camera Setting
        CameraManager.Instance.ChangeCamera("PlayerMainCamera", 1.5f);

        // State Exit
        owner.IsDodge = false;
    }

    public override void Update()
    {
        base.Update();

        if (owner.animator.IsPlayedOverTime(currentAnimationTag, 0.75f))
        {
            layer.ChangeStateNow(PlayerDodgeStateType.None);
            return;
        }

        if (owner.animator.IsPlayedInTime(currentAnimationTag, 0.0f, 0.05f))
            TimeEx.CrossFadeTimeScale(0.3f, 0.15f).Forget();
        else if (owner.animator.IsPlayedInTime(currentAnimationTag, 0.05f, 0.12f))
            TimeEx.CrossFadeTimeScale(0.9f, 0.15f).Forget();
        else if (owner.animator.IsPlayedInTime(currentAnimationTag, 0.12f, 0.3f))
            TimeEx.CrossFadeTimeScale(0.35f, 0.15f).Forget();
        else TimeEx.CrossFadeTimeScale(1f, 0.15f).Forget();

        // 이동 처리
        if (!animatorSetting.dodgeRoll.useRootMotion)
        {
            var dir = owner.baseObject.transform.rotation * moveDirection;
            owner.baseObject.transform.position += dir.normalized * 4.0f * Time.unscaledDeltaTime;
        }
        else if (IsDiagonalDirection)
        {
            var dir = owner.baseObject.transform.rotation * moveDirection;
            owner.baseObject.transform.position += dir.normalized * 4.0f * Time.unscaledDeltaTime;
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit(); 
        layer.ChangeStateNow(PlayerDodgeStateType.None);
    }
}

public class PlayerDodgeState_Slide : FsmState
{
    public class StateData
    {
        // set - normalize requied
        public Vector3 moveDirection;
    }

    new PlayerCharacterActorBase owner;
    DodgeAnimatorSetting animatorSetting;
    public StateData stateData;

    public PlayerDodgeState_Slide() : base(PlayerDodgeStateType.Slide) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        // Data Parsing        
        owner = GetOwner<PlayerCharacterActorBase>();
        owner.IsDodge = true;
        animatorSetting = owner.scriptableAnimatorSetting.dodgeAnimatorSetting;
        stateData = layer.param as StateData;

        // State Setting
        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);

        // Animation Setting
        {
            owner.animator.SetFloat("dx", stateData.moveDirection.x);
            owner.animator.SetFloat("dy", stateData.moveDirection.z);

            currentAnimationTag = "Dodge";
            owner.animator.applyRootMotion = animatorSetting.dodgeSlide.useRootMotion;
            owner.animator.CrossFadeInFixedTime("DodgeSlide", 0.15f);
            owner.animator.SetNormalizeTime("DodgeSlide", 0.01f);

            owner.legsAnimator.CrossFadeActive(false);
            owner.lookAnimator.CrossFadeActive(false);

            if(animatorSetting.useOverrideDodegSlideIdleAnimation)
            {
                var overrideLayer = owner.animator.GetLayerIndex(animatorSetting.overrideDodgeSlideIdleAvatarType.ToString());
                owner.animator.CrossFadeLayerWeight(overrideLayer, 1, 0.15f);
                owner.animator.CrossFadeInFixedTime("DodgeSlide", 0.15f, overrideLayer);
                owner.animator.SetNormalizeTime("DodgeSlide", 0.01f, overrideLayer);
            }
        }
    }

    public override async UniTask Exit()
    {
        await base.Exit();

        // Animator Setting
        {
            owner.animator.applyRootMotion = false;

            if (animatorSetting.useOverrideDodegSlideIdleAnimation)
            {
                var overrideLayer = owner.animator.GetLayerIndex(animatorSetting.overrideDodgeSlideIdleAvatarType.ToString());
                owner.animator.CrossFadeLayerWeight(overrideLayer, 0);
            }
        }

        // Data Set
        owner.IsDodge = false;
    }

    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // 이동 처리
        if (!animatorSetting.dodgeSlide.useRootMotion
         && owner.animator.IsPlayedInTime(currentAnimationTag, 0, 0.75f))
        {
            var dir = owner.baseObject.transform.rotation * stateData.moveDirection;
            owner.baseObject.transform.position += dir.normalized * 6.5f * Time.fixedDeltaTime;
        }

        if (owner.animator.IsPlayedOverTime(currentAnimationTag, 0.75f))
        {
            layer.ChangeStateNow(PlayerDodgeStateType.None);
            layer.context.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();

        layer.ChangeStateNow(PlayerDodgeStateType.None);
        layer.context.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
    }
}

public class PlayerDodgeState_Long : FsmState
{
    new PlayerCharacterActorBase owner;
    Vector3 moveDirection;
    public PlayerDodgeState_Long() : base(PlayerDodgeStateType.Long) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        owner = GetOwner<PlayerCharacterActorBase>();
        owner.IsDodge = true;

        moveDirection = owner.movementDir;

        owner.animator.SetFloat("dx", moveDirection.x);
        owner.animator.SetFloat("dy", moveDirection.z);

        // 이동 불가 강제 세팅
        await owner.fsmContext.ChangeStateNowAsync(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);

        currentAnimationTag = "Dodge";
        owner.animator.applyRootMotion = false;
        owner.animator.CrossFadeInFixedTime("DodgeRoll", 0.15f);
        owner.animator.SetNormalizeTime("DodgeRoll", 0.01f);

        owner.legsAnimator.CrossFadeActive(false);
        owner.lookAnimator.CrossFadeActive(false);
    }

    public override async UniTask Exit() 
    {
        await base.Exit();
        
        owner.animator.applyRootMotion = false;
        owner.IsDodge = false;
    }

    public override void Update()
    {
        base.Update();

        owner.baseObject.transform.position += moveDirection * 3f * Time.deltaTime;
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();

        layer.ChangeStateNowAsync(PlayerDodgeStateType.None);
        layer.context.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
    }
}
