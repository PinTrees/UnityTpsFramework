using UnityEngine;

public class TopViewCamera : CameraBase
{
    public Transform targetTransform;  // 카메라가 따라갈 타겟
    public float angle = 45f;          // 카메라가 타겟을 바라보는 각도
    public float height = 10f;         // 카메라의 높이 (타겟과의 거리)
    public float followSpeed = 5f;     // 카메라가 타겟을 따라가는 속도
    public float rotationSpeed = 100f; // 카메라 회전 속도

    private Vector3 targetPosition;    // 타겟의 위치

    protected override void OnInit()
    {
        base.OnInit();
    }

    protected override void OnUpdate()
    {
        if (targetTransform == null) return;

        // 타겟의 위치에서 일정 거리만큼 떨어진 카메라 위치 계산
        targetPosition = targetTransform.position - (Quaternion.Euler(angle, 0, 0) * Vector3.forward * distance);
        targetPosition.y = targetTransform.position.y + height;

        // 카메라를 부드럽게 타겟 위치로 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // 카메라가 타겟을 바라보도록 설정
        transform.LookAt(targetTransform);
    }

    // 추가: 회전 기능 (유저가 좌우로 회전 가능하도록)
    public void Rotate(float horizontalInput)
    {
        float rotationAngle = horizontalInput * rotationSpeed * Time.deltaTime;
        transform.RotateAround(targetTransform.position, Vector3.up, rotationAngle);
    }
}