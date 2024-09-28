using Cysharp.Threading.Tasks;
using Fsm;
using Fsm.State;
using UnityEngine;

public class NpcMovementStateType
{
    public const string None = "MV_None";
    public const string Idle = "MV_Idle";
    public const string Walk = "MV_Walk";
    public const string Run = "MV_Run";

    // Combat State
    public const string Trace = "MV_Trace";                            
    public const string Confronting = "MV_Confronting";                // 대치상태 - 타겟 따라서 이동
    public const string ConfrontingTrace = "MV_ConfrontingTrace";      // 대치 달리기 상태 - 타겟주위까지 전속력으로 이동
    public const string TacticalMove = "MV_TacticalMove";              // 전술적 위치 조정
    public const string RunToAttack = "MV_ReadyToAttackTrace";
}

public class NpcMovementState_None : FsmState
{
    new NpcCharacterActorBase owner;
    public NpcMovementState_None() : base(NpcMovementStateType.None) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<NpcCharacterActorBase>();
    }

    public override async UniTask Exit()
    {
        await base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (!owner.CanMove())
            return;

        var target = owner.targetController.forcusTarget;
        if (target == null)
            return;

        var targetPosition = target.baseObject.transform.position;
        var ownerPosition = owner.baseObject.transform.position;
        var targetDistance = Vector3.Distance(ownerPosition, targetPosition);

        if (targetDistance < owner.characterData.combatData.traceStartRange)
        {
            owner.IsTrace = true;
            layer.ChangeStateNow(NpcMovementStateType.Trace);
            return;
        }
    }
}

public class NpcMovementState_Idle : FsmState
{
    new NpcCharacterActorBase owner;
    CharacterCombatData combatData;
    public NpcMovementState_Idle() : base(NpcMovementStateType.Idle) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<NpcCharacterActorBase>();
        combatData = owner.characterData.combatData;

        await owner.animator.WaitMustTransitionCompleteAsync("StandIdle", "Idle"); 
    }

    public override async UniTask Exit()
    {
        await base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (!owner.CanMove())
            return;

        var target = owner.targetController.forcusTarget;
        if (target == null)
            return;

        var targetPosition = target.baseObject.transform.position;
        var ownerPosition = owner.baseObject.transform.position;
        var targetDistance = Vector3.Distance(ownerPosition, targetPosition);

        if (targetDistance < combatData.traceStartRange)
        {
            owner.IsTrace = true;
            layer.ChangeStateNow(NpcMovementStateType.Trace);
            return;
        }
    }
}


public class NpcMovementState_Walk : FsmState
{
    public NpcMovementState_Walk() : base(NpcMovementStateType.Walk) { }

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


// 추적중
public class NpcMovementState_Trace : FsmState
{
    new NpcCharacterActorBase owner;
    CharacterCombatData npcCombatData;
    public NpcMovementState_Trace() : base(NpcMovementStateType.Trace) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<NpcCharacterActorBase>();
        owner.IsTrace = true;
        npcCombatData = owner.characterData.combatData;

        owner.navMeshAgent.transform.rotation = owner.baseObject.transform.rotation;

        owner.animator.applyRootMotion = true;
        owner.animator.CrossFadeInFixedTime("StandRun", 0.15f);
    }

    public override async UniTask Exit()
    {
        await base.Exit();
        owner.navMeshAgent.ResetPath();
        owner.IsTrace = false;
    }

    public override void Update()
    {
        base.Update();

        var target = owner.targetController.forcusTarget;
        if(target == null)
        {
            layer.ChangeStateNow(NpcMovementStateType.Idle);
            return;
        }

        // Animation Setting
        {
            Vector3 currentDirection = owner.navMeshAgent.desiredVelocity.normalized;
            Vector3 localDirection = owner.baseObject.transform.InverseTransformDirection(currentDirection);
            Vector3 currentV = new Vector3(owner.animator.GetFloat("x"), 0, owner.animator.GetFloat("y"));

            localDirection = Vector3.Lerp(currentV, localDirection, 5f * Time.deltaTime);

            owner.animator.SetFloat("x", localDirection.x);
            owner.animator.SetFloat("y", localDirection.z);
        }
        owner.baseObject.transform.rotation = Quaternion.Lerp(owner.baseObject.transform.rotation, owner.navMeshAgent.transform.rotation, 5 * Time.deltaTime);

        var targetPosition = target.baseObject.transform.position;
        owner.navMeshAgent.SetDestination(targetPosition);

        var targetDistance = Vector3.Distance(owner.baseObject.transform.position, targetPosition);
        if(npcCombatData.confrontingRange > targetDistance)
        {
            owner.OnConfrontingTrace();
        }
    }
}

// 대치중
public class NpcMovementState_Confronting : FsmState
{
    new NpcCharacterActorBase owner;
    CharacterCombatData npcCombatData;
    float repositionDuration;
    float confrontingDistance;
    public NpcMovementState_Confronting() : base(NpcMovementStateType.Confronting) { }

    public override async UniTask Enter()
    {
        await base.Enter();

        owner = GetOwner<NpcCharacterActorBase>();
        npcCombatData = owner.characterData.combatData;
        owner.navMeshAgent.ResetPath();
        owner.IsConfronting = true;
        repositionDuration = Random.Range(0, npcCombatData.repositionDurationRandomOffset);
        IsBack = false;
        IsFont = false;

        confrontingDistance = Vector3.Distance(
            owner.targetController.forcusTarget.baseObject.transform.position,
            owner.baseObject.transform.position);

        owner.animator.applyRootMotion = true;
        owner.legsAnimator.CrossFadeActive(true);
        owner.animator.CrossFadeInFixedTime("StandWalk", 0.15f);
        owner.animator.SetFloat("x", 0);
        owner.animator.SetFloat("y", 0);
    }

    public override async UniTask Exit()
    {
        await base.Exit();
        owner.IsConfronting = false;
    }

    bool IsBack = false;
    bool IsFont = false;
    public override void Update()
    {
        base.Update();

        if (owner.IsHit)
            return;
        if (owner.IsAttack)
            return;
        if (owner.IsDodge)
            return;

        var target = owner.targetController.forcusTarget;
        if (target == null)
            return;

        var targetDistance = Vector3.Distance(target.baseObject.transform.position, owner.baseObject.transform.position);
        owner.baseObject.transform.LookAt_Y(target.baseObject.transform, 360);

        // Dodge
        if(targetDistance < npcCombatData.avoidDistance)
        {
            owner.IsDodge = true;
            owner.fsmContext.ChangeStateNow(NpcFsmLayer.DodgeLayer, NpcDodgeStateType.ConfrontingDodge);
            return;
        }

        if (targetDistance > npcCombatData.confrontingRange * 1.5f)
        {
            layer.ChangeStateNow(NpcMovementStateType.ConfrontingTrace);
            return;
        }
        if(stateEnterTime + repositionDuration + npcCombatData.repositionDuration < Time.time)
        {
            layer.ChangeStateNow(NpcMovementStateType.ConfrontingTrace);
            return;
        }

        if (IsBack || IsFont)
        {
            if (IsFont && targetDistance < confrontingDistance)
            {
                owner.animator.SetFloat("x", 0);
                owner.animator.SetFloat("y", 0);
                IsFont = false;
            }
            if (IsBack && targetDistance > confrontingDistance)
            {
                owner.animator.SetFloat("x", 0);
                owner.animator.SetFloat("y", 0);
                IsBack = false;
            }
            return;
        }

        if(targetDistance > confrontingDistance + npcCombatData.confrontingBandingOffset)
        {
            IsFont = true;
            owner.animator.SetFloat("x", 0);
            owner.animator.SetFloat("y", 1);
        }
        else if(targetDistance < confrontingDistance - npcCombatData.confrontingBandingOffset)
        {
            IsBack = true;
            owner.animator.SetFloat("x", 0);
            owner.animator.SetFloat("y", -1);
        }
    }
}

// 대치위치로 전속력 이동
public class NpcMovementState_ConfrontingTrace : FsmState
{
    new NpcCharacterActorBase owner;
    CharacterCombatData combatData;
    float confrontingBandingOffset;
    Vector2 confrontingPositionOffset;

    public NpcMovementState_ConfrontingTrace() : base(NpcMovementStateType.ConfrontingTrace) { }

    public override async UniTask Enter()
    {
        await base.Exit();
        owner = GetOwner<NpcCharacterActorBase>();
        owner.IsConfrontingTrace = true;
        combatData = owner.characterData.combatData;
        confrontingBandingOffset = Random.Range(-(float)combatData.confrontingBandingOffset, (float)combatData.confrontingBandingOffset);
        confrontingPositionOffset = Random.insideUnitCircle * combatData.congrontingPositionOffset;

        // Animation Setting
        {
            owner.animator.speed = 1;
            owner.animator.applyRootMotion = true;
            owner.animator.SetFloat("x", 0);
            owner.animator.SetFloat("y", 1);
            owner.animator.CrossFadeInFixedTime("StandRun", 0.15f);
            owner.legsAnimator.CrossFadeActive(false);
        }
    }

    public override async UniTask Exit()
    {
        await base.Exit();
        owner.navMeshAgent.ResetPath();
        owner.IsConfrontingTrace = false;
    }

    public override void Update()
    {
        base.Update();

        if (owner.IsDodge)
            return;

        // Target Check
        var target = owner.targetController.forcusTarget;
        if (target == null)
        {
            layer.ChangeStateNow(NpcMovementStateType.Idle);
            return;
        }

        // Position Setting
        var ownerPosition = owner.baseObject.transform.position;
        var targetPosition = target.baseObject.transform.position;

        // Transform Setting
        owner.baseObject.transform.LookAt_Y(target.baseObject.transform.position, 360f);

        // Animation Setting
        {
            // 1. 현재 이동방향 획득
            Vector3 currentDirection = owner.navMeshAgent.desiredVelocity.normalized;
            Vector3 localDirection = owner.baseObject.transform.InverseTransformDirection(currentDirection);
            Vector3 currentV = new Vector3(owner.animator.GetFloat("x"), 0, owner.animator.GetFloat("y"));
            localDirection = Vector3.Lerp(currentV, localDirection, 5f * Time.deltaTime);

            owner.animator.SetFloat("x", localDirection.x);
            owner.animator.SetFloat("y", localDirection.z);
        }

        // Confronting Setting
        var confrontingPosition = target.targetController.GetForcuedTargetAroundPosition(owner);
        var targetDirection = (confrontingPosition - target.baseObject.transform.position).normalized;
        confrontingPosition += targetDirection * confrontingBandingOffset;
        confrontingPosition.y = owner.baseObject.transform.position.y;
        confrontingPosition.x += confrontingPositionOffset.x;
        confrontingPosition.z += confrontingPositionOffset.y;

        // 현재 지점에서 목표지점까지 목표대상과의 각도
        float angle = Vector3Ex.AngleBetweenPoints(target.baseObject.transform.position, ownerPosition, confrontingPosition);

        if (angle > 70)
        {
            float subPositionDistance = Vector3.Distance(confrontingPosition, targetPosition);
            Vector3 ownerDirection = (ownerPosition - targetPosition).normalized;
            ownerDirection = ownerDirection.Rotate(angle * 0.5f, Vector3.up);
            var subPosition = targetPosition + ownerDirection * subPositionDistance + ownerDirection * 0.5f;

            owner.navMeshAgent.SetDestination(subPosition);
            GizmosSystem.Instance.DrawLine(owner.baseObject.transform.position, subPosition);
            return;
        }
        else if (angle < -70)
        {
            float subPositionDistance = Vector3.Distance(confrontingPosition, targetPosition);
            Vector3 ownerDirection = (ownerPosition - targetPosition).normalized;
            ownerDirection = ownerDirection.Rotate(angle * 0.5f, Vector3.up);
            var subPosition = targetPosition + ownerDirection * subPositionDistance + ownerDirection * 0.5f;

            owner.navMeshAgent.SetDestination(subPosition);
            GizmosSystem.Instance.DrawLine(owner.baseObject.transform.position, subPosition);
            return;
        }

        // Contronting End Position
        var distance = Vector3.Distance(confrontingPosition, ownerPosition);
        if (distance < 0.5f)
        {
            layer.ChangeStateNow(NpcMovementStateType.Confronting);
            return;
        }

        // Navigation Setting
        owner.navMeshAgent.SetDestination(confrontingPosition);
        GizmosSystem.Instance.DrawLine(owner.baseObject.transform.position, confrontingPosition);
    }
}

public class NpcMovementState_RunToAttack : FsmState
{
    new NpcCharacterActorBase owner;
    float distanseFromTarget = 0.0f;
    AttackNode attackNode;
    public NpcMovementState_RunToAttack() : base(NpcMovementStateType.RunToAttack) { }

    public override async UniTask Enter()
    {
        await base.Enter();
        owner = GetOwner<NpcCharacterActorBase>();
        owner.navMeshAgent.ResetPath();
        owner.IsRunToAttack = true;
        attackNode = owner.combatController.currentAttackNode;
        distanseFromTarget = Mathf.Max(attackNode.attackerTransformSetting.distanceFromTarget, 0.75f);

        currentAnimationTag = "Sprint";
        owner.animator.applyRootMotion = true;
        owner.animator.SetFloat("x", 0);
        owner.animator.SetFloat("y", 1);
        owner.animator.CrossFadeInFixedTime("StandSprint", 0.1f);
        owner.legsAnimator.CrossFadeActive(false);
    }

    public override async UniTask Exit()
    {
        await base.Exit();
        owner.IsRunToAttack = false;
    }

    public override void Update()
    {
        base.Update();

        if (owner.IsDeath || owner.IsHit)
        {
            owner.CancleAttack();
            return;
        }

        GizmosSystem.Instance.DrawLine(owner.baseObject.transform.position, owner.targetController.forcusTarget.baseObject.transform.position);

        var target = owner.targetController.forcusTarget;
        if (target == null)
        {
            layer.ChangeStateNow(NpcMovementStateType.Idle);
            return;
        }
       
        owner.baseObject.transform.LookAt_Y(target.baseObject.transform, 360.0f);

        var distance = Vector3.Distance(owner.baseObject.transform.position, target.baseObject.transform.position);
        if (distance < distanseFromTarget)
        {
            layer.ChangeStateNow(NpcMovementStateType.Idle);
            owner.OnAttack();
            return;
        }
    }
}