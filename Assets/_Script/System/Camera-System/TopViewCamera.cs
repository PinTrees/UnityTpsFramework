using UnityEngine;

public class TopViewCamera : CameraBase
{
    public Transform targetTransform;  // ī�޶� ���� Ÿ��
    public float angle = 45f;          // ī�޶� Ÿ���� �ٶ󺸴� ����
    public float height = 10f;         // ī�޶��� ���� (Ÿ�ٰ��� �Ÿ�)
    public float followSpeed = 5f;     // ī�޶� Ÿ���� ���󰡴� �ӵ�
    public float rotationSpeed = 100f; // ī�޶� ȸ�� �ӵ�

    private Vector3 targetPosition;    // Ÿ���� ��ġ

    protected override void OnInit()
    {
        base.OnInit();
    }

    protected override void OnUpdate()
    {
        if (targetTransform == null) return;

        // Ÿ���� ��ġ���� ���� �Ÿ���ŭ ������ ī�޶� ��ġ ���
        targetPosition = targetTransform.position - (Quaternion.Euler(angle, 0, 0) * Vector3.forward * distance);
        targetPosition.y = targetTransform.position.y + height;

        // ī�޶� �ε巴�� Ÿ�� ��ġ�� �̵�
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // ī�޶� Ÿ���� �ٶ󺸵��� ����
        transform.LookAt(targetTransform);
    }

    // �߰�: ȸ�� ��� (������ �¿�� ȸ�� �����ϵ���)
    public void Rotate(float horizontalInput)
    {
        float rotationAngle = horizontalInput * rotationSpeed * Time.deltaTime;
        transform.RotateAround(targetTransform.position, Vector3.up, rotationAngle);
    }
}