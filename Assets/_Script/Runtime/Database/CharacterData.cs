using System;
using UnityEngine;

[Serializable]
public class CharacterCombatData
{
    public float traceStartRange;

    public float avoidDistance;                     // 회피거리

    public float confrontingRange;                  // 전투 대상과의 대치 거리
    public float confrontingBandingOffset;          // 전투 대상과의 대치 거리 유효 오프셋
    public float congrontingPositionOffset;         // 전투 대상과의 대치 거리 최종 위치 오프셋

    public float repositionDuration;                // 대치 위치 재조정 쿨타임
    public float repositionDurationRandomOffset;    // 대치 위치 재조정 랜덤 쿨타임
}

[Serializable]
public class CharacterData : ScriptableObject
{
    [Header("Character Setting")]
    public CharacterCombatData combatData;
}
