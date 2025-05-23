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
    private const int AROUND_POSITION_LAYER_GAP = 3;                 // 해당 갭은 같은 레이어로 처리

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

    public CharacterActorBase forcusTarget;                         // 현재 포커싱 타겟
    public List<CharacterActorBase> targets = new();                // 탐지된 모든 타겟   
    public List<CharacterActorBase> forcusedByCharacters = new();   // 현재 나를 포커싱 중인 적
    public List<CharacterActorBase> hitTargets = new();             // 현재 내가 타격중인 캐릭터

    public List<CharacterActorBase> activeAttackers = new();                // 현재 나를 공격중인 캐릭터
    public List<CharacterActorBase> canAttackableWaitingActors = new();     // 현재 나를 공격 대기중인 캐릭터

    // 나를 포커싱 중인 적의 대치거리 레이어에 따른 대상 주변 위치 포인트
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
        // 이 캐릭터를 공격이 가능한지에 대한 정보 확인
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

                // 타겟의 태그 중 오너의 태그와 일치하는 것이 있는지 확인
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

        // 현재 적의 대치거리 레이어 가져오기
        var circleAroundLayer = forcusedByCharacterWithLayerCache[layerIndex];
        if (!circleAroundLayer.Contains(target))
            circleAroundLayer.Add(target);

        // 같은 대치 레이어에서 대상을 바라보고있는 적의 수
        var currentForucedCharacterCount = circleAroundLayer.Count;

        // 현재 대치레이어의 대치 위치값 업데이트
        UpdateForcusedCharacterAroundPositionLayer(layerIndex, confrontingRange, currentForucedCharacterCount);

        // 현재 캐릭터의 고유 순번에 대한 고유자리 획득
        var targetIndex = circleAroundLayer.IndexOf(target);
        var targetPosition = forcuedByCharacterAroundPositionLayer[layerIndex][targetIndex];

        // 반환
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
            // 각도를 라디안으로 변환
            float angleInRadians = Mathf.Deg2Rad * (i * angleStep);

            // 오너의 y축 회전을 무시하고 좌표 계산 (월드 좌표계 기준)
            float x = ownerPosition.x + Mathf.Cos(angleInRadians) * distance;
            float z = ownerPosition.z + Mathf.Sin(angleInRadians) * distance;

            // 타겟 위치 추가 (y 값은 동일한 높이로 설정)
            var targetWorldPosition = new Vector3(x, ownerPosition.y, z);
            var offsetPosition = targetWorldPosition - ownerPosition;
            forcuedByCharacterAroundPositionLayer[layerIndex].Add(offsetPosition);
        }
    }


    // 현재 나를 추적중인 캐릭터 추가
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

        // 접근자 생성
        var combatData = character.characterData.combatData;
        var confrontingRange = combatData.confrontingRange;
        var layerIndex = (int)((int)confrontingRange / AROUND_POSITION_LAYER_GAP);

        // 존재 확인
        if (!forcusedByCharacterWithLayerCache.ContainsKey(layerIndex))
            forcusedByCharacterWithLayerCache[layerIndex] = new();

        // 레이어 대치거리 캐싱목록 제거
        var index = forcusedByCharacterWithLayerCache[layerIndex].IndexOf(character);
        forcusedByCharacterWithLayerCache[layerIndex].Remove(character);
        forcuedByCharacterAroundPositionLayer[layerIndex].RemoveAt(index);

        // 포지션 재계산
        ForcusedTargetReposition();
    }

    // 나를 공격가능한 적 캐릭터 제어
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

        // 현재 적의 대치거리 레이어 가져오기
        var circleAroundLayer = forcusedByCharacterWithLayerCache[layerIndex];
        if (!circleAroundLayer.Contains(character))
            circleAroundLayer.Add(character);

        // 현재 캐릭터의 고유 순번에 대한 고유자리 획득
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
