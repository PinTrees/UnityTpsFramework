using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum PlayerMainCameraTargetControllType
{
    None,
    AllTarget,
    TwoTarget,
}

public class PlayerMainCameraTargetController : MonoBehaviour
{
    public PlayerMainCameraTargetControllType targetControllType;

    public ThirdPersonCamera thirdPersonCamera;
    public PlayerCharacterActorBase playerCharacter;
    public float lerp = 5f;
    public float lostTargetDistance = 5;
    public Vector2 cameraDistanceClamp = new Vector2(3, 10);

    void Start()
    {
        
    }

    void Update()
    {
        if (targetControllType == PlayerMainCameraTargetControllType.None)
            AllTarget();
        else if (targetControllType == PlayerMainCameraTargetControllType.AllTarget)
            AllTarget();
        else if (targetControllType == PlayerMainCameraTargetControllType.TwoTarget)
            TwoTarget();
    }

    public void AllTarget()
    {
        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        Vector3 targetPosition = playerCharacter.baseObject.transform.position;
        Quaternion targetRotation = playerCharacter.baseObject.transform.rotation;

        float cameraDistance = 3.5f;

        if (playerCharacter.targetController.forcusTarget)
        {
            Vector3 totalPosition = Vector3.zero;
            int validTargetCount = 0;

            foreach (var target in playerCharacter.targetController.targets)
            {
                float distanceToTarget = Vector3.Distance(playerCharacter.baseObject.transform.position, target.baseObject.transform.position);

                if (distanceToTarget < lostTargetDistance)
                {
                    totalPosition += target.baseObject.transform.position;  // Ÿ�� ��ġ �ջ�
                    validTargetCount++;
                }
            }

            // ��ȿ�� Ÿ���� ������ �߰� ������ ȸ���� ���
            if (validTargetCount > 0)
            {
                Vector3 midPoint = (totalPosition + playerCharacter.baseObject.transform.position) / (validTargetCount + 1);  // �߰� ���� ���
                targetPosition = midPoint;

                // ī�޶� Ÿ�� �߰� ������ �ٶ󺸵��� ȸ�� ����
                targetRotation = Quaternion.LookRotation(midPoint - transform.position);

                // ��� Ÿ�ٰ� �÷��̾� ���� ��� �Ÿ��� ���� ī�޶��� �ܾƿ� ���� ����
                float averageDistanceToTarget = totalPosition.magnitude / validTargetCount;
                cameraDistance = Mathf.Lerp(cameraDistanceClamp.x, cameraDistanceClamp.y, averageDistanceToTarget / cameraDistanceClamp.y);
            }
        }

        // Lerp�� ����� �ε巴�� ��ġ�� ȸ������ ����
        transform.position = Vector3.Lerp(currentPos, targetPosition, lerp * Time.unscaledDeltaTime);
        transform.rotation = Quaternion.Slerp(currentRot, targetRotation, lerp * Time.unscaledDeltaTime);
        thirdPersonCamera.distance = Mathf.Lerp(thirdPersonCamera.distance, cameraDistance, lerp * Time.unscaledDeltaTime);
    }

    public void TwoTarget()
    {
        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;

        Vector3 targetPosition = playerCharacter.baseObject.transform.position;
        Quaternion targetRotation = playerCharacter.baseObject.transform.rotation;

        float cameraDistance = 3.5f;

        if (playerCharacter.targetController.forcusTarget)
        {
            var target = playerCharacter.targetController.forcusTarget;
            Vector3 totalPosition = Vector3.zero;
            int validTargetCount = 0;

            float distanceToTarget = Vector3.Distance(playerCharacter.baseObject.transform.position, target.baseObject.transform.position);
            if (distanceToTarget < lostTargetDistance)
            {
                totalPosition += target.baseObject.transform.position;  // Ÿ�� ��ġ �ջ�
                validTargetCount++;
            }

            // ��ȿ�� Ÿ���� ������ �߰� ������ ȸ���� ���
            if (validTargetCount > 0)
            {
                Vector3 midPoint = (totalPosition + playerCharacter.baseObject.transform.position) / (validTargetCount + 1);  // �߰� ���� ���
                targetPosition = midPoint;

                // ī�޶� Ÿ�� �߰� ������ �ٶ󺸵��� ȸ�� ����
                targetRotation = Quaternion.LookRotation(midPoint - transform.position);

                // ��� Ÿ�ٰ� �÷��̾� ���� ��� �Ÿ��� ���� ī�޶��� �ܾƿ� ���� ����
                float averageDistanceToTarget = totalPosition.magnitude / validTargetCount;
                cameraDistance = Mathf.Lerp(cameraDistanceClamp.x, cameraDistanceClamp.y, averageDistanceToTarget / cameraDistanceClamp.y);
            }
        }

        // Lerp�� ����� �ε巴�� ��ġ�� ȸ������ ����
        transform.position = Vector3.Lerp(currentPos, targetPosition, lerp * Time.unscaledDeltaTime);
        transform.rotation = Quaternion.Slerp(currentRot, targetRotation, lerp * Time.unscaledDeltaTime);
        thirdPersonCamera.distance = Mathf.Lerp(thirdPersonCamera.distance, cameraDistance, lerp * Time.unscaledDeltaTime);
    }
}
