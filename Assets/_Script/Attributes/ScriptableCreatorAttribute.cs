using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ScriptableCreatorAttribute : PropertyAttribute
{
    // �ʿ��� ��� ��Ʈ����Ʈ�� �Ķ���� �߰� ����
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ScriptableCreatorAttribute))]
public class ScriptableCreatorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect fieldRect = new Rect(position.x, position.y, position.width - 65, position.height);
        EditorGUI.PropertyField(fieldRect, property, label);

        // �ʵ� ���� ��ư �߰� (�� 60)
        Rect buttonRect = new Rect(position.x + position.width - 60, position.y, 60, position.height);
        if (GUI.Button(buttonRect, "Create"))
        {
            CreateScriptableObject(property);
        }
    }

    private void CreateScriptableObject(SerializedProperty property)
    {
        // ������ ScriptableObject�� Ÿ�� ��������
        Type objectType = fieldInfo.FieldType;

        // ScriptableObject �ν��Ͻ� ����
        ScriptableObject newObject = ScriptableObject.CreateInstance(objectType);

        // ���� ������Ʈ â���� ���� �ִ� ��ο� ScriptableObject�� ����
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets"; // �⺻ ���
        }
        else if (!AssetDatabase.IsValidFolder(path))
        {
            path = System.IO.Path.GetDirectoryName(path); // ���õ� ���� ������ �ƴϸ� ���� ������ ����
        }

        // ScriptableObject ���� �� ����
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{objectType.Name}.asset");
        AssetDatabase.CreateAsset(newObject, assetPath);
        AssetDatabase.SaveAssets();

        // ������ ScriptableObject �� �� ��Ŀ��
        EditorGUIUtility.PingObject(newObject);
        Selection.activeObject = newObject;

        // �ʵ忡 ScriptableObject �Ҵ�
        property.objectReferenceValue = newObject;
        property.serializedObject.ApplyModifiedProperties();
    }
}
#endif