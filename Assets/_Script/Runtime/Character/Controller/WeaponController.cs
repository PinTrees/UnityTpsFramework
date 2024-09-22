using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Rnutime Value")]
    public CharacterActorBase ownerCharacter;
    public ScriptableBoneController boneController;

    public List<Weapon> equipOnWeapons = new();
    public List<Weapon> equipOffWeapons = new();

    public void Init(CharacterActorBase owner)
    {
        ownerCharacter = owner;
        boneController = GetComponent<ScriptableBoneController>();
        boneController?.Init(owner);
    }

    public void EquipWeapon(Weapon weapon, HumanBodyBones equipTargetbone)
    {
        var weaponEquipPosition = weapon.weaponData.equipPositions.Where(e => e.parentBoneType == equipTargetbone).FirstOrDefault();

        var parentTransform = ownerCharacter.animator.GetBoneTransform(equipTargetbone);
        weapon.transform.SetParent(parentTransform, true);
        weapon.transform.localPosition = weaponEquipPosition.localPosition;
        weapon.transform.localEulerAngles = weaponEquipPosition.localEulerAngle;

        equipOnWeapons.Add(weapon);
    }

    public Weapon GetEquipWeapon()
    {
        return equipOnWeapons.First();
    }
}
