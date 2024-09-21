using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public enum BuffDebuffType
{
    None,

    // Buffs
    __Buff__ = 100,

    // Debuffs
    __Debuff__ = 200,
    Stunned,           // ����
    Knockdown,         // �˴ٿ�
    Slowed,            // �ӵ� ����
    Poisoned,          // �� ����
    Burned,            // ȭ��
    Bleeding,          // ���� ����
    Weakened,          // ��ȭ (���ݷ� ����)
    DefenseReduced,    // ���� ����
    Silenced,          // ��ų ��� �Ұ�
    Frozen,            // ���� (������ �Ұ�)
    Blind,             // �Ǹ� (���߷� ����)
    Fear,              // ���� (�������� ����)
    Rooted             // ���� (�̵� �Ұ�)
}

public enum BodyAnimationType
{ 
    None,
    KnockDownUp,
}

public enum BuffDebuffEnterTimeType
{
    None,
    StartState,
    ExitState,
}

[Serializable]
public class BuffDebuffEventBase : ScriptableObject
{
    [Header("Default Setting")]
    public BuffDebuffEnterTimeType buffdebuffEnterTime; 
    public float duration;

    [Header("Animation Setting")]
    public bool useRootMotion;
    public BodyAnimationType bodyAnimationType;


    public virtual async UniTask Run(CharacterActorBase owner)
    {
        Enter(owner);

        var spawnTime = Time.time;

        while (true)
        {
            // Late Update
            await UniTask.Yield(PlayerLoopTiming.PreLateUpdate);

            if (Time.time > spawnTime + duration)
                break;

            if (owner.IsDeath)
                return;

            Update(owner);
        }

        Exit(owner);
    }

    protected virtual void Enter(CharacterActorBase owner) 
    { 
         
    }

    protected virtual void Update(CharacterActorBase owner)
    {

    }

    protected virtual void Exit(CharacterActorBase owner)
    {

    }
}
