using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class VfxObjectManager : Singleton<VfxObjectManager>
{
    public List<VfxObject> initVfxObjects = new();

    public override void Init()
    {
        base.Init();

        initVfxObjects.ForEach(e =>
        {
            ObjectPoolManager.Instance.CreatePool(e.name, e.gameObject);
        });
    }

#if UNITY_EDITOR
    [Button("Add VfxObjects In Project")]
    public void _Editor_AddVfxObjectInProject()
    {
        // 리스트 초기화
        initVfxObjects.Clear();

        // 프로젝트 내 모든 프리팹 검색
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string prefabGuid in allPrefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                // VfxObject 컴포넌트가 있는지 확인
                VfxObject vfxObject = prefab.GetComponent<VfxObject>();
                if (vfxObject != null)
                {
                    // VfxObject가 있는 프리팹을 리스트에 추가
                    initVfxObjects.Add(vfxObject);
                }
            }
        }

        // 에디터에 변경 사항 반영
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}
