using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TextureBoxAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TextureBoxAttribute))]
public class TextureBoxDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // �ؽ�ó �ڽ��� �׸��� ���� position ���� �����մϴ�.
        var textureBoxPosition = position;
        textureBoxPosition.height = 128;
        textureBoxPosition.width = 128;

        if (property.objectReferenceValue != null)
        {
            // �ؽ�ó�� �����ͼ� �׸��ϴ�.
            Texture2D texture = (Texture2D)property.objectReferenceValue;
            EditorGUI.DrawPreviewTexture(textureBoxPosition, texture);
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        EditorGUILayout.PropertyField(property, GUIContent.none);

        EditorGUILayout.EndHorizontal();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // �󺧰� �ؽ�ó ���̸� ������ �� ���̸� ��ȯ�մϴ�.
        return 128 + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif
