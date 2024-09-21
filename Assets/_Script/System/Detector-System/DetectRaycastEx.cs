using System.Collections.Generic;
using UnityEngine;

public static class DetectRaycastEx 
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ownerTransform"></param>
    /// <param name="range"></param>
    /// <param name="targetlayer"></param>
    /// <returns></returns>
    public static (Transform, List<Transform>)? ScanNearByWithCone(Transform ownerTransform, float range, float angle, LayerMask targetlayer)
    {
        Collider[] nearbyTargets = Physics.OverlapSphere(ownerTransform.position, range, targetlayer);
        if (nearbyTargets == null) return null;
        if (nearbyTargets.Length <= 0) return null;

        float baseAngle = angle / 2;
        float closestAngle = baseAngle;

        Transform closestTarget = null;
        List<Transform> targets = new();

        for (int i = 0; i < nearbyTargets.Length; i++)
        {
            Vector3 hor_dir = nearbyTargets[i].transform.position - ownerTransform.position;
            hor_dir.y = 0;
            float hor_angle = Vector3.Angle(ownerTransform.forward, hor_dir);

            if (hor_angle < closestAngle)
            {
                targets.Add(nearbyTargets[i].transform);
            }
        }

        float closestDistance = float.MaxValue;
        foreach (var target in targets)
        {
            float distance = Vector3.Distance(ownerTransform.position, target.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return (closestTarget, targets);
    }

    public static (Transform, List<Transform>)? ScanNearByWithBox(Transform ownerTransform, Vector3 localOffset, Vector3 localRotationOffset, Vector3 size, LayerMask targetLayer)
    {
        // ���� �������� ���� ��ǥ�� ��ȯ�Ͽ� �ڽ��� �߽� ���
        Vector3 boxCenter = ownerTransform.TransformPoint(localOffset);

        // ���� ȸ�� �������� ���� ȸ������ ��ȯ
        Quaternion rotationOffset = Quaternion.Euler(localRotationOffset);
        Quaternion boxRotation = ownerTransform.rotation * rotationOffset;

        // ������ �ڽ� ���� ���� Ÿ���� �˻� (�ڽ��� ȸ���� ������Ʈ�� ���� ȸ���� ����)
        Collider[] nearbyTargets = Physics.OverlapBox(boxCenter, size / 2f, boxRotation, targetLayer);

        // Ÿ���� ���� ��� null ��ȯ
        if (nearbyTargets == null || nearbyTargets.Length == 0) return null;

        Transform closestTarget = null;
        List<Transform> targets = new List<Transform>();

        // ��� �˻��� Ÿ�ٿ� ���� �ݺ�
        for (int i = 0; i < nearbyTargets.Length; i++)
        {
            Vector3 horDir = nearbyTargets[i].transform.position - ownerTransform.position;
            horDir.y = 0; // ���� ���� ���� ���

            // Ÿ���� ����Ʈ�� �߰�
            targets.Add(nearbyTargets[i].transform);
        }

        // ���� ����� Ÿ�� ã��
        float closestDistance = float.MaxValue;
        foreach (var target in targets)
        {
            float distance = Vector3.Distance(ownerTransform.position, target.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }

        return (closestTarget, targets);
    }
}
