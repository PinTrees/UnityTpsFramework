using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class UserWeaponData
{
    public HumanBodyBones parentBone;
    public WeaponData weaponData;
}

[Serializable]
public class UserCharacterData
{
    public List<UserWeaponData> equipWeaponDatas = new();
}
