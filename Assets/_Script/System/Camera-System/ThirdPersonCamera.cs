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
    public float currentRotationX = 0f; // X�� ȸ���� (����)
    public float currentRotationY = 0f; // Y�� ȸ���� (����)


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

        // ���콺 �Է��� �޾� ȸ������ ��� (ī�޶� ��ü�� ȸ���� ����)
        currentRotationX += Input.GetAxis("Mouse X") * rotationInputSpeed;
        currentRotationY -= Input.GetAxis("Mouse Y") * rotationInputSpeed;
        currentRotationY = Mathf.Clamp(currentRotationY, clampRotationY.x, clampRotationY.y); // ���� ȸ�� ���� ����

        // ī�޶� ���� ������Ʈ
        camDir = Quaternion.Euler(currentRotationY, currentRotationX, 0) * Vector3.back; // ���� ��ǥ�� �������� ȸ�� ����

        var targetPosition = targetTransform.position + offset;

        // ĳ������ ������ǥ�� ���� ī�޶� ��ġ ���
        Vector3 desiredPosition = targetPosition + camDir * distance;
        desiredPosition += transform.rotation * cameraLocalOffset;

        // �浹 �˻�
        RaycastHit hit;
        if (Physics.Raycast(targetPosition, desiredPosition - targetPosition, out hit, distance, collisionLayerMask))
        {
            // �浹�� �����Ǹ�, �浹 �������� �ణ �������� ī�޶� ��ġ ����
            desiredPosition = hit.point - (camDir * 0.1f);
        }

        // ī�޶� ��ġ�� �ε巴�� Ÿ�� ��ġ�� �̵� (Lerp ���)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * deltaTime);

        // ī�޶� �׻� ������ �ݴ� ������ �ٶ󺸵��� ����
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-camDir, Vector3.up), rotationSpeed * deltaTime);

        // �� ���̷��� ������Ʈ
        lookDirection = transform.forward;
    }
}
