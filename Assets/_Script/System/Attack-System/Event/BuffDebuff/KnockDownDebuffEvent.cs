using System;
using UnityEngine;

[Serializable]
public class KnockDownDebuffEvent : BuffDebuffEventBase
{
    protected override void Enter(CharacterActorBase owner)
    {
        base.Enter(owner);

        if(owner is NpcCharacterActorBase npc)
        {
            npc.IsKnockDown = true;
            npc.fsmContext.ChangeStateNow(NpcFsmLayer.BodyLayer, NpcBodyStateType.KnockDown_Up);
        }
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
