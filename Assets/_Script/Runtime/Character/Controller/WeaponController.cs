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

            // ���⸦ �����ϰ� ƨ�ܳ����� �� ���� ũ��
            float upwardForce = 2.5f * equipOnWeapons[i].rb.mass;     // ���� ���� ��
            float horizontalForce = 1f * equipOnWeapons[i].rb.mass;   // ������ ���� ��
            float torqueForce = 5f * equipOnWeapons[i].rb.mass;       // ȸ����

            // ������ ���� ���⿡�� ����
            Vector3 randomDirection = new Vector3(
                Random.Range(-1f, 1f),   // �¿� ���� ����
                Random.Range(0.5f, 1.5f), // ���� ���� ���� (0.5 ~ 1.5 ������ ����)
                Random.Range(-1f, 1f));  // �յ� ���� ����

            // AddForce�� ���⸦ ƨ�ܳ����� �� (���� ����)
            equipOnWeapons[i].rb.AddForce((randomDirection.normalized * horizontalForce + Vector3.up * upwardForce), ForceMode.Impulse);

            // ������ ȸ���� �߰� (���� ����)
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

    public Weapon GetEquipWeapon()
    {
        return equipOnWeapons.First();
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
}
