using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NpcCombatData
{
}

[Serializable]
public class NpcWeaponData
{
    public HumanBodyBones parentBone;
    public WeaponData weaponData;
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

    [Header("Equip On Weapon")]
    public List<NpcWeaponData> equipOnWeaponDatas;

    [Header("Equip Off Weapon")]
    public List<NpcWeaponData> equipOffWeaponDatas; 
}
