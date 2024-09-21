using UnityEngine;

/// <summary>
/// SoftLockOn Target Logic
/// </summary>
public enum SoftLockOnType
{
    None,
    ClosestCameraCenter,
    ClosestPlayerDistance,
}

public class LockOnController : MonoBehaviour
{
    [Header("Soft LockOn Setting")]
    public SoftLockOnType softLockOnType; 

    [Header("Runtime Value")]
    public PlayerCharacterActorBase playerCharacter;
    public CharacterActorBase lockOnTarget;

    [Header("Runtime Status Value")]
    public bool IsSoftLockOn = false;
    private UI_SoftLockOn_Indicator softLockOnIndicator;


    public void Init(PlayerCharacterActorBase playerCharacter)
    {
        this.playerCharacter = playerCharacter;
        softLockOnIndicator = UISystemManager.Instance.Create<UI_SoftLockOn_Indicator>("Soft LockOn Indicator");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            IsSoftLockOn = !IsSoftLockOn;

            if(IsSoftLockOn)
            {
                playerCharacter.targetController.lockDetectUpdate = true;
                lockOnTarget = playerCharacter.targetController.forcusTarget;
                if(lockOnTarget)
                {
                    softLockOnIndicator.Show(lockOnTarget);
                }
            }
            else
            {
                playerCharacter.targetController.lockDetectUpdate = false;
                softLockOnIndicator.Close();
            }
        }
    }

    public void LateUpdate()
    {
        if(IsSoftLockOn)
        {
            if (softLockOnType == SoftLockOnType.None)
                UpdateSoftLockOn_CameraCenter();
            else if (softLockOnType == SoftLockOnType.ClosestCameraCenter)
                UpdateSoftLockOn_CameraCenter();
            else if (softLockOnType == SoftLockOnType.ClosestPlayerDistance)
                UpdateSoftLockOn_PlayerDistance();
        }
    }

    private void UpdateSoftLockOn_CameraCenter()
    {
        var camera = Camera.main;
        var targets = playerCharacter.targetController.targets;

        // 화면의 가운데에 가장 가까운 적을 타겟으로 잡음
        // 화면 중앙에 가장 가까운 타겟을 찾기 위한 변수
        float closestDistance = float.MaxValue;
        CharacterActorBase closestTarget = null;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        foreach (var t in targets)
        {
            // 타겟의 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPosition = camera.WorldToScreenPoint(t.baseObject.transform.position);
            float distanceFromCenter = Vector3.Distance(screenPosition, screenCenter);

            // 가장 가까운 타겟을 저장
            if (distanceFromCenter < closestDistance)
            {
                closestDistance = distanceFromCenter;
                closestTarget = t;
            }
        }

        // 가장 가까운 타겟을 포커스 타겟으로 설정
        if (closestTarget != null)
        {
            playerCharacter.targetController.forcusTarget = closestTarget;

            if (lockOnTarget != closestTarget)
            {
                softLockOnIndicator.Close();
                lockOnTarget = playerCharacter.targetController.forcusTarget;
                softLockOnIndicator.Show(lockOnTarget);
            }
        }
    }
    private void UpdateSoftLockOn_PlayerDistance()
    {
        var playerPosition = playerCharacter.baseObject.transform.position;
        var targets = playerCharacter.targetController.targets;

        float maxLockOnDistance = 10.0f;

        float closestDistance = float.MaxValue;
        CharacterActorBase closestTarget = null;

        foreach (var t in targets)
        {
            if (t.IsDeath)
                continue;

            // 타겟의 위치
            var targetPosition = t.baseObject.transform.position;

            // 플레이어와 타겟 간의 거리 계산
            float distanceToTarget = Vector3.Distance(playerPosition, targetPosition);

            // 타겟이 기준 거리 이내에 있는지 확인
            if (distanceToTarget <= maxLockOnDistance)
            {
                // 가장 가까운 타겟을 선택
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    closestTarget = t;
                }
            }
        }

        // 가장 가까운 타겟을 포커스 타겟으로 설정
        if (closestTarget != null)
        {
            playerCharacter.targetController.forcusTarget = closestTarget;

            if (lockOnTarget != closestTarget)
            {
                softLockOnIndicator.Close();
                lockOnTarget = playerCharacter.targetController.forcusTarget;
                softLockOnIndicator.Show(lockOnTarget);
            }
        }
        else
        {
            lockOnTarget = null;
            playerCharacter.targetController.forcusTarget = null;
            softLockOnIndicator.Close();
        }
    }
}
