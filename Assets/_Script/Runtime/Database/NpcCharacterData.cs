using System;
using UnityEngine;

[Serializable]
public class NpcCombatData
{
    public float traceStartRange;

    public float confrontingRange;              // ���� ������ ��ġ �Ÿ�
    public float confrontingBandingOffset;      // ���� ������ ��ġ �Ÿ� ��ȿ ������
    public float confrontingPositionOffset;     // ���� ������ ��ġ �Ÿ� ���� ��ġ ������

    public float repositionDuration;                // ��ġ ��ġ ������ ��Ÿ��
    public float repositionDurationRandomOffset;    // ��ġ ��ġ ������ ���� ��Ÿ��
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
