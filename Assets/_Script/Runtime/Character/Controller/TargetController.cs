using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class TargetController : MonoBehaviour
{
    // Const Value Setting
    private const int AROUND_POSITION_LAYER_GAP = 3;                 // �ش� ���� ���� ���̾�� ó��

    [Header("Data")]
    public CharacterActorBase ownerCharacter;
    public LayerMask targetLayerMask;
    public float detectDelay = 1f;

    [Header("Hear Data")]
    public float canHearRange;
    public float canHearAngle;

    [Header("See Data")]
    public float canSeeRange;
    public float canSeeAngle;

    [Header("Runtime Value")]
    private Coroutine targetDetectCoroutine = null;

    public CharacterActorBase forcusTarget;                         // ���� ��Ŀ�� Ÿ��
    public List<CharacterActorBase> targets = new();                // Ž���� ��� Ÿ��   
    public List<CharacterActorBase> forcusedByCharacters = new();   // ���� ���� ��Ŀ�� ���� ��
    public List<CharacterActorBase> hitTargets = new();             // ���� ���� Ÿ������ ĳ����

    public List<CharacterActorBase> activeAttackers = new();                // ���� ���� �������� ĳ����
    public List<CharacterActorBase> canAttackableWaitingActors = new();     // ���� ���� ���� ������� ĳ����

    // ���� ��Ŀ�� ���� ���� ��ġ�Ÿ� ���̾ ���� ��� �ֺ� ��ġ ����Ʈ
    private Dictionary<int, List<CharacterActorBase>> forcusedByCharacterWithLayerCache = new();
    private Dictionary<int, List<Vector3>> forcuedByCharacterAroundPositionLayer = new();

    public bool lockDetectUpdate;


    
    public void Init(CharacterActorBase owner)
    {
        this.ownerCharacter = owner;
        targetDetectCoroutine = StartCoroutine(UpdateTargetDetectTask());
    }
    public void Exit()
    {
        if(forcusTarget)
        {
            forcusTarget.targetController.RemoveForcusedTarget(ownerCharacter);
            forcusTarget.targetController.canAttackableWaitingActors.Remove(ownerCharacter);
        }

        if (targetDetectCoroutine != null) StopCoroutine(targetDetectCoroutine);
    }

    protected void Update()
    {
    }

    protected void LateUpdate()
    {
        // �� ĳ���͸� ������ ���������� ���� ���� Ȯ��
        if(activeAttackers.Count <= 0 && canAttackableWaitingActors.Count > 0)
        {
            var attacker = canAttackableWaitingActors.PopRandomElement();
            if(attacker)
            {
                activeAttackers.Add(attacker);
                attacker.IsReadyToAttack = true;
                attacker.targetController.lockDetectUpdate = true;
                attacker.transform.DOMove(attacker.transform.position, 1.5f).OnComplete(() =>
                {
                    attacker.OnRunToAttack();
                });
            }
        }
    }

    IEnumerator UpdateTargetDetectTask()
    {
        List<Transform> tmpTargets = ListPool<Transform>.Get();

        while (true)
        {
            if (lockDetectUpdate)
            {
                yield return new WaitForSeconds(0.35f);
                continue;
            }
            if(ownerCharacter.IsDeath)
            {
                break;
            }

            // See Targets
            var targetSee = DetecteSystem.FindTargetUpdate(ownerCharacter.baseObject.transform, tmpTargets, new DetectorSetting()
            {
                detectRange = canSeeRange,
                detectAngle = canSeeAngle,
                detectLayer = targetLayerMask,
                shapeType = Detect_Shape_Type.Speher,
            });

            targets.Clear();
            for(int i = 0; i < tmpTargets.Count; ++i)
            {
                var tmpTarget = tmpTargets[i].GetComponentInParent<CharacterActorBase>();
                if (tmpTarget == null)
                    continue;
                if (tmpTarget.IsDeath)
                    continue;

                // Ÿ���� �±� �� ������ �±׿� ��ġ�ϴ� ���� �ִ��� Ȯ��
                if (ownerCharacter.targetTags.Any(ownerTag => tmpTarget.characterTags.Any(targetTag => targetTag.tag == ownerTag.tag)))
                {
                    targets.Add(tmpTarget);
                }
            }

            CharacterActorBase prevForcusedTarget = forcusTarget;

            if (targets.Count > 0)
            {
                var currentForcusTarget = targets.OrderBy(t => Vector3.Distance(t.baseObject.transform.position, ownerCharacter.baseObject.transform.position)).First();
                forcusTarget = currentForcusTarget;
            }
            else
            {
                forcusTarget = null;
            }

            if (forcusTarget != null && forcusTarget.targetController != null)
            {
                if (!forcusTarget.targetController.HasForcusedTarget(ownerCharacter))
                    forcusTarget.targetController.AddForcusedTarget(ownerCharacter);
            }
            if(prevForcusedTarget != forcusTarget && prevForcusedTarget != null && prevForcusedTarget.targetController != null)
            {
                if (prevForcusedTarget.targetController.HasForcusedTarget(ownerCharacter))
                    prevForcusedTarget.targetController.RemoveForcusedTarget(ownerCharacter);
            }

            yield return new WaitForSeconds(detectDelay);
        }

        ListPool<Transform>.Release(tmpTargets);
    }

    // Update Forcued Target AroundPosition
    public void UpdateForcusedCharacterAroundPosition(CharacterActorBase target)
    {
        if (target.characterData == null)
        {
            //Debug.LogError($"Null ForcuedTargetAroundPosition Error - {target.name} CharacterData");
            return;
        }

        var combatData = target.characterData.combatData;
        var confrontingRange = combatData.confrontingRange;
        var layerIndex = (int)((int)confrontingRange / AROUND_POSITION_LAYER_GAP);

        if (!forcusedByCharacterWithLayerCache.ContainsKey(layerIndex))
            forcusedByCharacterWithLayerCache[layerIndex] = new();

        // ���� ���� ��ġ�Ÿ� ���̾� ��������
        var circleAroundLayer = forcusedByCharacterWithLayerCache[layerIndex];
        if (!circleAroundLayer.Contains(target))
            circleAroundLayer.Add(target);

        // ���� ��ġ ���̾�� ����� �ٶ󺸰��ִ� ���� ��
        var currentForucedCharacterCount = circleAroundLayer.Count;

        // ���� ��ġ���̾��� ��ġ ��ġ�� ������Ʈ
        UpdateForcusedCharacterAroundPositionLayer(layerIndex, confrontingRange, currentForucedCharacterCount);

        // ���� ĳ������ ���� ������ ���� �����ڸ� ȹ��
        var targetIndex = circleAroundLayer.IndexOf(target);
        var targetPosition = forcuedByCharacterAroundPositionLayer[layerIndex][targetIndex];

        // ��ȯ
        return;
    }

    private void UpdateForcusedCharacterAroundPositionLayer(int layerIndex, float distance, int targetCount)
    {
        if (!forcuedByCharacterAroundPositionLayer.ContainsKey(layerIndex))
            forcuedByCharacterAroundPositionLayer[layerIndex] = new();

        forcuedByCharacterAroundPositionLayer[layerIndex].Clear();
        Vector3 ownerPosition = ownerCharacter.baseObject.transform.position;

        float angleStep = 360f / targetCount;

        for (int i = 0; i < targetCount; i++)
        {
            // ������ �������� ��ȯ
            float angleInRadians = Mathf.Deg2Rad * (i * angleStep);

            // ������ y�� ȸ���� �����ϰ� ��ǥ ��� (���� ��ǥ�� ����)
            float x = ownerPosition.x + Mathf.Cos(angleInRadians) * distance;
            float z = ownerPosition.z + Mathf.Sin(angleInRadians) * distance;

            // Ÿ�� ��ġ �߰� (y ���� ������ ���̷� ����)
            var targetWorldPosition = new Vector3(x, ownerPosition.y, z);
            var offsetPosition = targetWorldPosition - ownerPosition;
            forcuedByCharacterAroundPositionLayer[layerIndex].Add(offsetPosition);
        }
    }


    // ���� ���� �������� ĳ���� �߰�
    private bool HasForcusedTarget(CharacterActorBase character)
    {
        return forcusedByCharacters.Contains(character);
    }
    private void AddForcusedTarget(CharacterActorBase character)
    {
        forcusedByCharacters.Add(character);
        ForcusedTargetReposition();
    }
    private void RemoveForcusedTarget(CharacterActorBase character)
    {
        forcusedByCharacters.Remove(character);

        if (character.characterData == null)
            return;

        // ������ ����
        var combatData = character.characterData.combatData;
        var confrontingRange = combatData.confrontingRange;
        var layerIndex = (int)((int)confrontingRange / AROUND_POSITION_LAYER_GAP);

        // ���� Ȯ��
        if (!forcusedByCharacterWithLayerCache.ContainsKey(layerIndex))
            forcusedByCharacterWithLayerCache[layerIndex] = new();

        // ���̾� ��ġ�Ÿ� ĳ�̸�� ����
        var index = forcusedByCharacterWithLayerCache[layerIndex].IndexOf(character);
        forcusedByCharacterWithLayerCache[layerIndex].Remove(character);
        forcuedByCharacterAroundPositionLayer[layerIndex].RemoveAt(index);

        // ������ ����
        ForcusedTargetReposition();
    }

    // ���� ���ݰ����� �� ĳ���� ����
    public void AddAttackableWaitingActor(CharacterActorBase actor)
    {
        if (canAttackableWaitingActors.Contains(actor))
            return;
        canAttackableWaitingActors.Add(actor);
    }

    public void ForcusedTargetReposition()
    {
        foreach (var character in forcusedByCharacters)
        {
            UpdateForcusedCharacterAroundPosition(character);
        }
    }

    public Vector3 GetForcuedTargetAroundPosition(CharacterActorBase character)
    {
        if (character.characterData == null)
            return Vector3.zero;

        var combatData = character.characterData.combatData;
        var confrontingRange = combatData.confrontingRange;
        var layerIndex = (int)((int)confrontingRange / AROUND_POSITION_LAYER_GAP);

        if (!forcusedByCharacterWithLayerCache.ContainsKey(layerIndex))
            forcusedByCharacterWithLayerCache[layerIndex] = new();

        // ���� ���� ��ġ�Ÿ� ���̾� ��������
        var circleAroundLayer = forcusedByCharacterWithLayerCache[layerIndex];
        if (!circleAroundLayer.Contains(character))
            circleAroundLayer.Add(character);

        // ���� ĳ������ ���� ������ ���� �����ڸ� ȹ��
        var targetIndex = circleAroundLayer.IndexOf(character);
        var targetPosition = forcuedByCharacterAroundPositionLayer[layerIndex][targetIndex];

        return targetPosition + ownerCharacter.baseObject.transform.position;
    }

#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        if (ownerCharacter == null)
            return;

        Gizmos.color = new Color(0, 1, 1, 1);
        GizmosEx.DrawWireArc(ownerCharacter.baseObject.transform.position,
            ownerCharacter.baseObject.transform.forward, canSeeAngle, canSeeRange, thick: 4);

        Gizmos.color = new Color(0, 1, 0, 1);
        GizmosEx.DrawWireArc(ownerCharacter.baseObject.transform.position,
            ownerCharacter.baseObject.transform.forward, canHearAngle, canHearRange, thick: 4);

        foreach(var layer in forcuedByCharacterAroundPositionLayer)
        {
            layer.Value.ForEach(e =>
            {
                Gizmos.color = new Color(1, 0, 0, 1);
                Gizmos.DrawSphere(e + ownerCharacter.baseObject.transform.position, 0.1f);
            });
        }
    }
#endif
}
