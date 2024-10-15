using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcCombatController : MonoBehaviour
{
    [Header("CombatData Setting")]
    public AttackPatternData attackPatternData;

    [Header("Runtime Value")]
    [SerializeField] private NpcCharacterActorBase ownerCharacter;
    [SerializeField] public AttackComboData currentAttackCombo;
    [SerializeField] public AttackNode currentAttackNode;

    public Coroutine coroutineUpdateReadyToAttack;

    public void Init(NpcCharacterActorBase ownerCharacter)
    {
        this.ownerCharacter = ownerCharacter;
        coroutineUpdateReadyToAttack = StartCoroutine(UpdateReayToAttack());
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

    IEnumerator UpdateReayToAttack()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.15f);

            if (ownerCharacter.IsDeath)
                continue;

            if (ownerCharacter.IsReadyToAttack) 
                continue;
            if (ownerCharacter.IsRunToAttack)
                continue;

            if (ownerCharacter.IsAttack)
                continue;
            if (ownerCharacter.IsHit)
                continue;

            if (ownerCharacter.IsDodge)
                continue;
            if (!ownerCharacter.IsConfronting)
                continue;
            if (!ownerCharacter.targetController.forcusTarget)
                continue;

            var attackCombo = attackPatternData.FindAttackCombo(ownerCharacter);
            if (attackCombo == null)
                continue;
            
            currentAttackNode = attackCombo.attackNodes.First();
            if (currentAttackNode)
            {
                SetAttackCombo(attackCombo);
                ownerCharacter.targetController.forcusTarget.targetController.AddAttackableWaitingActor(ownerCharacter);
            }
        }
    }

    public void Exit()
    {
        if (coroutineUpdateReadyToAttack != null)
            StopCoroutine(coroutineUpdateReadyToAttack);
    }

    public void ExitAttack()
    {
        currentAttackNode = null;
    }

    //public void Update()
    //{
    //}
}
