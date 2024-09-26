using System;
using UnityEngine;

public enum KnockDownMotionType
{ 
    None,
    KnockDown_Up,
    KnockDown_Down,
}

[Serializable]
public class KnockDownDebuffEvent : BuffDebuffEventBase
{
    [Header("KnockDown Animation Setting")]
    public KnockDownMotionType motionType;

    protected override void Enter(CharacterActorBase owner)
    {
        base.Enter(owner);
        owner.OnKnockDown(motionType);
    }

    protected override void Exit(CharacterActorBase owner)
    {
        base.Exit(owner);

        if (owner is NpcCharacterActorBase npc)
        {
            npc.IsCanNotMove = true;
            npc.fsmContext.ChangeStateNow(NpcFsmLayer.BodyLayer, NpcBodyStateType.KnockDownToStand);
        }
    }

    protected override void Update(CharacterActorBase owner)
    {
        base.Update(owner);
    }
}
