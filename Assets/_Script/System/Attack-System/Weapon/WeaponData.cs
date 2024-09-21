using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class EquipPositionContainer
{
    public HumanBodyBones parentBoneType;

    public Vector3 localPosition;
    public Vector3 localEulerAngle;
}

[CreateAssetMenu(menuName = "Scriptable/Weapon")]
public class WeaponData : ScriptableObject
{
    public List<EquipPositionContainer> equipPositions = new();
    public GameObject perfab;

    public Weapon Equip(HumanBodyBones equipPosition, CharacterActorBase owner)
    {
        var weaponPerfab = GameObject.Instantiate(perfab);
        var weaponEquipPosition = equipPositions.Where(e => e.parentBoneType == equipPosition).FirstOrDefault();

        var parentTransform = owner.animator.GetBoneTransform(equipPosition);
        weaponPerfab.transform.SetParent(parentTransform, true);
        weaponPerfab.transform.localPosition = weaponEquipPosition.localPosition;
        weaponPerfab.transform.localEulerAngles = weaponEquipPosition.localEulerAngle;

        return weaponPerfab.GetComponent<Weapon>();
    }

#if UNITY_EDITOR
    public void Save(EquipPositionContainer equipPosition)
    {
        var target = equipPositions.Where(e => e.parentBoneType == equipPosition.parentBoneType).FirstOrDefault();
        if (target == null)
            equipPositions.Add(equipPosition);
        else
        {
            equipPositions.Remove(target);
            equipPositions.Add(equipPosition);
        }

        // 객체가 수정되었음을 Unity 에디터에 알림
        EditorUtility.SetDirty(this);

        // 변경된 내용을 저장
        AssetDatabase.SaveAssets(); 
        AssetDatabase.Refresh();
    }
#endif
}
