using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

[Serializable]
public class TargetHealthCondition : AttackCondition
{
    public int targetHealth;

    public override bool Check(CharacterActorBase owner)
    {
        return base.Check(owner);
    }
}

