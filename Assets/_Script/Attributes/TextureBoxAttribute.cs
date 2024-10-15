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
        // 텍스처 박스를 그리기 위해 position 값을 조정합니다.
        var textureBoxPosition = position;
        textureBoxPosition.height = 128;
        textureBoxPosition.width = 128;

        if (property.objectReferenceValue != null)
        {
            // 텍스처를 가져와서 그립니다.
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
        // 라벨과 텍스처 높이를 포함한 총 높이를 반환합니다.
        return 128 + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif
