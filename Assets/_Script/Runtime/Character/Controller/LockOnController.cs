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

        // ȭ���� ����� ���� ����� ���� Ÿ������ ����
        // ȭ�� �߾ӿ� ���� ����� Ÿ���� ã�� ���� ����
        float closestDistance = float.MaxValue;
        CharacterActorBase closestTarget = null;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

        foreach (var t in targets)
        {
            // Ÿ���� ���� ��ǥ�� ��ũ�� ��ǥ�� ��ȯ
            Vector3 screenPosition = camera.WorldToScreenPoint(t.baseObject.transform.position);
            float distanceFromCenter = Vector3.Distance(screenPosition, screenCenter);

            // ���� ����� Ÿ���� ����
            if (distanceFromCenter < closestDistance)
            {
                closestDistance = distanceFromCenter;
                closestTarget = t;
            }
        }

        // ���� ����� Ÿ���� ��Ŀ�� Ÿ������ ����
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

            // Ÿ���� ��ġ
            var targetPosition = t.baseObject.transform.position;

            // �÷��̾�� Ÿ�� ���� �Ÿ� ���
            float distanceToTarget = Vector3.Distance(playerPosition, targetPosition);

            // Ÿ���� ���� �Ÿ� �̳��� �ִ��� Ȯ��
            if (distanceToTarget <= maxLockOnDistance)
            {
                // ���� ����� Ÿ���� ����
                if (distanceToTarget < closestDistance)
                {
                    closestDistance = distanceToTarget;
                    closestTarget = t;
                }
            }
        }

        // ���� ����� Ÿ���� ��Ŀ�� Ÿ������ ����
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
