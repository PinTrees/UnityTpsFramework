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

    public void DropEquipOnWeapon()
    {
        for(int i = 0; i < equipOnWeapons.Count; ++i)
        {
            equipOnWeapons[i].transform.SetParent(null, true);
            equipOnWeapons[i].boxCollider.enabled = true;
            equipOnWeapons[i].rb.isKinematic = false;

            // 무기를 랜덤하게 튕겨나가게 할 힘의 크기
            float upwardForce = 2.5f * equipOnWeapons[i].rb.mass;     // 위로 가는 힘
            float horizontalForce = 1f * equipOnWeapons[i].rb.mass;   // 옆으로 가는 힘
            float torqueForce = 5f * equipOnWeapons[i].rb.mass;       // 회전력

            // 랜덤한 힘을 무기에게 가함
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),   // 좌우 방향 랜덤
                Random.Range(0.5f, 1.5f), // 위로 가는 방향 (0.5 ~ 1.5 정도로 설정)
                Random.Range(-1f, 1f));  // 앞뒤 방향 랜덤

            // AddForce로 무기를 튕겨나가게 함 (질량 보정)
            equipOnWeapons[i].rb.AddForce((randomDirection.normalized * horizontalForce + Vector3.up * upwardForce), ForceMode.Impulse);

            // 랜덤한 회전력 추가 (질량 보정)
            Vector3 randomTorque = new Vector3(
                Random.Range(-1f, 1f) * torqueForce,
                Random.Range(-1f, 1f) * torqueForce,
                Random.Range(-1f, 1f) * torqueForce);
            equipOnWeapons[i].rb.AddTorque(randomTorque);
        }

        equipOnWeapons.Clear();
    }

    public void EquipWeapon(Weapon weapon, HumanBodyBones equipTargetBone)
    {
        var weaponEquipPosition = weapon.weaponData.equipPositions.Where(e => e.parentBoneType == equipTargetBone).FirstOrDefault();

        // 부모의 예상 축 정보 (무기 데이터에 저장된 부모 축)
        var expectedParentUpAxis = weaponEquipPosition.upAxis;
        var expectedParentForwardAxis = weaponEquipPosition.forwardAxis;

        // 현재 타겟 캐릭터 뼈대 축 정보 초기화
        var actualParentUpAxis = ScriptableBoneAxisType.X;
        var actualParentForwardAxis = ScriptableBoneAxisType.Y;

        // 실제 캐릭터의 부모 뼈대 축 정보
        var currentTargetBone = boneController.GetScriptableBone(equipTargetBone);
        if (currentTargetBone != null)
        {
            actualParentUpAxis = currentTargetBone.upAxis;
            actualParentForwardAxis = currentTargetBone.forwardAxis;
        }

        // 부모 Transform 설정
        var parentTransform = ownerCharacter.animator.GetBoneTransform(equipTargetBone);
        weapon.transform.SetParent(parentTransform, false);

        // 무기의 로컬 위치와 회전 설정
        weapon.transform.localPosition = weaponEquipPosition.localPosition;
        weapon.transform.localRotation = Quaternion.Euler(weaponEquipPosition.localEulerAngle);

        // 예상 부모 축과 실제 부모 축의 차이를 계산하여 무기의 회전을 조정
        AdjustWeaponRotation(weapon.transform, expectedParentUpAxis, expectedParentForwardAxis, actualParentUpAxis, actualParentForwardAxis);

        equipOnWeapons.Add(weapon);
    }

    public Weapon GetEquipWeapon()
    {
        return equipOnWeapons.First();
    }

    private void AdjustWeaponRotation(Transform weaponTransform, ScriptableBoneAxisType expectedUp, ScriptableBoneAxisType expectedForward, ScriptableBoneAxisType actualUp, ScriptableBoneAxisType actualForward)
    {
        // 예상 부모 축과 실제 부모 축 사이의 회전을 계산
        Quaternion rotationFromExpectedToActual = GetRotationFromExpectedToActualAxes(expectedUp, expectedForward, actualUp, actualForward);
        // 무기의 로컬 회전에 회전을 적용하여 보정
        weaponTransform.localRotation = rotationFromExpectedToActual * weaponTransform.localRotation;
    }

    private Quaternion GetRotationFromExpectedToActualAxes(ScriptableBoneAxisType expectedUpAxis, ScriptableBoneAxisType expectedForwardAxis, ScriptableBoneAxisType actualUpAxis, ScriptableBoneAxisType actualForwardAxis)
    {
        // 예상 부모 축을 벡터로 변환
        Vector3 expectedUp = GetDirectionFromAxis(expectedUpAxis);
        Vector3 expectedForward = GetDirectionFromAxis(expectedForwardAxis);

        // 실제 부모 축을 벡터로 변환
        Vector3 actualUp = GetDirectionFromAxis(actualUpAxis);
        Vector3 actualForward = GetDirectionFromAxis(actualForwardAxis);

        // 예상 부모 축 기반의 좌표계 생성
        Vector3 expectedRight = Vector3.Cross(expectedUp, expectedForward).normalized;
        expectedForward = Vector3.Cross(expectedRight, expectedUp).normalized;

        // 실제 부모 축 기반의 좌표계 생성
        Vector3 actualRight = Vector3.Cross(actualUp, actualForward).normalized;
        actualForward = Vector3.Cross(actualRight, actualUp).normalized;

        // 예상 부모 좌표계에서 실제 부모 좌표계로의 회전을 계산
        Quaternion rotation = Quaternion.LookRotation(actualForward, actualUp) * Quaternion.Inverse(Quaternion.LookRotation(expectedForward, expectedUp));

        return rotation;
    }

    private Vector3 GetDirectionFromAxis(ScriptableBoneAxisType axis)
    {
        switch (axis)
        {
            case ScriptableBoneAxisType.X:
                return Vector3.right;
            case ScriptableBoneAxisType.Y:
                return Vector3.up;
            case ScriptableBoneAxisType.Z:
                return Vector3.forward;
            case ScriptableBoneAxisType.NEG_X:
                return Vector3.left;
            case ScriptableBoneAxisType.NEG_Y:
                return Vector3.down;
            case ScriptableBoneAxisType.NEG_Z:
                return Vector3.back;
            default:
                return Vector3.up; // 기본값
        }
    }
}
