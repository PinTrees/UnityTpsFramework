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

        Gizmos.color = Color.green; // Gizmo ���� ����

        if (detectorSetting.shapeType == Detect_Shape_Type.Speher)
        {
            // ���� ���� �׸���
            //Gizmos.DrawWireSphere(transform.position + areaOffset, radius);
        }
        else if (detectorSetting.shapeType == Detect_Shape_Type.Box)
        {
            // ������ Gizmo ����� ���
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // ���� ȸ�� �������� �ݿ�
            Quaternion localRotationOffset = Quaternion.Euler(detectorSetting.rotationOffset);

            // Gizmo�� ��ȯ ����� ������Ʈ�� ��ġ, ȸ�� �� ���� ȸ�� �������� ������� ����
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation * localRotationOffset, Vector3.one);

            // ���� ��� ȸ���� ����Ͽ� �ڽ� ���� �׸���
            Gizmos.DrawWireCube(detectorSetting.offset, detectorSetting.size);

            // ������ Gizmo ��ķ� ����
            Gizmos.matrix = oldMatrix;
        }
    }
}
