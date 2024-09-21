using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class AttackEventContainer
{
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData eventNormalizeTime;
    public List<AttackEvent> events = new();

    private bool isOverStart;

    public void Init()
    {
        isOverStart = false;
    }

    public void UpdateEvent(AttackNode attackNode, CharacterActorBase owner) 
    {
        if (!isOverStart && owner.animator.IsPlayedOverTime(attackNode.uid, eventNormalizeTime.start))
        {
            events.ForEach(e => e.Enter());
            isOverStart = true;
        }

        if (owner.animator.IsPlayedInTime(attackNode.uid, eventNormalizeTime.start, eventNormalizeTime.exit))
        {
            events.ForEach(e =>
            {
                e.OnUpdateEvent(owner);
            });
        }
    }
}

public class AttackEvent : ScriptableObject
{
    public virtual void OnDrawGizmo(Transform transform)
    {

    }

    public virtual void Enter()
    {

    }

    public virtual bool OnUpdateEvent(CharacterActorBase owner)
    {
        return false;
    }

    public virtual void Exit()
    {

    }
}
