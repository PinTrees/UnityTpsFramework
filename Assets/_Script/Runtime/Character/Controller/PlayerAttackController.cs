using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttackController : MonoBehaviour
{
    public List<TargetGroupTag> attackableTargetGroupTags = new();  
    public AttackPatternData attackPatternData;

    [Header("Runtime Value")]
    [SerializeField] private PlayerCharacterActorBase ownerCharacter;
    [SerializeField] private AttackComboData currentAttackCombo;
    [SerializeField] public AttackNode currentAttackNode;

    public CharacterActorBase justDodgeAttacker;
    public AttackNode justDodgeAttackNode;
    private bool isInit = false;

    public void Init(PlayerCharacterActorBase owner)
    {
        if (isInit)
            return;
        isInit = true;

        ownerCharacter = owner;
        UpdateAttackTask().Forget();
        UpdateJustDodgeTask().Forget();
    }

    async UniTask UpdateJustDodgeTask()
    {
        while (true)
        {
            await UniTask.Yield();

            if (ownerCharacter.IsHit)
                continue;
            if (ownerCharacter.IsDodge)
                continue;
            if (ownerCharacter.IsJustDodge)
                continue;
            if (ownerCharacter.IsAttack)
                continue;

            // Input Setting
            //if (!Input.GetKey(KeyCode.Mouse1))
            //    continue;

            // 객체 변경 잠금 설정 필요
            foreach (var attacker in ownerCharacter.targetController.forcusedByCharacters)
            {
                if (!attacker.IsAttack)
                    continue;

                // 범용적으로 재설계 필요
                if (attacker is NpcCharacterActorBase npc)
                {
                    if (npc.combatController.currentAttackNode == null)
                        continue;
                    if (!npc.combatController.currentAttackNode.canJustDodge)
                        continue;

                    var distance = npc.combatController.currentAttackNode.canJustDodgeDistance;
                    var animationTag = npc.combatController.currentAttackNode.uid;
                    var animationNormalizeTime = npc.combatController.currentAttackNode.canJustDodgeNormailzetime;

                    if (Vector3.Distance(ownerCharacter.baseObject.transform.position, npc.baseObject.transform.position) > distance)
                        continue;

                    // 저스트 회피 발동
                    if (npc.animator.IsPlayedInTime(animationTag, animationNormalizeTime.start, animationNormalizeTime.exit))
                    {
                        // State Data Setting
                        ownerCharacter.IsJustDodge = true;
                        justDodgeAttacker = npc;
                        justDodgeAttackNode = npc.combatController.currentAttackNode;

                        // State Setting
                        var dodgeLayer = ownerCharacter.fsmContext.FindLayer(PlayerFsmLayer.DodgeLayer);
                        var movementLayer = ownerCharacter.fsmContext.FindLayer(PlayerFsmLayer.MovementLayer);
                        var attackLayer = ownerCharacter.fsmContext.FindLayer(PlayerFsmLayer.AttackLayer);
                        await attackLayer.ChangeStateNowAsync(PlayerAttackStateType.None);
                        await movementLayer.ChangeStateNowAsync(PlayerMovementStateType.Idle);
                        await dodgeLayer.ChangeStateNowAsync(PlayerDodgeStateType.JustDodge);

                        // 최조 조건 만족시 즉시 진행
                        break;
                    }
                }
            }
        }
    }

    async UniTask UpdateAttackTask()
    {
        if (attackPatternData == null)
        {
            return;
        }

        while (true)
        {
            await UniTask.Yield();

            if (ownerCharacter.IsJustDodge)
                continue;
            if (ownerCharacter.IsHit)
                continue;
            if (ownerCharacter.IsAttack)
                continue;
            if (ownerCharacter.IsDodge)
                continue;

            // Attack Combo Setting
            currentAttackCombo = attackPatternData.FindAttackCombo(ownerCharacter);
            if (currentAttackCombo == null)
                continue;

            // Attack Node Setting
            currentAttackNode = currentAttackCombo.attackNodes.First();
            if (currentAttackNode != null)
            {
                ownerCharacter.IsAttack = true;

                // State Chain Setting - 반드시 연속 체인 변경 - Queue
                var dodgeLayer = ownerCharacter.fsmContext.FindLayer(PlayerFsmLayer.DodgeLayer);
                var movementLayer = ownerCharacter.fsmContext.FindLayer(PlayerFsmLayer.MovementLayer);
                var attackLayer = ownerCharacter.fsmContext.FindLayer(PlayerFsmLayer.AttackLayer);
                await dodgeLayer.ChangeStateNowAsync(PlayerDodgeStateType.None);
                await movementLayer.ChangeStateNowAsync(PlayerMovementStateType.Idle);
                await attackLayer.ChangeStateNowAsync(PlayerAttackStateType.Attack);
            }
        }
    }

    public void SetNextComboAttack(AttackNode attackNode)
    {
        currentAttackNode = attackNode;
    }
    public void ClearAttackCombo()
    {
        currentAttackCombo = null;
        currentAttackNode = null;
    }
    public void ClearJustDodgeData()
    {
        justDodgeAttacker = null;
        justDodgeAttackNode = null;
    }

    public int GetCurrentAttackComboCount()
    {
        if (currentAttackCombo == null)
            return 0;
        return currentAttackCombo.attackNodes.Count;
    }

    public AttackNode CanNextAttackCombo(AttackNode currentAttackNode)
    {
        if (currentAttackCombo == null)
            return null;
        
        var currentAttackComboIndex = currentAttackCombo.attackNodes.IndexOf(currentAttackNode);
        if (currentAttackCombo.attackNodes.Count <= currentAttackComboIndex + 1)
            return null;

        var nextAttackCombo = currentAttackCombo.attackNodes[currentAttackComboIndex + 1];
        if (nextAttackCombo.CanAttack(ownerCharacter))
            return nextAttackCombo;
        
        return null;
    }

    public void SetAttackCombo(AttackComboData attackCombo)
    {
        currentAttackCombo = attackCombo;
    }

    public void Update()
    {
    }
}
