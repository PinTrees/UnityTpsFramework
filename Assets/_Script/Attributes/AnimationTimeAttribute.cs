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
    private const float AdditionalHeight = 40f; // 총 높이 조정

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 총 높이 반환: 헤더 + 슬라이더 + 추가 높이
        return HeaderHeight + AdditionalHeight + BoxPadding * 2 + 4 + 4;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 속성들 가져오기
        SerializedProperty startProp = property.FindPropertyRelative("start");
        SerializedProperty exitProp = property.FindPropertyRelative("exit");

        AnimationTimeAttribute animAttr = attribute as AnimationTimeAttribute;

        float startTime = startProp.floatValue;
        float exitTime = exitProp.floatValue;

        // 전체 박스 그리기 (라운딩 처리)
        Rect boxRect = new Rect(position.x, position.y + 4, position.width, HeaderHeight + AdditionalHeight + BoxPadding * 2 + 4);
        DrawRoundedBox(boxRect, new Color(0.2f, 0.2f, 0.2f), Color.black);

        // 헤더 박스 그리기
        Rect headerRect = new Rect(boxRect.x + 1, boxRect.y + 1, boxRect.width - 2, HeaderHeight);
        EditorGUI.DrawRect(headerRect, new Color(0.2f, 0.2f, 0.2f)); // 헤더 배경을 더 진한 색으로
        EditorGUI.LabelField(headerRect, $" {property.displayName}");

        // 디바이더 라인 그리기
        Rect dividerRect = new Rect(boxRect.x + 1, headerRect.yMax + 2, boxRect.width - 2, 2f);
        EditorGUI.DrawRect(dividerRect, new Color(0, 0, 0, 0.5f));

        // 눈금자 그리기
        DrawRuler(boxRect, BoxPadding, HeaderHeight + 4, animAttr.Min, animAttr.Max);

        // 슬라이더 그리기 (외부 함수 호출)
        DrawSlider(boxRect, BoxPadding, HeaderHeight + 20 + 4, 20, startProp, exitProp, animAttr, ref startTime, ref exitTime);

        // 키프레임 마커 그리기 (마름모 모양)
        DrawMarker(boxRect, BoxPadding, HeaderHeight + 20 + 4, startTime, animAttr.Min, animAttr.Max, Color.cyan);
        DrawMarker(boxRect, BoxPadding, HeaderHeight + 20 + 4, exitTime, animAttr.Min, animAttr.Max, Color.red);
    }

    private void DrawMarker(Rect boxRect, float BoxPadding, float yOffset, float time, float min, float max, Color color)
    {
        // 슬라이더와 관련된 Rect를 생성
        Rect sliderRect = new Rect(boxRect.x + BoxPadding, boxRect.y + yOffset, boxRect.width - BoxPadding * 2, 20f);

        float normalizedTime = Mathf.InverseLerp(min, max, time);
        float markerX = Mathf.Lerp(sliderRect.x, sliderRect.xMax, normalizedTime);

        // 마름모 모양을 그리기 위한 포인트 설정 (정사각형 형태의 마름모)
        float markerSize = 16f; // 마름모의 크기
        Vector3[] diamondVertices = new Vector3[4]
        {
        new Vector3(markerX, sliderRect.y + sliderRect.height / 2 - markerSize / 2, 0),
        new Vector3(markerX - markerSize / 2, sliderRect.y + sliderRect.height / 2, 0),
        new Vector3(markerX, sliderRect.y + sliderRect.height / 2 + markerSize / 2, 0),
        new Vector3(markerX + markerSize / 2, sliderRect.y + sliderRect.height / 2, 0)
        };

        // 마름모 내부 색상 채우기
        Handles.color = new Color(0.7f, 0.7f, 0.7f);
        Handles.DrawAAConvexPolygon(diamondVertices);

        // 마름모 아웃라인 그리기
        Handles.color = Color.black; // 아웃라인 색상
        Handles.DrawPolyLine(diamondVertices);
        Handles.DrawLine(diamondVertices[3], diamondVertices[0]);

        // 값 표시하기
        string timeLabel = $"{time:F2}";

        // GUIStyle을 사용하여 폰트 크기 조정
        GUIStyle smallLabelStyle = new GUIStyle(GUI.skin.label);
        smallLabelStyle.fontSize = 10; // 폰트 크기를 8로 설정
        smallLabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f); // 텍스트 색상

        // 텍스트 크기를 계산하고 배치
        Vector2 textSize = smallLabelStyle.CalcSize(new GUIContent(timeLabel));
        Rect labelRect = new Rect(markerX - textSize.x / 2, sliderRect.y - textSize.y - 5f, textSize.x, textSize.y);
        EditorGUI.LabelField(labelRect, timeLabel, smallLabelStyle);
    }

    private void DrawRoundedBox(Rect rect, Color fillColor, Color outlineColor)
    {
        // 그라운딩된 박스 내부 채우기
        EditorGUI.DrawRect(rect, fillColor);

        // 아웃라인 그리기
        Handles.DrawSolidRectangleWithOutline(rect, Color.clear, outlineColor);
    }

    private void DrawRuler(Rect boxRect, float BoxPadding, float HeaderHeight, float min, float max)
    {
        Rect rulerRect = new Rect(boxRect.x + BoxPadding, boxRect.y + HeaderHeight, boxRect.width - BoxPadding * 2, 20f);

        // 눈금자 배경
        EditorGUI.DrawRect(rulerRect, new Color(0.18f, 0.18f, 0.18f));

        int numberOfMarks = 100; // 눈금의 개수 (0.01 간격으로 100개의 눈금)
        float increment = (max - min) / numberOfMarks;

        for (int i = 1; i < numberOfMarks; i++) // 0.0과 1.0은 제외
        {
            float normalizedPosition = i / (float)numberOfMarks;
            float markX = Mathf.Lerp(rulerRect.x, rulerRect.xMax, normalizedPosition);

            // 0.2마다 더 두꺼운 긴 막대, 0.01마다 일반 막대
            float lineHeight = (i % 20 == 0) ? 10f : 6f; // 0.2 (i % 20 == 0)에서 긴 막대, 나머지는 짧은 막대
            float lineWidth = (i % 20 == 0) ? 3f : 1.8f;   // 0.2에서 더 두꺼운 막대, 나머지는 일반 두께

            Handles.color = (i % 20 == 0) ? Color.gray : new Color(0.35f, 0.35f, 0.35f);
            Handles.DrawAAPolyLine(lineWidth, new Vector3[]
            {
            new Vector3(markX, rulerRect.y + rulerRect.height - lineHeight, 0),
            new Vector3(markX, rulerRect.y + rulerRect.height, 0)
            });

            // 긴 막대에만 값을 표시 (위에 표시)
            //if (i % 20 == 0)
            //{
            //    EditorGUI.LabelField(new Rect(markX - 10f, rulerRect.y, 40f, 20f), $"{(min + increment * i):F2}", new GUIStyle() { fontSize = 8, normal = new GUIStyleState() { textColor = Color.white } });
            //}
        }
    }

    private void DrawSlider(Rect boxRect, float BoxPadding, float HeaderHeight, float AdditionalHeight, SerializedProperty startProp, SerializedProperty exitProp, AnimationTimeAttribute animAttr, ref float startTime, ref float exitTime)
    {
        // 슬라이더 박스 그리기
        Rect sliderRect = new Rect(boxRect.x + BoxPadding, boxRect.y + HeaderHeight + BoxPadding, boxRect.width - BoxPadding * 2, AdditionalHeight);
        EditorGUI.DrawRect(sliderRect, new Color(0.13f, 0.13f, 0.13f, 1.0f)); // 슬라이더 배경을 더 연한 색으로

        // 가운데 흰색 라인 그리기
        Rect centerLineRect = new Rect(sliderRect.x, sliderRect.y + sliderRect.height / 2 - 1,
            sliderRect.width, 1f);
        EditorGUI.DrawRect(centerLineRect, new Color(1, 1, 1, 0.15f));

        // 현재 이벤트 처리
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
                e.Use(); // 이벤트 사용
            }
        }
    }
}
#endif