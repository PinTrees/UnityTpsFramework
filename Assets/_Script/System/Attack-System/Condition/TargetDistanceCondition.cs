using UnityEngine;

public class TargetDistanceCondition : AttackCondition
{
    public float distanceFromOwner; 

    public override bool Check(CharacterActorBase owner)
    {
        CharacterActorBase target = null;

        foreach(var e in owner.targetController.targets)
        {
            if (Vector3.Distance(owner.baseObject.transform.position, e.baseObject.transform.position) < distanceFromOwner)
            {
                target = e;
                break;
            }
        }

        if (target)
        {
            owner.targetController.lockDetectUpdate = true;
            owner.targetController.forcusTarget = target;
            return true;
        }
        else return false;
    }

    public override void Exit(CharacterActorBase owner)
    {
        base.Exit(owner);

        owner.targetController.lockDetectUpdate = false;
    }
}
