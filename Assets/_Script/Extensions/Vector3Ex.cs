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
        // 8방향 벡터 미리 정의
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

        // 가장 가까운 방향을 찾기 위한 변수
        Vector3 closestDirection = Vector3.zero;
        float maxDot = -Mathf.Infinity;

        // 입력 벡터를 정규화
        Vector3 normalizedInput = vector.normalized;

        // 가장 가까운 방향 찾기
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
    /// 세 점으로 이루어진 두 벡터 사이의 각도를 구하는 확장 함수 (각도는 디그리 단위).
    /// </summary>
    /// <param name="center">중심점 (기준점)</param>
    /// <param name="from">시작점</param>
    /// <param name="to">목표점</param>
    /// <returns>세 점으로 구성된 두 벡터 사이의 각도 (degree)</returns>
    public static float AngleBetweenPoints(Vector3 center, Vector3 from, Vector3 to)
    {
        // 중심점에서 시작점과 목표점으로 향하는 벡터 계산 (Y축 회전만 고려하므로 Y 값은 무시)
        Vector3 fromVector = new Vector3(from.x - center.x, 0, from.z - center.z);
        Vector3 toVector = new Vector3(to.x - center.x, 0, to.z - center.z);

        // 두 벡터를 정규화 (normalized)하여 방향만 유지
        Vector3 fromNorm = fromVector.normalized;
        Vector3 toNorm = toVector.normalized;

        // 내적 계산
        float dotProduct = Vector3.Dot(fromNorm, toNorm);

        // 내적 값을 통한 각도 계산 (라디안 -> 디그리로 변환)
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        // 외적을 사용해 방향 확인 (시계/반시계 방향 판별)
        Vector3 crossProduct = Vector3.Cross(fromNorm, toNorm);

        // Y축 방향을 기준으로, 외적의 Y값이 음수이면 반시계 방향, 양수이면 시계 방향
        if (crossProduct.y < 0)
        {
            angle = -angle;
        }

        return angle;
    }

    public static Vector3 Rotate(this Vector3 vector, float degree, Vector3 axis)
    {
        // 축을 정규화 (Normalized)합니다.
        Vector3 normalizedAxis = axis.normalized;

        // 주어진 축에 대한 회전을 표현하는 쿼터니언을 생성합니다.
        Quaternion rotation = Quaternion.AngleAxis(degree, normalizedAxis);

        // 벡터를 회전합니다.
        Vector3 rotatedVector = rotation * vector;

        return rotatedVector;
    }
}
