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
    Stunned,           // 기절
    Knockdown,         // 넉다운
    Slowed,            // 속도 감소
    Poisoned,          // 독 상태
    Burned,            // 화상
    Bleeding,          // 출혈 상태
    Weakened,          // 약화 (공격력 감소)
    DefenseReduced,    // 방어력 감소
    Silenced,          // 스킬 사용 불가
    Frozen,            // 동결 (움직임 불가)
    Blind,             // 실명 (명중률 감소)
    Fear,              // 공포 (도망가게 만듦)
    Rooted             // 고정 (이동 불가)
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
