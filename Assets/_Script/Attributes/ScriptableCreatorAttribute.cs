using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ScriptableCreatorAttribute : PropertyAttribute
{
    // 필요한 경우 어트리뷰트에 파라미터 추가 가능
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ScriptableCreatorAttribute))]
public class ScriptableCreatorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect fieldRect = new Rect(position.x, position.y, position.width - 65, position.height);
        EditorGUI.PropertyField(fieldRect, property, label);

        // 필드 옆에 버튼 추가 (폭 60)
        Rect buttonRect = new Rect(position.x + position.width - 60, position.y, 60, position.height);
        if (GUI.Button(buttonRect, "Create"))
        {
            CreateScriptableObject(property);
        }
    }

    private void CreateScriptableObject(SerializedProperty property)
    {
        // 생성할 ScriptableObject의 타입 가져오기
        Type objectType = fieldInfo.FieldType;

        // ScriptableObject 인스턴스 생성
        ScriptableObject newObject = ScriptableObject.CreateInstance(objectType);

        // 현재 프로젝트 창에서 보고 있는 경로에 ScriptableObject를 생성
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            path = "Assets"; // 기본 경로
        }
        else if (!AssetDatabase.IsValidFolder(path))
        {
            path = System.IO.Path.GetDirectoryName(path); // 선택된 것이 폴더가 아니면 상위 폴더로 지정
        }

        // ScriptableObject 생성 및 저장
        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/{objectType.Name}.asset");
        AssetDatabase.CreateAsset(newObject, assetPath);
        AssetDatabase.SaveAssets();

        // 생성된 ScriptableObject 핑 및 포커스
        EditorGUIUtility.PingObject(newObject);
        Selection.activeObject = newObject;

        // 필드에 ScriptableObject 할당
        property.objectReferenceValue = newObject;
        property.serializedObject.ApplyModifiedProperties();
    }
}
#endif