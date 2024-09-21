using System;
using UnityEngine;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

[Serializable]
public class AnimationClip8Dir
{
    public bool useRootMotion;

    [Space]
    public AnimationClip CENTER;
    public AnimationClip F;
    public AnimationClip R;
    public AnimationClip L;
    public AnimationClip B;
    public AnimationClip FR;
    public AnimationClip FL;
    public AnimationClip BR;
    public AnimationClip BL;
}

[Serializable]
[CreateAssetMenu(menuName = "Scriptable/Animator/BaseSetting")]
public class ScriptableAnimatorSetting : ScriptableObject
{
    public RuntimeAnimatorController animatorController;

    [Header("Sub Scriptable Animator Setting")]
    public BodyAnimatorSetting bodyAnimatorSetting;
    public IdleAnimatorSetting idleAnimatorSetting; 
    public MovementAnimatorSetting movementAnimatorSetting; 
    public DodgeAnimatorSetting dodgeAnimatorSetting;
    public HitAnimatorSetting hitAnimatorSetting;
    public DefenseAnimatorSetting defenseAnimatorSetting;

    [Header("Attack Data")]
    public List<AttackTree> attackAnimatorSettings = new();


#if UNITY_EDITOR
    [ButtonSO("Create")]
    public void _Editor_Create()
    {
        if(bodyAnimatorSetting == null)
        {
            var bodyAnimatorSettingType = typeof(BodyAnimatorSetting);
            bodyAnimatorSetting = CreateInstance(bodyAnimatorSettingType) as BodyAnimatorSetting;
            bodyAnimatorSetting.name = bodyAnimatorSettingType.Name;
            bodyAnimatorSetting.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(bodyAnimatorSetting, this);
        }

        if (idleAnimatorSetting == null)
        {
            var idleAnimatorSettingType = typeof(IdleAnimatorSetting);
            idleAnimatorSetting = CreateInstance(idleAnimatorSettingType) as IdleAnimatorSetting;
            idleAnimatorSetting.name = idleAnimatorSettingType.Name;
            idleAnimatorSetting.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(idleAnimatorSetting, this);
        }

        if (movementAnimatorSetting == null)
        {
            var movementAnimatorSettingType = typeof(MovementAnimatorSetting);
            movementAnimatorSetting = CreateInstance(movementAnimatorSettingType) as MovementAnimatorSetting;
            movementAnimatorSetting.name = movementAnimatorSettingType.Name;
            movementAnimatorSetting.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(movementAnimatorSetting, this);
        }

        if (dodgeAnimatorSetting == null)
        {
            var dodgeAnimatorSettingType = typeof(DodgeAnimatorSetting);
            dodgeAnimatorSetting = CreateInstance(dodgeAnimatorSettingType) as DodgeAnimatorSetting;
            dodgeAnimatorSetting.name = dodgeAnimatorSettingType.Name;
            dodgeAnimatorSetting.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(dodgeAnimatorSetting, this);
        }

        if (hitAnimatorSetting == null)
        {
            var hitAnimatorSettingType = typeof(HitAnimatorSetting);
            hitAnimatorSetting = CreateInstance(hitAnimatorSettingType) as HitAnimatorSetting;
            hitAnimatorSetting.name = hitAnimatorSettingType.Name;
            hitAnimatorSetting.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(hitAnimatorSetting, this);
        }

        if (defenseAnimatorSetting == null)
        {
            var defenseAnimatorSettingType = typeof(DefenseAnimatorSetting);
            defenseAnimatorSetting = CreateInstance(defenseAnimatorSettingType) as DefenseAnimatorSetting;
            defenseAnimatorSetting.name = defenseAnimatorSettingType.Name;
            defenseAnimatorSetting.hideFlags = HideFlags.None;
            AssetDatabase.AddObjectToAsset(defenseAnimatorSetting, this);
        } 

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.SetDirty(this);
    }

    [ButtonSO("Setup")]
    public void _Editor_Setup()
    {
        string basePath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));

        string animatorName = $"{this.name} [AnimatorController]";
        string fullPath = System.IO.Path.Combine(basePath, animatorName + ".asset");

        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(fullPath) != null)
        {
            AssetDatabase.DeleteAsset(fullPath);
        }

        var editorAnimatorController = AnimatorController.CreateAnimatorControllerAtPath(fullPath);
        var animatorLayer = editorAnimatorController.layers[0];
        this.animatorController = editorAnimatorController;

        // Setup Parameter
        editorAnimatorController.AddParameter("x", AnimatorControllerParameterType.Float);
        editorAnimatorController.AddParameter("y", AnimatorControllerParameterType.Float);
        editorAnimatorController.AddParameter("hx", AnimatorControllerParameterType.Float);
        editorAnimatorController.AddParameter("hy", AnimatorControllerParameterType.Float);
        editorAnimatorController.AddParameter("dx", AnimatorControllerParameterType.Float);
        editorAnimatorController.AddParameter("dy", AnimatorControllerParameterType.Float);

        bodyAnimatorSetting?._Editor_CreateAnimator(editorAnimatorController, animatorLayer);
        idleAnimatorSetting?._Editor_CreateAnimator(editorAnimatorController, animatorLayer);
        movementAnimatorSetting?._Editor_CreateAnimator(editorAnimatorController, animatorLayer);
        dodgeAnimatorSetting?._Editor_CreateAnimator(editorAnimatorController, animatorLayer);
        hitAnimatorSetting?._Editor_CreateAnimator(editorAnimatorController, animatorLayer);
        defenseAnimatorSetting?._Editor_CreateAnimator(editorAnimatorController, animatorLayer);

        attackAnimatorSettings.ForEach(e =>
        {
            e?._Editor_CreateAnimator(editorAnimatorController, animatorLayer);
        });

        animatorLayer.AddPublicState("CustomHit", "Hit");

        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(editorAnimatorController);
        EditorUtility.SetDirty(this);
    }
#endif
}
