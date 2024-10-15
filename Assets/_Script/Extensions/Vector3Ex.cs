using UnityEngine;

public static class Vector3Ex
{
    public static bool Eqaul_XZ(this Vector3 a, Vector3 b)
    {
        if (a.x != b.x) return false;
        if (a.z != b.z) return false;
        return true;
    }

    public static Vector3 RandomVector(Vector3 min, Vector3 max)
    {
        return new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(max.z, max.z)
            );
    }

    public static Vector3 NormalizeToBoundary(this Vector3 vector)
    {
        // 8���� ���� �̸� ����
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,                    // (0, 0, 1)
            Vector3.back,                       // (0, 0, -1)
            Vector3.right,                      // (1, 0, 0)
            Vector3.left,                       // (-1, 0, 0)
            (Vector3.forward + Vector3.right).normalized,    // (1, 0, 1)
            (Vector3.forward + Vector3.left).normalized,     // (-1, 0, 1)
            (Vector3.back + Vector3.right).normalized,       // (1, 0, -1)
            (Vector3.back + Vector3.left).normalized         // (-1, 0, -1)
        };

        // ���� ����� ������ ã�� ���� ����
        Vector3 closestDirection = Vector3.zero;
        float maxDot = -Mathf.Infinity;

        // �Է� ���͸� ����ȭ
        Vector3 normalizedInput = vector.normalized;

        // ���� ����� ���� ã��
        foreach (var dir in directions)
        {
            float dotProduct = Vector3.Dot(normalizedInput, dir);

            if (dotProduct > maxDot)
            {
                maxDot = dotProduct;
                closestDirection = dir;
            }
        }

        return closestDirection;
    }

    /// <summary>
    /// �� ������ �̷���� �� ���� ������ ������ ���ϴ� Ȯ�� �Լ� (������ ��׸� ����).
    /// </summary>
    /// <param name="center">�߽��� (������)</param>
    /// <param name="from">������</param>
    /// <param name="to">��ǥ��</param>
    /// <returns>�� ������ ������ �� ���� ������ ���� (degree)</returns>
    public static float AngleBetweenPoints(Vector3 center, Vector3 from, Vector3 to)
    {
        // �߽������� �������� ��ǥ������ ���ϴ� ���� ��� (Y�� ȸ���� ����ϹǷ� Y ���� ����)
        Vector3 fromVector = new Vector3(from.x - center.x, 0, from.z - center.z);
        Vector3 toVector = new Vector3(to.x - center.x, 0, to.z - center.z);

        // �� ���͸� ����ȭ (normalized)�Ͽ� ���⸸ ����
        Vector3 fromNorm = fromVector.normalized;
        Vector3 toNorm = toVector.normalized;

        // ���� ���
        float dotProduct = Vector3.Dot(fromNorm, toNorm);

        // ���� ���� ���� ���� ��� (���� -> ��׸��� ��ȯ)
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        // ������ ����� ���� Ȯ�� (�ð�/�ݽð� ���� �Ǻ�)
        Vector3 crossProduct = Vector3.Cross(fromNorm, toNorm);

        // Y�� ������ ��������, ������ Y���� �����̸� �ݽð� ����, ����̸� �ð� ����
        if (crossProduct.y < 0)
        {
            angle = -angle;
        }

        return angle;
    }

    public static Vector3 Rotate(this Vector3 vector, float degree, Vector3 axis)
    {
        // ���� ����ȭ (Normalized)�մϴ�.
        Vector3 normalizedAxis = axis.normalized;

        // �־��� �࿡ ���� ȸ���� ǥ���ϴ� ���ʹϾ��� �����մϴ�.
        Quaternion rotation = Quaternion.AngleAxis(degree, normalizedAxis);

        // ���͸� ȸ���մϴ�.
        Vector3 rotatedVector = rotation * vector;

        return rotatedVector;
    }
}
