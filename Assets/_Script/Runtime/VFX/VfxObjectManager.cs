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
        // ����Ʈ �ʱ�ȭ
        initVfxObjects.Clear();

        // ������Ʈ �� ��� ������ �˻�
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (string prefabGuid in allPrefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab != null)
            {
                // VfxObject ������Ʈ�� �ִ��� Ȯ��
                VfxObject vfxObject = prefab.GetComponent<VfxObject>();
                if (vfxObject != null)
                {
                    // VfxObject�� �ִ� �������� ����Ʈ�� �߰�
                    initVfxObjects.Add(vfxObject);
                }
            }
        }

        // �����Ϳ� ���� ���� �ݿ�
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
#endif
}
