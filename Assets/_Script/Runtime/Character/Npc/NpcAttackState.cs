using UnityEngine;
using Fsm.State;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using DG.Tweening;
using System.Linq;
using TMPro;

public class NpcAttackStateType
{
    public const string None = "ATK_None";
    public const string Attack = "ATK_Attack";
}

public class NpcAttackState_None : FsmState
{
    public NpcAttackState_None() : base(NpcAttackStateType.None) { }

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

public class NpcAttackState_Attack : FsmState
{
    new NpcCharacterActorBase owner;
    bool isComboAttack = false;
    AttackNode attackNode;
    CharacterActorBase attackTarget;
    Dictionary<int, bool> vfxEventsActive = new();
    Dictionary<int, VfxObject> spawnedVfxObjects = new();

    public NpcAttackState_Attack() : base(NpcAttackStateType.Attack) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        
        owner = GetOwner<NpcCharacterActorBase>();
        owner.IsAttack = true;
        isComboAttack = false;
        attackNode = owner.combatController.currentAttackNode;
        attackTarget = owner.targetController.forcusTarget;

        // Animation Setting
        {
            currentAnimationTag = attackNode.uid;
            owner.animator.speed = attackNode.animationSpeed;
            owner.animator.applyRootMotion = attackNode.useRootMotion;
            owner.legsAnimator.CrossFadeActive(attackNode.useLegIK);
            // owner.animator.CrossFadeInFixedTime(currentAnimationTag, 0.15f);
            await owner.animator.WaitMustTransitionCompleteAsync(currentAnimationTag);
            if (attackNode.animationPlayNormailzeTime.start > 0.01f)
                owner.animator.SetNormalizeTime(currentAnimationTag, attackNode.animationPlayNormailzeTime.start);
        }

        // Hitbox Event Start
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

        attackNode.Exit(owner);
        attackNode.hitboxTree.Exit();
        attackNode.conditions.ForEach(e => e.Exit(owner));

        // 공격자 위치 수정 로직 제거
        owner.baseObject.transform.DOKill();

        // Vfx Event Exit
        foreach (var item in spawnedVfxObjects)
        {
            if (item.Value != null)
                item.Value.Stop();
        }

        if (!isComboAttack)
        {
            owner.combatController.ExitAttack();
            owner.combatController.ClearAttackCombo();
            owner.targetController.forcusTarget.targetController.activeAttackers.Remove(owner);
            owner.fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.None);
            owner.IsAttack = false;
        }
    }

    public override void OnAnimationExit()
    {
        base.OnAnimationExit();
        layer.ChangeStateNow(NpcAttackStateType.None);
    }

    public override void Update()
    {
        base.Update();

        var targetPosition = attackTarget.baseObject.transform.position;
        var ownerPosition = owner.baseObject.transform.position;

        // Attacker Transform Setting
        if (attackTarget)
        {
            // LookAt Target - RRR
            if (attackNode.attackerTransformSetting.lookAtTarget
             && owner.animator.IsPlayedInTime(currentAnimationTag, attackNode.attackerTransformSetting.canRotateableNormalizeTime))
            {
                owner.baseObject.transform.LookAt_Y(targetPosition, 360.0f);
            }

            if (attackNode.attackerTransformSetting.useAttackerTransform
             && owner.animator.IsPlayedInTime(currentAnimationTag, attackNode.attackerTransformSetting.canMoveableNormalizeTime))
            {
                var distance = Vector3.Distance(ownerPosition, targetPosition);
                if (distance < attackNode.attackerTransformSetting.distanceFromTargetOver
                 && attackNode.attackerTransformSetting.distanceFromTargetOver != 0)
                {
                    var dir = owner.baseObject.transform.position - targetPosition;

                    float distanceToTarget = Vector3.Distance(ownerPosition, targetPosition);
                    if (distanceToTarget < attackNode.attackerTransformSetting.distanceFromTarget)
                    {
                        dir *= -1f;
                    }

                    var a = distance / attackNode.attackerTransformSetting.distanceFromTargetOver;
                    var moveSpeed = Mathf.Lerp(0.1f, attackNode.attackerTransformSetting.moveSpeedPerSec, a);
                    owner.baseObject.transform.position -= dir.normalized * moveSpeed * Time.deltaTime;
                }
            }
        }

        if (owner.animator.IsPlayedOverTime(currentAnimationTag, attackNode.animationPlayNormailzeTime.exit))
        {
            layer.ChangeStateNow(NpcAttackStateType.None);
            return;
        }

        if (owner.animator.IsPlayedInTime(currentAnimationTag, attackNode.nextAttackNormalizeTime.start, attackNode.nextAttackNormalizeTime.exit))
        {
            //var nextRootAttackCombo = owner.combatController.attackPatternData.FindAttackCombo(owner);
            //AttackNode nextRootAttackNode = null;
            //if (nextRootAttackCombo != null)
            //{
            //    nextRootAttackNode = nextRootAttackCombo.attackNodes.First();
            //    if (nextRootAttackNode == attackNode)
            //        nextRootAttackNode = null;
            //}

            var nextComboAttackNode = owner.combatController.CanNextAttackCombo(attackNode);

            if (nextComboAttackNode)
            {
                isComboAttack = true;
                owner.combatController.SetNextComboAttack(nextComboAttackNode);
                layer.ChangeStateNow(NpcAttackStateType.Attack);
                return;
            }
        }

        // Rotation Transform Setting
        if (owner.targetController.forcusTarget)
        {
            var targetTransform = owner.targetController.forcusTarget.baseObject.transform;
            owner.baseObject.transform.LookAt_Y(targetTransform, 180.0f);
        }

        // Vfx Slash Effect Event
        for (int i = 0; i < attackNode.vfxEvents.Count; ++i)
        {
            var vfxEvent = attackNode.vfxEvents[i];
            if (!vfxEventsActive[i] && owner.animator.IsPlayedOverTime(currentAnimationTag, vfxEvent.vfxPlayNormalizeTime.start))
            {
                var vfxObject = VfxObject.Create(vfxEvent.vfxObject.name);
                vfxObject.transform.position = owner.baseObject.transform.position + owner.baseObject.transform.rotation * vfxEvent.offsetLocalPosition;
                vfxObject.transform.rotation = owner.baseObject.transform.rotation;
                vfxObject.transform.rotation *= Quaternion.Euler(vfxEvent.offsetLocalRotation);

                if (vfxEvent.useOverrideScale)
                    vfxObject.transform.localScale = vfxEvent.overrideScale * 0.9f;
                else vfxObject.transform.localScale = Vector3.one * 0.9f;

                if (vfxEvent.useOverrideArc)
                    vfxObject.SetFloat("Arc", (vfxEvent.overrideArcAngle / 57.3f));
                else vfxObject.SetFloat("Arc", (180 / 57.3f));

                vfxObject.SetFloat("Alpha Scale", 1f);
                vfxObject.SetFloat("Emission", 0.01f);

                vfxEventsActive[i] = true;
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
    }
}
