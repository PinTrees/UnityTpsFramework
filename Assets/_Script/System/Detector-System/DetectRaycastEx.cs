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
        // 로컬 오프셋을 월드 좌표로 변환하여 박스의 중심 계산
        Vector3 boxCenter = ownerTransform.TransformPoint(localOffset);

        // 로컬 회전 오프셋을 월드 회전으로 변환
        Quaternion rotationOffset = Quaternion.Euler(localRotationOffset);
        Quaternion boxRotation = ownerTransform.rotation * rotationOffset;

        // 지정된 박스 범위 내의 타겟을 검색 (박스의 회전은 오브젝트의 로컬 회전을 따름)
        Collider[] nearbyTargets = Physics.OverlapBox(boxCenter, size / 2f, boxRotation, targetLayer);

        // 타겟이 없을 경우 null 반환
        if (nearbyTargets == null || nearbyTargets.Length == 0) return null;

        Transform closestTarget = null;
        List<Transform> targets = new List<Transform>();

        // 모든 검색된 타겟에 대해 반복
        for (int i = 0; i < nearbyTargets.Length; i++)
        {
            Vector3 horDir = nearbyTargets[i].transform.position - ownerTransform.position;
            horDir.y = 0; // 수평 방향 벡터 계산

            // 타겟을 리스트에 추가
            targets.Add(nearbyTargets[i].transform);
        }

        // 가장 가까운 타겟 찾기
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
