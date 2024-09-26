using UnityEngine;
using Fsm.State;
using System.Collections;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using DG.Tweening;

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
        owner.IsStartToAttack = false;
        isComboAttack = false;
        attackNode = owner.combatController.currentAttackNode;
        attackTarget = owner.targetController.forcusTarget;
        var targetPosition = attackTarget.baseObject.transform.position;
        var ownerPosition = owner.baseObject.transform.position;

        // State Setting
        owner.fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.Idle);

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
            owner.animator.CrossFadeInFixedTime(currentAnimationTag, 0.15f);
            if (attackNode.animationPlayNormailzeTime.start > 0.01f)
                owner.animator.SetNormalizeTime(currentAnimationTag, attackNode.animationPlayNormailzeTime.start);

            await owner.animator.TransitionCompleteAsync(currentAnimationTag);
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

        owner.IsAttack = false;
        owner.combatController.ExitAttack();

        // Exit Event
        if (attackNode != null)
        {
            attackNode.Exit(owner);
            attackNode.hitboxTree.Exit();
            attackNode.conditions.ForEach(e => e.Exit(owner));
        }

        // Vfx Event Exit
        foreach (var item in spawnedVfxObjects)
        {
            if (item.Value != null)
                item.Value.Stop();
        }

        if (!isComboAttack)
        {
            owner.targetController.forcusTarget.targetController.activeAttackers.Remove(owner);
            owner.fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.Idle);
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

        if(owner.animator.IsPlayedOverTime(currentAnimationTag, attackNode.animationPlayNormailzeTime.exit))
        {
            layer.ChangeStateNow(NpcAttackStateType.None);
            return;
        }

        // Rotation Transform Setting
        if(owner.targetController.forcusTarget)
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
