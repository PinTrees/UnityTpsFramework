using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AttackCondition : ScriptableObject
{
    public List<TargetGroupTag> targetLayers;

    // Runtime Value
    protected AttackNode ownerNode;


    public virtual bool Check(CharacterActorBase owner)
    {
        foreach (var targetLayer in targetLayers)
        {
            bool isMatch = owner.characterTags.Any(e => e.tag == targetLayer.tag);
            if (isMatch)
            {
                return false;
            }
        }

        return true; 
    }

    public virtual void Exit(CharacterActorBase owner)
    {

    }
}
