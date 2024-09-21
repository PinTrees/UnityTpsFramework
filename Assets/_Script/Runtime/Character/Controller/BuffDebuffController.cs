using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BuffDebuffEventContainer
{
    public BuffDebuffEventBase eventData;
    public void Update()
    {

    }
}

public class BuffDebuffController : MonoBehaviour
{
    [Header("Runtime Value")]
    public CharacterActorBase owner;
    public List<BuffDebuffEventContainer> buffDebuffs = new();

    private bool _lockUpdate = false;

    public void Init(CharacterActorBase owner)
    {
        this.owner = owner;
    }

    public void AddBuffDebuff(BuffDebuffEventBase eventData)
    {
        _lockUpdate = true;



        _lockUpdate = false;
    }

    public void RemoveBuffDebuff()
    {
        _lockUpdate = true;



        _lockUpdate = false;
    }

    public void Update()
    {
        if (_lockUpdate)
            return;
    }
}
