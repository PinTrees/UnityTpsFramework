using UnityEngine;

public class ThirdPersonCamera : CameraBase
{
    [Header("Components")]
    public Transform targetTransform;

    [Header("Data")]
    public Vector3 offset;
    public Vector3 cameraLocalOffset;
    public Vector2 clampRotationY = new Vector2(-60, 60);
    public float rotationSpeed = 10f;
    public float followSpeed = 10f;
    public float rotationInputSpeed = 5f;
    public LayerMask collisionLayerMask;

    // Runtime Value
    [Header("Runtime Value")]
    public float currentRotationX = 0f; // X축 회전값 (수평)
    public float currentRotationY = 0f; // Y축 회전값 (수직)


    protected override void OnInit()
    {
        base.OnInit();
    }


    private Vector3 camDir = Vector3.back;
    protected override void OnUpdate()
    {
        var deltaTime = Time.deltaTime;
        if (updateType == CameraUpdateType.Update) deltaTime = Time.unscaledDeltaTime;
        else if (updateType == CameraUpdateType.FixedUpdate) deltaTime = Time.fixedUnscaledDeltaTime;
        else if (updateType == CameraUpdateType.LateUpdate) deltaTime = Time.unscaledDeltaTime;

        // 마우스 입력을 받아 회전값을 계산 (카메라 자체의 회전만 조정)
        currentRotationX += Input.GetAxis("Mouse X") * rotationInputSpeed;
        currentRotationY -= Input.GetAxis("Mouse Y") * rotationInputSpeed;
        currentRotationY = Mathf.Clamp(currentRotationY, clampRotationY.x, clampRotationY.y); // 수직 회전 각도 제한

        // 카메라 방향 업데이트
        camDir = Quaternion.Euler(currentRotationY, currentRotationX, 0) * Vector3.back; // 월드 좌표계 기준으로 회전 적용

        var targetPosition = targetTransform.position + offset;

        // 캐릭터의 월드좌표계 기준 카메라 위치 계산
        Vector3 desiredPosition = targetPosition + camDir * distance;
        desiredPosition += transform.rotation * cameraLocalOffset;

        // 충돌 검사
        RaycastHit hit;
        if (Physics.Raycast(targetPosition, desiredPosition - targetPosition, out hit, distance, collisionLayerMask))
        {
            // 충돌이 감지되면, 충돌 지점보다 약간 앞쪽으로 카메라 위치 조정
            desiredPosition = hit.point - (camDir * 0.1f);
        }

        // 카메라 위치를 부드럽게 타겟 위치로 이동 (Lerp 사용)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * deltaTime);

        // 카메라가 항상 벡터의 반대 방향을 바라보도록 설정
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-camDir, Vector3.up), rotationSpeed * deltaTime);

        // 룩 다이렉션 업데이트
        lookDirection = transform.forward;
    }
}
