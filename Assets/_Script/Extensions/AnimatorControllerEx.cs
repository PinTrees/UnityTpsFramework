using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Security.Cryptography.X509Certificates;
using System.IO;




#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

public static class AnimatorControllerEx
{
    // Utility
#if UNITY_EDITOR
    public static AnimatorControllerLayer FindOrCreateLayer(this AnimatorController animatorController, 
        string layerName, AvatarMask avataMask = null)
    {
        // 레이어 검색
        foreach (var layer in animatorController.layers)
        {
            if (layer.name == layerName)
            {
                return layer; // 레이어가 이미 있으면 반환
            }
        }

        // 레이어가 없으면 새로 생성
        var newLayer = new AnimatorControllerLayer
        {
            name = layerName,
            stateMachine = new AnimatorStateMachine(),
            avatarMask = avataMask,
        };

        newLayer.stateMachine.name = newLayer.name;
        newLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;

        if (AssetDatabase.GetAssetPath(animatorController) != "")
            AssetDatabase.AddObjectToAsset(newLayer.stateMachine, AssetDatabase.GetAssetPath(animatorController));

        animatorController.AddLayer(newLayer);
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(animatorController);

        return newLayer;
    }

    public static int GetLayerIndex(this AnimatorController animatorController, string layerName)
    {
        for (int i = 0; i < animatorController.layers.Length; i++)
        {
            if (animatorController.layers[i].name == layerName)
            {
                return i; 
            }
        }

        return -1; 
    }
#endif


#if UNITY_EDITOR
    public static void AddPublicState(this AnimatorControllerLayer layer, string stateName, string tag = null)
    {
        string basePath = "Assets/_Animation/Public/";

        if (string.IsNullOrEmpty(tag))
        {
            tag = stateName.ToString();
        }

        string animatorName = $"{stateName}";
        string fullPath = Path.Combine(basePath, animatorName + ".asset");

        // 폴더가 존재하지 않는 경우 생성
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
            AssetDatabase.Refresh();
        }

        AnimationClip motion;

        // 클립이 이미 존재하는지 확인
        motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(fullPath);
        if (motion == null)
        {
            // 존재하지 않으면 새 애니메이션 클립 생성
            motion = new AnimationClip();
            AssetDatabase.CreateAsset(motion, fullPath);
            AssetDatabase.SaveAssets();
        }
        else
        {
        }

        // 에디터에서 생성된 또는 로드된 애니메이션 클립 선택
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = motion;

        // Animator 상태 추가 또는 업데이트
        AnimatorState state = layer.stateMachine.AddState(stateName.ToString());
        state.motion = motion;
        state.tag = tag;
    }
    public static void AddState(this AnimatorControllerLayer layer, AnimationClip motion, string stateName, string tag = "")
    {
        if (tag == "")
            tag = stateName;

        AnimatorState state = layer.stateMachine.AddState(stateName.ToString());
        state.motion = motion;
        state.tag = tag;
    }

    public static BlendTree CreateBlendTree(
       this AnimatorController animatorController, 
       AnimationClip8Dir animationClip8Dir,
       string stateName, string tag = "", AnimatorControllerLayer layer = null,  string paramXName = "x", string paramYName = "y")
    {
        if (tag == "")
            tag = stateName;

        if (layer == null)
            layer = animatorController.layers.First();

        var layerIndex = animatorController.GetLayerIndex(layer.name);

        BlendTree blendTree;
    
        animatorController.CreateBlendTreeInController(stateName, out blendTree, layerIndex);
        blendTree.blendType = BlendTreeType.FreeformDirectional2D;
        blendTree.blendParameter = paramXName;
        blendTree.blendParameterY = paramYName;

        blendTree.AddChild(animationClip8Dir.CENTER, Vector2.zero);
        blendTree.AddChild(animationClip8Dir.F, new Vector2(0, 1).normalized);
        blendTree.AddChild(animationClip8Dir.B, new Vector2(0, -1).normalized);
        blendTree.AddChild(animationClip8Dir.R, new Vector2(1, 0).normalized);
        blendTree.AddChild(animationClip8Dir.L, new Vector2(-1, 0).normalized);
        blendTree.AddChild(animationClip8Dir.FR, new Vector2(1, 1).normalized);
        blendTree.AddChild(animationClip8Dir.FL, new Vector2(-1, 1).normalized);
        blendTree.AddChild(animationClip8Dir.BL, new Vector2(-1, -1).normalized);
        blendTree.AddChild(animationClip8Dir.BR, new Vector2(1, -1).normalized);

        EditorUtility.SetDirty(blendTree);

        layer.stateMachine.FindState(stateName).tag = tag;

        EditorUtility.SetDirty(animatorController);
        AssetDatabase.SaveAssets();

        return blendTree;
    }

    public static BlendTree CreateBlendTree(
     this AnimatorController animatorController,
     List<AnimationClip> animationClips,
     string stateName, string tag = "", string paramName="")
    {
        if (tag == "")
            tag = stateName;

        BlendTree blendTree;

        animatorController.CreateBlendTreeInController(stateName, out blendTree);
        blendTree.blendType = BlendTreeType.Simple1D;
        blendTree.blendParameter = paramName;

        int index = 0;
        animationClips.ForEach(e =>
        {
            blendTree.AddChild(e, index++);
        });

        animatorController.layers.First().stateMachine.FindState(stateName).tag = tag;

        return blendTree;
    }
#endif
}
