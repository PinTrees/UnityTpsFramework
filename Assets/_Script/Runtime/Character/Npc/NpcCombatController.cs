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
    [SerializeField] public AttackNode currentAttackNode;

    public void Init(NpcCharacterActorBase ownerCharacter)
    {
        this.ownerCharacter = ownerCharacter;
        UpdateTask().Forget();
    }

    // Attackable Loop Task
    async UniTask UpdateTask()
    {
        while (true)
        {
            await UniTask.Yield();

            if (ownerCharacter.IsDeath)
                return;

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

            continue;

            //var attackCombo = attackPatternData.FindAttackCombo(ownerCharacter);
            //if (attackCombo == null)
            //    continue;
            //
            //currentAttackNode = attackCombo.attackNodes.First();
            //if (currentAttackNode)
            //{
            //    if(currentAttackNode.canJustDodge)
            //    {
            //        var attackVfx = VfxObject.Create("VfxSpark2");
            //        //var transform = ownerCharacter.animator.GetBoneTransform(HumanBodyBones.RightHand).transform;
            //        //attackVfx.transform.SetParent(transform, true);
            //        attackVfx.transform.localScale = Vector3.one;
            //        attackVfx.transform.position = ownerCharacter.GetCenterOfMass() + Vector3.up;
            //        attackVfx.transform.DOScale(Vector3.one * 1.5f, 0.35f).OnComplete(() =>
            //        {
            //            attackVfx.Stop();
            //        });
            //    }
            //
            //    ownerCharacter.IsAttack = true;
            //    yield return ownerCharacter.StartCoroutine(ownerCharacter.fsmContext.ChangeStateNowAsync(NpcFsmLayer.MovementLayer, NpcMovementStateType.Idle));
            //    yield return ownerCharacter.StartCoroutine(ownerCharacter.fsmContext.ChangeStateNowAsync(NpcFsmLayer.AttackLayer, NpcAttackStateType.Attack));
            //}
        }
    }

    public void ExitAttack()
    {
        currentAttackNode = null;
    }

    //public void Update()
    //{
    //}
}
