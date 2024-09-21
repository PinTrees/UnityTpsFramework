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
            // 이름 변경 창을 표시
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
                // 서브 에셋 삭제
                Object.DestroyImmediate(selectedObject, true); // 서브 에셋 삭제
                AssetDatabase.SaveAssets(); // 에셋 상태 저장
                AssetDatabase.Refresh(); // 프로젝트 새로고침
            }
        }
        else
        {
            Debug.LogError("Selected object is not a sub-asset.");
        }
    }
}
