using System.Linq;
using UnityEditor;
using UnityEngine;

public class ScriptableBoneController : MonoBehaviour
{
    [ScriptableCreator]
    public ScriptableBoneData boneData;

#if UNITY_EDITOR
    [Header("Editor Value")]
    public ScriptableBone editingBoneData;
#endif

    [Header("Runtime Value")]
    public CharacterActorBase ownerCharacter;
    public Animator animator;

    public void Init(CharacterActorBase owner)
    {
        ownerCharacter = owner;
    }

#if UNITY_EDITOR
    [Button("Save Current Edite Bone")]
    public void _Editor_SaveBone()
    {
        if (boneData == null)
            return;

        var target = boneData.bones.Where(e => e.targetBone == editingBoneData.targetBone).FirstOrDefault();
        if (target != null)
        {
            target.upAxis = editingBoneData.upAxis;
            target.forwardAxis = editingBoneData.forwardAxis;
        }
        else
        {
            boneData.bones.Add(editingBoneData);
        }

        EditorUtility.SetDirty(boneData);
        AssetDatabase.SaveAssets();
    }
#endif
}
