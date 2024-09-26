using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fsm.State;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAttackStateType
{
    public const string None = "AK_None";
    public const string Attack = "AK_Attack";
}

public class PlayerAttackState_None  : FsmState
{
    new PlayerCharacterActorBase owner;
    public PlayerAttackState_None() : base(PlayerAttackStateType.None) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        // Data Set
        owner = GetOwner<PlayerCharacterActorBase>();
        owner.IsAttack = false;
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

public class PlayerAttackState_Attack : FsmState
{
    new PlayerCharacterActorBase owner;
    AttackNode attackNode;
    bool isComboAttack = false;
    Vector3 targetLookAtPosition = Vector3.zero;
    Dictionary<int, bool> vfxEventsActive = new();
    Dictionary<int, VfxObject> spawnedVfxObjects = new();
    int currentCameraShakeIndex = 0;
    CharacterActorBase attackTarget;

    public PlayerAttackState_Attack() : base(PlayerAttackStateType.Attack) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        // Data Set
        owner = GetOwner<PlayerCharacterActorBase>();
        owner.IsAttack = true;
        isComboAttack = false;
        currentCameraShakeIndex = 0;

        attackTarget = owner.targetController.forcusTarget;
        attackNode = owner.attackController.currentAttackNode;
        attackNode.Init();

        Debug.Log("AAAAAAAAAAAAAAAAA");

        var targetPosition = attackTarget != null ? attackTarget.baseObject.transform.position : Vector3.zero;
        var ownerPosition = owner.baseObject.transform.position;

        // Target Position Setting
        targetLookAtPosition = targetPosition;
        if (targetLookAtPosition != Vector3.zero)
            targetLookAtPosition += (targetPosition - ownerPosition) * 10f;

        // Attacker Transform Setting
        if (attackTarget)
        {
            // LookAt Target - RRR
            owner.baseObject.transform.LookAt_Y(targetPosition);

            if (attackNode.attackerTransformSetting.useAttackerTransform && attackTarget)
            {
                if ((Vector3.Distance(ownerPosition, targetPosition) < attackNode.attackerTransformSetting.distanceFromTargetOver)
                 || attackNode.attackerTransformSetting.distanceFromTargetOver == 0)
                {
                    var dir = owner.baseObject.transform.position - targetPosition;
                    var attackerPosition = targetPosition + dir.normalized * attackNode.attackerTransformSetting.distanceFromTarget;
                    owner.baseObject.transform.DOMoveX(attackerPosition.x, attackNode.attackerTransformSetting.transitionDuration);
                    owner.baseObject.transform.DOMoveZ(attackerPosition.z, attackNode.attackerTransformSetting.transitionDuration);
                }
            }
        }

        // Animation Setting
        {
            currentAnimationTag = attackNode.uid;
            owner.animator.speed = attackNode.animationSpeed;
            owner.animator.applyRootMotion = attackNode.useRootMotion;
            owner.animator.CrossFadeInFixedTime(attackNode.uid, 0.15f);
            if(attackNode.animationPlayNormailzeTime.start > 0.01f)
                owner.animator.SetNormalizeTime(attackNode.uid, attackNode.animationPlayNormailzeTime.start);

            owner.legsAnimator.CrossFadeActive(attackNode.useLegIK);
            owner.lookAnimator.CrossFadeActive(false);
            await owner.animator.WaitMustTransitionComplete(currentAnimationTag);
        }

        // Event Start
        attackNode.Enter(owner);
        attackNode.hitboxTree.Enter(owner, attackNode.uid);

        // Vfx Event Setting
        vfxEventsActive.Clear();
        spawnedVfxObjects.Clear();

        for (int i = 0; i < attackNode.vfxEvents.Count; ++i)
        {
            vfxEventsActive[i] = false;
            spawnedVfxObjects[i] = null;
        }
    }

    public override async UniTask Exit()
    {
        await base.Exit();

        owner.IsSuperArmor = false;

        // Event Exit
        attackNode.Exit(owner);
        attackNode.hitboxTree.Exit();
        attackNode.conditions.ForEach(e => e.Exit(owner));

        foreach (var item in spawnedVfxObjects)
        {
            if (item.Value != null)
                item.Value.Stop();
        }

        if (!isComboAttack)
        {
            owner.attackController.ClearAttackCombo();
            owner.fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
            owner.IsAttack = false;
        }
    }

    public override void Update()
    {
        Debug.Log("AAAAAAAAAAAAAAAAA");

        if (owner.animator.IsPlayedOverTime(currentAnimationTag, attackNode.animationPlayNormailzeTime.exit))
        {
            layer.ChangeStateNow(PlayerAttackStateType.None);
            return;
        }

        if(attackNode.superArmorSetting.useSuperAmor)
        {
            var superArmorSetting = attackNode.superArmorSetting;
            owner.IsSuperArmor = owner.animator.IsPlayedInTime(currentAnimationTag, superArmorSetting.superAmorNormalizeTime.start, superArmorSetting.superAmorNormalizeTime.exit);
        }

        if (owner.animator.IsPlayedInTime(currentAnimationTag, attackNode.nextAttackNormalizeTime.start, attackNode.nextAttackNormalizeTime.exit))
        {
            var nextRootAttackCombo = owner.attackController.attackPatternData.FindAttackCombo(owner);
            AttackNode nextRootAttackNode = null;
            if (nextRootAttackCombo != null)
            {
                nextRootAttackNode = nextRootAttackCombo.attackNodes.First();
                if (nextRootAttackNode == attackNode)
                    nextRootAttackNode = null;  
            }

            var nextComboAttackNode = owner.attackController.CanNextAttackCombo(attackNode);

            if (nextRootAttackNode)
            {
                if(nextComboAttackNode == null)
                {
                    owner.attackController.SetAttackCombo(nextRootAttackCombo);
                    nextComboAttackNode = nextRootAttackNode;
                }
                else if (nextRootAttackNode.priorityRank > nextComboAttackNode.priorityRank)
                {
                    owner.attackController.SetAttackCombo(nextRootAttackCombo);
                    nextComboAttackNode = nextRootAttackNode;
                }
            }
            if (nextComboAttackNode)
            {
                isComboAttack = true;
                owner.attackController.SetNextComboAttack(nextComboAttackNode);
                layer.ChangeStateNow(PlayerAttackStateType.Attack);
                return;
            }
            else if(nextRootAttackNode)
            {
                isComboAttack = true;
                owner.attackController.SetNextComboAttack(nextRootAttackNode);
                layer.ChangeStateNow(PlayerAttackStateType.Attack);
                return;
            }
        }

        // Rotation Transform Setting
        // LookAt Target  
        if (owner.lockOnController.IsSoftLockOn)
        {
            if (owner.lockOnController.lockOnTarget)
            {
                var targetPosition = owner.lockOnController.lockOnTarget.baseObject.transform.position;
                owner.baseObject.transform.LookAt_Y(targetPosition, 360f);
            }
        }
        else if (attackNode.attackerTransformSetting.useAttackerTransform
              && attackNode.attackerTransformSetting.lookAtTarget)
        {
            var target = owner.targetController.forcusTarget;
            if(target != null)
                owner.baseObject.transform.LookAt_Y(target.baseObject.transform.position, 360f);
        }
        // LookAt Camera 
        else
        {
          
            owner.baseObject.transform.LookCameraY(360f);
        }

        // Vfx Slash Effect Event
        for(int i = 0; i < attackNode.vfxEvents.Count; ++i)
        {
            var vfxEvent = attackNode.vfxEvents[i];
            if (!vfxEventsActive[i] && owner.animator.IsPlayedOverTime(currentAnimationTag, vfxEvent.vfxPlayNormalizeTime.start))
            {
                vfxEventsActive[i] = true;
                var vfxObject = VfxObject.Create(vfxEvent.vfxObject.name);
                vfxObject.transform.position = owner.baseObject.transform.position + owner.baseObject.transform.rotation * vfxEvent.offsetLocalPosition;
                vfxObject.transform.rotation = owner.baseObject.transform.rotation;
                vfxObject.transform.rotation *= Quaternion.Euler(vfxEvent.offsetLocalRotation);

                if (vfxEvent.useOverrideScale)
                    vfxObject.transform.localScale = vfxEvent.overrideScale;
                else vfxObject.transform.localScale = Vector3.one;

                if (vfxEvent.useOverrideArc)
                    vfxObject.SetFloat("Arc", (vfxEvent.overrideArcAngle / 57.3f));
                else vfxObject.SetFloat("Arc", (180 / 57.3f));

                if(vfxEvent.useOverrideAlpha)
                    vfxObject.SetFloat("Alpha Scale", vfxEvent.overrideAlpha);
                else vfxObject.SetFloat("Alpha Scale", 1);

                spawnedVfxObjects[i] = vfxObject;
            }

            if (vfxEventsActive[i]  
                && spawnedVfxObjects[i] != null
                && owner.animator.IsPlayedOverTime(currentAnimationTag, vfxEvent.vfxPlayNormalizeTime.exit))
            {
                spawnedVfxObjects[i].Stop();
                spawnedVfxObjects[i] = null;
            }
        }

        // Camera Event
        if (attackNode.cameraEvents.Count <= currentCameraShakeIndex)
            return;

        var currentCameraShake = attackNode.cameraEvents[currentCameraShakeIndex];
        if(owner.animator.IsPlayedOverTime(currentAnimationTag, currentCameraShake.eventNormalizeTime))
        {
            currentCameraShakeIndex++;
            CameraManager.Instance.ShakeCamera(currentCameraShake.shakeData, currentCameraShake.duration).Forget();
        }
    
        //stateData.attackNode.UpdateEvent(owner);
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
        layer.ChangeStateNow(PlayerAttackStateType.None);
    }
}

