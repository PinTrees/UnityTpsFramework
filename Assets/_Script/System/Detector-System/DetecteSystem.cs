using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public enum Detect_Shape_Type
{
    Speher,
    Box,
    Arc,
}

[System.Serializable]
public struct DetectorSetting
{
    public Detect_Shape_Type shapeType;
    public LayerMask detectLayer;
    
    [Space]
    public float detectRange;         
    public float detectAngle;         
    public float detectMaxCount;

    [Space]
    public Vector3 offset;
    public Vector3 rotationOffset;
    public Vector3 size;
}

/// <summary>
/// 범위 내의 타겟을 감지하고 이벤트를 발생시키는 고급 감지기.
/// </summary>
public class DetecteSystem 
{
    [Header("Runtime System Values")]
    public static GameObject boxColliderObject;
    public static BoxCollider boxColliderUnit;
    public static int detectSystemLayerIndex;
    public static int defaultLayerIndex;


    // 명시적 초기화가 없을경우 초기화되지 않습니다.
    public static void Init()
    {
        if (boxColliderObject) GameObject.Destroy(boxColliderObject);

        detectSystemLayerIndex = LayerMask.NameToLayer("DetectSystemLayer");
        defaultLayerIndex = LayerMask.NameToLayer("Ignore Raycast");

        boxColliderObject = new GameObject("BoxObjectUnit");
        boxColliderObject.layer = defaultLayerIndex;
        boxColliderUnit = boxColliderObject.AddComponent<BoxCollider>();
        boxColliderUnit.size = Vector3.one;
        boxColliderUnit.isTrigger = true;
    }


    public static Transform FindTargetUpdate(Transform ownerTransform, List<Transform> targets, DetectorSetting setting)
    {
        targets.Clear();
        (Transform, List<Transform>)? ssss = null;

        if (setting.shapeType == Detect_Shape_Type.Speher)
        {
            var colliders = DetectRaycastEx.ScanObjectWithSpehre(ownerTransform, setting.detectRange, setting.detectLayer);
            if (colliders != null && colliders.Length > 0)
            {
                for (int i = 0; i < colliders.Length; ++i)
                    targets.Add(colliders[i].transform);
                return colliders.First().transform;
            }
            else return null;
        }
        else if (setting.shapeType == Detect_Shape_Type.Box)
        {
            Quaternion rotation = ownerTransform.rotation * Quaternion.Euler(setting.rotationOffset);
            Vector3 position = ownerTransform.position + rotation * setting.offset;

            var more = Mathf.Max(setting.size.x, setting.size.y, setting.size.z);
            Collider[] colliders = Physics.OverlapSphere(position, setting.detectRange > 0 ? setting.detectRange : more, setting.detectLayer);

            if (colliders != null && colliders.Length > 0)
            {
                boxColliderUnit.transform.rotation = rotation;
                boxColliderUnit.transform.position = position;
                boxColliderUnit.transform.localScale = setting.size;
                boxColliderUnit.gameObject.layer = detectSystemLayerIndex;

                Physics.SyncTransforms();

                // 충돌 검사 로직 추가
                foreach (Collider col in colliders)
                {
                    if (boxColliderUnit.bounds.Intersects(col.bounds))
                        targets.Add(col.transform);
                }
                
                boxColliderUnit.gameObject.layer = defaultLayerIndex;
                return targets.FirstOrDefault();
            }
            else return null;
        }
        else if (setting.shapeType == Detect_Shape_Type.Arc)
        {
            ssss = DetectRaycastEx.ScanNearByWithCone(ownerTransform, setting.detectRange, setting.detectAngle, setting.detectLayer);
        }
       
        if (ssss == null)
        {
            targets.Clear();
            return null;
        }

        targets.Clear();
        targets.AddRange(ssss.Value.Item2);
        return ssss.Value.Item1;
    }
}