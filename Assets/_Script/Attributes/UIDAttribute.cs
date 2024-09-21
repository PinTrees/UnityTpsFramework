using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIDAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(UIDAttribute))]
public class UIDAttributeDrawer : PropertyDrawer
{
    // 고유한 UID를 생성하는 메서드
    private string GenerateUID()
    {
        return System.Guid.NewGuid().ToString();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 위치 설정
        Rect labelRect = new Rect(position.x, position.y, position.width - 70, position.height);
        Rect buttonRect = new Rect(position.x + position.width - 65, position.y, 65, position.height);

        // UID 라벨 출력
        EditorGUI.LabelField(labelRect, $"UID: {property.stringValue}");

        // UID 생성 버튼
        if (GUI.Button(buttonRect, "Generate"))
        {
            property.stringValue = GenerateUID();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif