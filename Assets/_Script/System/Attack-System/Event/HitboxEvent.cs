using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HitboxActionType
{
    Single,
    Repeat,
}

public enum HitboxDamageType
{
    Single,
    Multiple,
    Count,
}

public enum HitDirectionType
{
    None,
    Left,
    Right,
    Up,
    Down,
    Back,
}

[CreateAssetMenu(menuName = "Scriptable/Attack/Event/Hitbox")]
public class HitboxEvent : AttackEvent
{
    [Header("Detector Setting")]
    public bool useBaseTransform;
    public DetectorSetting detectorSetting;

    [Header("Type Setting")]
    public HitboxDamageType damageType;
    public HitboxActionType actionType;
    public HitDirectionType hitDirectionType;

    [Header("Animation Setting")]
    public bool useCustomHitAnimation;
    public AnimationClip hitAnimationClip;
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData hitAnimationPlayNormalizeTime;
    public float hitAnimationSpeed = 1;

    [Header("Runtime Value")]
    public List<Transform> targets = new();
    public bool isHit;


    public override void Enter()
    {
        base.Enter();
        targets.Clear();
        isHit = false;
    }

    public void FindHit(CharacterActorBase owner, HitboxDataTree root, List<CharacterActorBase> hitTargets)
    {
        hitTargets.Clear();

        if (isHit)
            return;

        Transform rootTransform = null;
        if (useBaseTransform) rootTransform = owner.baseObject.transform;
        else rootTransform = owner.weaponController.GetEquipWeapon().hitboxTransform;

        detectorSetting.detectLayer = root.targetLayerMask;
        var target = DetecteSystem.FindTargetUpdate(rootTransform, targets, detectorSetting);

        if (targets.Count > 0)
        {
            isHit = true;

            if (damageType == HitboxDamageType.Single)
            {
                var targetTransform = targets.Where(e => e.GetComponentInParent<CharacterActorBase>()).FirstOrDefault();
                hitTargets.Add(targetTransform.GetComponentInParent<CharacterActorBase>());
            }
            else if (damageType == HitboxDamageType.Multiple)
            {
                targets.Where(e => e.GetComponentInParent<CharacterActorBase>()).ToList().ForEach(e =>
                {
                    hitTargets.Add(e.GetComponentInParent<CharacterActorBase>());
                });
            }
        }

        return;
    }


    public override void OnDrawGizmo(Transform transform)
    {
        base.OnDrawGizmo(transform);

        Gizmos.color = Color.green; // Gizmo 색상 설정

        if (detectorSetting.shapeType == Detect_Shape_Type.Speher)
        {
            // 원형 영역 그리기
            //Gizmos.DrawWireSphere(transform.position + areaOffset, radius);
        }
        else if (detectorSetting.shapeType == Detect_Shape_Type.Box)
        {
            // 기존의 Gizmo 행렬을 백업
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // 로컬 회전 오프셋을 반영
            Quaternion localRotationOffset = Quaternion.Euler(detectorSetting.rotationOffset);

            // Gizmo의 변환 행렬을 오브젝트의 위치, 회전 및 로컬 회전 오프셋을 기반으로 설정
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation * localRotationOffset, Vector3.one);

            // 로컬 축과 회전을 고려하여 박스 영역 그리기
            Gizmos.DrawWireCube(detectorSetting.offset, detectorSetting.size);

            // 원래의 Gizmo 행렬로 복원
            Gizmos.matrix = oldMatrix;
        }
    }
}
