using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public static class GizmosEx
{
    /// <summary>
    /// Draws a wire arc.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="dir">The direction from which the anglesRange is taken into account</param>
    /// <param name="angles">The angle range, in degrees.</param>
    /// <param name="range"></param>
    /// <param name="maxSteps">How many steps to use to draw the arc.</param>
    public static void DrawWireArc(Vector3 position, Vector3 dir, float angles, float range, float maxSteps = 20, float thick=5)
    {
        var srcAngles = GetAnglesFromDir(position, dir);
        var initialPos = position;
        var posA = initialPos;
        var stepAngles = angles / maxSteps;
        var angle = srcAngles - angles / 2;
        for (var i = 0; i <= maxSteps; i++)
        {
            var rad = Mathf.Deg2Rad * angle;
            var posB = initialPos;
            posB += new Vector3(range * Mathf.Cos(rad), 0, range * Mathf.Sin(rad));

            GizmosEx.DrawLineThick(posA, posB, thick);

            angle += stepAngles;
            posA = posB;
        }
        GizmosEx.DrawLineThick(posA, initialPos, thick);
    }



    static float GetAnglesFromDir(Vector3 position, Vector3 dir)
    {
        var forwardLimitPos = position + dir;
        var srcAngles = Mathf.Rad2Deg * Mathf.Atan2(forwardLimitPos.z - position.z, forwardLimitPos.x - position.x);

        return srcAngles;
    }

    public static void DrawLineThick(Vector3 startPos, Vector3 endPos, float thickness=3)
    {
        Handles.DrawBezier(startPos, endPos, startPos, endPos, Gizmos.color, null, thickness);
    }
}
#endif