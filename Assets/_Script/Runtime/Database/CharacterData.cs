using System;
using UnityEngine;

[Serializable]
public class CharacterCombatData
{
    public float traceStartRange;

    public float avoidDistance;                     // ȸ�ǰŸ�

    public float confrontingRange;                  // ���� ������ ��ġ �Ÿ�
    public float confrontingBandingOffset;          // ���� ������ ��ġ �Ÿ� ��ȿ ������
    public float congrontingPositionOffset;         // ���� ������ ��ġ �Ÿ� ���� ��ġ ������

    public float repositionDuration;                // ��ġ ��ġ ������ ��Ÿ��
    public float repositionDurationRandomOffset;    // ��ġ ��ġ ������ ���� ��Ÿ��
}

[Serializable]
public class CharacterData : ScriptableObject
{
    [Header("Character Setting")]
    public CharacterCombatData combatData;
}
