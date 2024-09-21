using UnityEngine;
using System;

[Serializable]
public class HitboxData 
{
    public HitDirectionType Direction;
    public CharacterActorBase ownerCharacter;

    public HitboxEvent eventData;
}

