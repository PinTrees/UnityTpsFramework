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
                    totalPosition += target.baseObject.transform.position;  // 타겟 위치 합산
                    validTargetCount++;
                }
            }

            // 유효한 타겟이 있으면 중간 지점과 회전을 계산
            if (validTargetCount > 0)
            {
                Vector3 midPoint = (totalPosition + playerCharacter.baseObject.transform.position) / (validTargetCount + 1);  // 중간 지점 계산
                targetPosition = midPoint;

                // 카메라가 타겟 중간 지점을 바라보도록 회전 설정
                targetRotation = Quaternion.LookRotation(midPoint - transform.position);

                // 모든 타겟과 플레이어 간의 평균 거리에 따라 카메라의 줌아웃 정도 조절
                float averageDistanceToTarget = totalPosition.magnitude / validTargetCount;
                cameraDistance = Mathf.Lerp(cameraDistanceClamp.x, cameraDistanceClamp.y, averageDistanceToTarget / cameraDistanceClamp.y);
            }
        }

        // Lerp를 사용해 부드럽게 위치와 회전값을 변경
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
                totalPosition += target.baseObject.transform.position;  // 타겟 위치 합산
                validTargetCount++;
            }

            // 유효한 타겟이 있으면 중간 지점과 회전을 계산
            if (validTargetCount > 0)
            {
                Vector3 midPoint = (totalPosition + playerCharacter.baseObject.transform.position) / (validTargetCount + 1);  // 중간 지점 계산
                targetPosition = midPoint;

                // 카메라가 타겟 중간 지점을 바라보도록 회전 설정
                targetRotation = Quaternion.LookRotation(midPoint - transform.position);

                // 모든 타겟과 플레이어 간의 평균 거리에 따라 카메라의 줌아웃 정도 조절
                float averageDistanceToTarget = totalPosition.magnitude / validTargetCount;
                cameraDistance = Mathf.Lerp(cameraDistanceClamp.x, cameraDistanceClamp.y, averageDistanceToTarget / cameraDistanceClamp.y);
            }
        }

        // Lerp를 사용해 부드럽게 위치와 회전값을 변경
        transform.position = Vector3.Lerp(currentPos, targetPosition, lerp * Time.unscaledDeltaTime);
        transform.rotation = Quaternion.Slerp(currentRot, targetRotation, lerp * Time.unscaledDeltaTime);
        thirdPersonCamera.distance = Mathf.Lerp(thirdPersonCamera.distance, cameraDistance, lerp * Time.unscaledDeltaTime);
    }
}
