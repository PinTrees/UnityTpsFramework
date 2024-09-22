using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public CharacterActorBase ownerCharacter;

    public List<Weapon> equipOnWeapons = new();  
    public List<Weapon> equipOffWeapons = new(); 


    public void Init(CharacterActorBase owner)
    {
        ownerCharacter = owner;
    }

    public void EquipWeapon(Weapon weapon)
    {
        equipOnWeapons.Add(weapon);
    }

    public Weapon GetEquipWeapon()
    {
        return equipOnWeapons.First();
    }
}
