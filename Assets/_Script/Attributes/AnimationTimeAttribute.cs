using System;
using UnityEngine;
using FIMSpace.Basics;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class AnimationInNormalizeTimeData
{
    public float start;
    public float exit;
}

public class AnimationTimeAttribute : PropertyAttribute
{
    public float Min { get; private set; }
    public float Max { get; private set; }

    public AnimationTimeAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(AnimationTimeAttribute))]
public class AnimationTimeAttributeDrawer : PropertyDrawer
{
    private const float LineHeight = 2f;
    private const float MarkerHeight = 8f;
    private const float HeaderHeight = 18f;
    private const float BoxPadding = 1f;
    private const float AdditionalHeight = 40f; // �� ���� ����

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // �� ���� ��ȯ: ��� + �����̴� + �߰� ����
        return HeaderHeight + AdditionalHeight + BoxPadding * 2 + 4 + 4;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // �Ӽ��� ��������
        SerializedProperty startProp = property.FindPropertyRelative("start");
        SerializedProperty exitProp = property.FindPropertyRelative("exit");

        AnimationTimeAttribute animAttr = attribute as AnimationTimeAttribute;

        float startTime = startProp.floatValue;
        float exitTime = exitProp.floatValue;

        // ��ü �ڽ� �׸��� (���� ó��)
        Rect boxRect = new Rect(position.x, position.y + 4, position.width, HeaderHeight + AdditionalHeight + BoxPadding * 2 + 4);
        DrawRoundedBox(boxRect, new Color(0.2f, 0.2f, 0.2f), Color.black);

        // ��� �ڽ� �׸���
        Rect headerRect = new Rect(boxRect.x + 1, boxRect.y + 1, boxRect.width - 2, HeaderHeight);
        EditorGUI.DrawRect(headerRect, new Color(0.2f, 0.2f, 0.2f)); // ��� ����� �� ���� ������
        EditorGUI.LabelField(headerRect, $" {property.displayName}");

        // ����̴� ���� �׸���
        Rect dividerRect = new Rect(boxRect.x + 1, headerRect.yMax + 2, boxRect.width - 2, 2f);
        EditorGUI.DrawRect(dividerRect, new Color(0, 0, 0, 0.5f));

        // ������ �׸���
        DrawRuler(boxRect, BoxPadding, HeaderHeight + 4, animAttr.Min, animAttr.Max);

        // �����̴� �׸��� (�ܺ� �Լ� ȣ��)
        DrawSlider(boxRect, BoxPadding, HeaderHeight + 20 + 4, 20, startProp, exitProp, animAttr, ref startTime, ref exitTime);

        // Ű������ ��Ŀ �׸��� (������ ���)
        DrawMarker(boxRect, BoxPadding, HeaderHeight + 20 + 4, startTime, animAttr.Min, animAttr.Max, Color.cyan);
        DrawMarker(boxRect, BoxPadding, HeaderHeight + 20 + 4, exitTime, animAttr.Min, animAttr.Max, Color.red);
    }

    private void DrawMarker(Rect boxRect, float BoxPadding, float yOffset, float time, float min, float max, Color color)
    {
        // �����̴��� ���õ� Rect�� ����
        Rect sliderRect = new Rect(boxRect.x + BoxPadding, boxRect.y + yOffset, boxRect.width - BoxPadding * 2, 20f);

        float normalizedTime = Mathf.InverseLerp(min, max, time);
        float markerX = Mathf.Lerp(sliderRect.x, sliderRect.xMax, normalizedTime);

        // ������ ����� �׸��� ���� ����Ʈ ���� (���簢�� ������ ������)
        float markerSize = 16f; // �������� ũ��
        Vector3[] diamondVertices = new Vector3[4]
        {
        new Vector3(markerX, sliderRect.y + sliderRect.height / 2 - markerSize / 2, 0),
        new Vector3(markerX - markerSize / 2, sliderRect.y + sliderRect.height / 2, 0),
        new Vector3(markerX, sliderRect.y + sliderRect.height / 2 + markerSize / 2, 0),
        new Vector3(markerX + markerSize / 2, sliderRect.y + sliderRect.height / 2, 0)
        };

        // ������ ���� ���� ä���
        Handles.color = new Color(0.7f, 0.7f, 0.7f);
        Handles.DrawAAConvexPolygon(diamondVertices);

        // ������ �ƿ����� �׸���
        Handles.color = Color.black; // �ƿ����� ����
        Handles.DrawPolyLine(diamondVertices);
        Handles.DrawLine(diamondVertices[3], diamondVertices[0]);

        // �� ǥ���ϱ�
        string timeLabel = $"{time:F2}";

        // GUIStyle�� ����Ͽ� ��Ʈ ũ�� ����
        GUIStyle smallLabelStyle = new GUIStyle(GUI.skin.label);
        smallLabelStyle.fontSize = 10; // ��Ʈ ũ�⸦ 8�� ����
        smallLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f); // �ؽ�Ʈ ����

        // �ؽ�Ʈ ũ�⸦ ����ϰ� ��ġ
        Vector2 textSize = smallLabelStyle.CalcSize(new GUIContent(timeLabel));
        Rect labelRect = new Rect(markerX - textSize.x / 2, sliderRect.y - textSize.y - 5f, textSize.x, textSize.y);
        EditorGUI.LabelField(labelRect, timeLabel, smallLabelStyle);
    }

    private void DrawRoundedBox(Rect rect, Color fillColor, Color outlineColor)
    {
        // �׶����� �ڽ� ���� ä���
        EditorGUI.DrawRect(rect, fillColor);

        // �ƿ����� �׸���
        Handles.DrawSolidRectangleWithOutline(rect, Color.clear, outlineColor);
    }

    private void DrawRuler(Rect boxRect, float BoxPadding, float HeaderHeight, float min, float max)
    {
        Rect rulerRect = new Rect(boxRect.x + BoxPadding, boxRect.y + HeaderHeight, boxRect.width - BoxPadding * 2, 20f);

        // ������ ���
        EditorGUI.DrawRect(rulerRect, new Color(0.18f, 0.18f, 0.18f));

        int numberOfMarks = 100; // ������ ���� (0.01 �������� 100���� ����)
        float increment = (max - min) / numberOfMarks;

        for (int i = 1; i < numberOfMarks; i++) // 0.0�� 1.0�� ����
        {
            float normalizedPosition = i / (float)numberOfMarks;
            float markX = Mathf.Lerp(rulerRect.x, rulerRect.xMax, normalizedPosition);

            // 0.2���� �� �β��� �� ����, 0.01���� �Ϲ� ����
            float lineHeight = (i % 20 == 0) ? 10f : 6f; // 0.2 (i % 20 == 0)���� �� ����, �������� ª�� ����
            float lineWidth = (i % 20 == 0) ? 3f : 1.8f;   // 0.2���� �� �β��� ����, �������� �Ϲ� �β�

            Handles.color = (i % 20 == 0) ? Color.gray : new Color(0.35f, 0.35f, 0.35f);
            Handles.DrawAAPolyLine(lineWidth, new Vector3[]
            {
            new Vector3(markX, rulerRect.y + rulerRect.height - lineHeight, 0),
            new Vector3(markX, rulerRect.y + rulerRect.height, 0)
            });

            // �� ���뿡�� ���� ǥ�� (���� ǥ��)
            //if (i % 20 == 0)
            //{
            //    EditorGUI.LabelField(new Rect(markX - 10f, rulerRect.y, 40f, 20f), $"{(min + increment * i):F2}", new GUIStyle() { fontSize = 8, normal = new GUIStyleState() { textColor = Color.white } });
            //}
        }
    }

    private void DrawSlider(Rect boxRect, float BoxPadding, float HeaderHeight, float AdditionalHeight, SerializedProperty startProp, SerializedProperty exitProp, AnimationTimeAttribute animAttr, ref float startTime, ref float exitTime)
    {
        // �����̴� �ڽ� �׸���
        Rect sliderRect = new Rect(boxRect.x + BoxPadding, boxRect.y + HeaderHeight + BoxPadding, boxRect.width - BoxPadding * 2, AdditionalHeight);
        EditorGUI.DrawRect(sliderRect, new Color(0.13f, 0.13f, 0.13f, 1.0f)); // �����̴� ����� �� ���� ������

        // ��� ��� ���� �׸���
        Rect centerLineRect = new Rect(sliderRect.x, sliderRect.y + sliderRect.height / 2 - 1,
            sliderRect.width, 1f);
        EditorGUI.DrawRect(centerLineRect, new Color(1, 1, 1, 0.15f));

        // ���� �̺�Ʈ ó��
        Event e = Event.current;
        if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
        {
            if (sliderRect.Contains(e.mousePosition))
            {
                float relativePos = Mathf.Clamp((e.mousePosition.x - sliderRect.x) / sliderRect.width, 0f, 1f);
                float timeValue = Mathf.Lerp(animAttr.Min, animAttr.Max, relativePos);

                if (Mathf.Abs(timeValue - startTime) < Mathf.Abs(timeValue - exitTime))
                {
                    startTime = timeValue;
                }
                else
                {
                    exitTime = timeValue;
                }

                startProp.floatValue = Mathf.Clamp(startTime, animAttr.Min, exitTime);
                exitProp.floatValue = Mathf.Clamp(exitTime, startTime, animAttr.Max);
                e.Use(); // �̺�Ʈ ���
            }
        }
    }
}
#endif