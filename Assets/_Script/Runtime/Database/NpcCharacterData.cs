using System;
using UnityEngine;

[Serializable]
public class NpcCombatData
{
    public float traceStartRange;

    public float confrontingRange;              // 전투 대상과의 대치 거리
    public float confrontingBandingOffset;      // 전투 대상과의 대치 거리 유효 오프셋
    public float confrontingPositionOffset;     // 전투 대상과의 대치 거리 최종 위치 오프셋

    public float repositionDuration;                // 대치 위치 재조정 쿨타임
    public float repositionDurationRandomOffset;    // 대치 위치 재조정 랜덤 쿨타임
}

[CreateAssetMenu(menuName = "Scriptable/Database/Npc")]
public class NpcCharacterData : CharacterData
{
    [Header("Npc Character Setting")]
    [UID]
    public string uid;

    [Header("HostilityCriteria Setting")]
    public float hostilityLimit;
    public HostilityCriteriaWeight hostilityCriteriaWeight;

    [Header("Combat Setting")]
    public NpcCombatData npcCombatData;

    [Header("Object Setting")]
    public GameObject perfab;
}
