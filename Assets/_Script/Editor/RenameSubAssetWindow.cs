using UnityEditor;
using UnityEngine;

public class RenameSubAssetWindow : EditorWindow
{
    private string newName = "";
    private Object targetObject;

    public static void ShowWindow(Object target)
    {
        // 윈도우 생성
        var window = GetWindow<RenameSubAssetWindow>("Rename Sub Asset");
        window.targetObject = target;
        window.newName = target.name;
    }

    void OnGUI()
    {
        // 새 이름 입력 필드
        GUILayout.Label("Rename Sub Asset", EditorStyles.boldLabel);
        newName = EditorGUILayout.TextField("New Name", newName);

        // 버튼 - 이름 변경
        if (GUILayout.Button("Rename"))
        {
            if (!string.IsNullOrEmpty(newName) && targetObject != null && targetObject.name != newName)
            {
                Undo.RecordObject(targetObject, "Rename Sub Asset");
                targetObject.name = newName;
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Close();
            }
        }
    }
}
