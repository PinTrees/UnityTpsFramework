using UnityEditor;
using UnityEngine;

public class RenameSubAssetWindow : EditorWindow
{
    private string newName = "";
    private Object targetObject;

    public static void ShowWindow(Object target)
    {
        // ������ ����
        var window = GetWindow<RenameSubAssetWindow>("Rename Sub Asset");
        window.targetObject = target;
        window.newName = target.name;
    }

    void OnGUI()
    {
        // �� �̸� �Է� �ʵ�
        GUILayout.Label("Rename Sub Asset", EditorStyles.boldLabel);
        newName = EditorGUILayout.TextField("New Name", newName);

        // ��ư - �̸� ����
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
