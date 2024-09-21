using UnityEditor;
using UnityEngine;

public class SubAssetUtility
{
    [MenuItem("Assets/Rename Sub Asset", false, 1)]
    private static void RenameSubAsset()
    {
        var selectedObject = Selection.activeObject;

        if (selectedObject != null && AssetDatabase.IsSubAsset(selectedObject))
        {
            // �̸� ���� â�� ǥ��
            RenameSubAssetWindow.ShowWindow(selectedObject);
        }
        else
        {
            Debug.LogError("Selected object is not a sub-asset.");
        }
    }

    [MenuItem("Assets/Delete Sub Asset", false, 1)]
    private static void DeleteSubAsset()
    {
        var selectedObject = Selection.activeObject;

        if (selectedObject != null && AssetDatabase.IsSubAsset(selectedObject))
        {
            if (EditorUtility.DisplayDialog("Delete Sub Asset", $"Are you sure you want to delete {selectedObject.name}?", "Delete", "Cancel"))
            {
                // ���� ���� ����
                Object.DestroyImmediate(selectedObject, true); // ���� ���� ����
                AssetDatabase.SaveAssets(); // ���� ���� ����
                AssetDatabase.Refresh(); // ������Ʈ ���ΰ�ħ
            }
        }
        else
        {
            Debug.LogError("Selected object is not a sub-asset.");
        }
    }
}
