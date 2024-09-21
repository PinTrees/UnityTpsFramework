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
    // ������ UID�� �����ϴ� �޼���
    private string GenerateUID()
    {
        return System.Guid.NewGuid().ToString();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // ��ġ ����
        Rect labelRect = new Rect(position.x, position.y, position.width - 70, position.height);
        Rect buttonRect = new Rect(position.x + position.width - 65, position.y, 65, position.height);

        // UID �� ���
        EditorGUI.LabelField(labelRect, $"UID: {property.stringValue}");

        // UID ���� ��ư
        if (GUI.Button(buttonRect, "Generate"))
        {
            property.stringValue = GenerateUID();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif