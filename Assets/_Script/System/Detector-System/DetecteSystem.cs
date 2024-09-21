using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public enum Detect_Shape_Type
{
    Speher,
    Box,
}

[System.Serializable]
public struct DetectorSetting
{
    public float detectRange;           // 감지할 레이어
    public LayerMask detectLayer;       // 감지 범위
    public float detectAngle;           // 감지 각도
    public float detectMaxCount;        // 감지 가능 수

    public Detect_Shape_Type shapeType; 
    public Vector3 offset;
    public Vector3 rotationOffset;
    public Vector3 size;
}

/// <summary>
/// 범위 내의 타겟을 감지하고 이벤트를 발생시키는 고급 감지기.
/// </summary>
public class DetecteSystem 
{
    [Header("Detection Settings")]
    public DetectorSetting setting;

    // 감지 이벤트
    public Action<Transform> onTargetDetected;
    public Action<Transform> onTargetUpdated;
    public Action<Transform> onTargetLost;
    public Func<Transform> onOverrideDetector;

    [Header("Runtime Debuge Values")]
    [SerializeField]
    private Transform currentTarget;
    private Transform temporaryTarget;

    private bool _init = false;


    // 명시적 초기화가 없을경우 초기화되지 않습니다.
    public void Init()
    {
        if (_init)
            return;

        // TODO
        temporaryTarget = null;
        currentTarget = null;

        _init = true;
    }


    #region Has Target
    /// <summary>
    /// 타겟이 있는지 확인합니다.
    /// </summary>
    public bool HasTarget() => currentTarget != null;

    /// <summary>
    /// 현재 타겟을 가져옵니다.
    /// </summary>
    public Transform GetCurrentTarget() => currentTarget;
    #endregion


    public static Transform FindTargetUpdate(Transform ownerTransform, List<Transform> targets, DetectorSetting setting)
    {
        (Transform, List<Transform>)? ssss = null;

        if (setting.shapeType == Detect_Shape_Type.Speher)
        {
            ssss = DetectRaycastEx.ScanNearByWithCone(ownerTransform, setting.detectRange, setting.detectAngle, setting.detectLayer);
        }
        else if(setting.shapeType == Detect_Shape_Type.Box)
        {
            ssss = DetectRaycastEx.ScanNearByWithBox(ownerTransform, setting.offset, setting.rotationOffset, setting.size, setting.detectLayer);
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