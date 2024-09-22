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

    public void EquipWeapon(Weapon weapon, HumanBodyBones equipTargetBone)
    {
        var weaponEquipPosition = weapon.weaponData.equipPositions.Where(e => e.parentBoneType == equipTargetBone).FirstOrDefault();

        // �θ��� ���� �� ���� (���� �����Ϳ� ����� �θ� ��)
        var expectedParentUpAxis = weaponEquipPosition.upAxis;
        var expectedParentForwardAxis = weaponEquipPosition.forwardAxis;

        // ���� Ÿ�� ĳ���� ���� �� ���� �ʱ�ȭ
        var actualParentUpAxis = ScriptableBoneAxisType.X;
        var actualParentForwardAxis = ScriptableBoneAxisType.Y;

        // ���� ĳ������ �θ� ���� �� ����
        var currentTargetBone = boneController.GetScriptableBone(equipTargetBone);
        if (currentTargetBone != null)
        {
            actualParentUpAxis = currentTargetBone.upAxis;
            actualParentForwardAxis = currentTargetBone.forwardAxis;
        }

        // �θ� Transform ����
        var parentTransform = ownerCharacter.animator.GetBoneTransform(equipTargetBone);
        weapon.transform.SetParent(parentTransform, false);

        // ������ ���� ��ġ�� ȸ�� ����
        weapon.transform.localPosition = weaponEquipPosition.localPosition;
        weapon.transform.localRotation = Quaternion.Euler(weaponEquipPosition.localEulerAngle);

        // ���� �θ� ��� ���� �θ� ���� ���̸� ����Ͽ� ������ ȸ���� ����
        AdjustWeaponRotation(weapon.transform, expectedParentUpAxis, expectedParentForwardAxis, actualParentUpAxis, actualParentForwardAxis);

        equipOnWeapons.Add(weapon);
    }

    private void AdjustWeaponRotation(Transform weaponTransform, ScriptableBoneAxisType expectedUp, ScriptableBoneAxisType expectedForward, ScriptableBoneAxisType actualUp, ScriptableBoneAxisType actualForward)
    {
        // ���� �θ� ��� ���� �θ� �� ������ ȸ���� ���
        Quaternion rotationFromExpectedToActual = GetRotationFromExpectedToActualAxes(expectedUp, expectedForward, actualUp, actualForward);
        // ������ ���� ȸ���� ȸ���� �����Ͽ� ����
        weaponTransform.localRotation = rotationFromExpectedToActual * weaponTransform.localRotation;
    }

    private Quaternion GetRotationFromExpectedToActualAxes(ScriptableBoneAxisType expectedUpAxis, ScriptableBoneAxisType expectedForwardAxis, ScriptableBoneAxisType actualUpAxis, ScriptableBoneAxisType actualForwardAxis)
    {
        // ���� �θ� ���� ���ͷ� ��ȯ
        Vector3 expectedUp = GetDirectionFromAxis(expectedUpAxis);
        Vector3 expectedForward = GetDirectionFromAxis(expectedForwardAxis);

        // ���� �θ� ���� ���ͷ� ��ȯ
        Vector3 actualUp = GetDirectionFromAxis(actualUpAxis);
        Vector3 actualForward = GetDirectionFromAxis(actualForwardAxis);

        // ���� �θ� �� ����� ��ǥ�� ����
        Vector3 expectedRight = Vector3.Cross(expectedUp, expectedForward).normalized;
        expectedForward = Vector3.Cross(expectedRight, expectedUp).normalized;

        // ���� �θ� �� ����� ��ǥ�� ����
        Vector3 actualRight = Vector3.Cross(actualUp, actualForward).normalized;
        actualForward = Vector3.Cross(actualRight, actualUp).normalized;

        // ���� �θ� ��ǥ�迡�� ���� �θ� ��ǥ����� ȸ���� ���
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
                return Vector3.up; // �⺻��
        }
    }

    public Weapon GetEquipWeapon()
    {
        return equipOnWeapons.First();
    }
}
