using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Animations;

using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerLayer = UnityEditor.Animations.AnimatorControllerLayer;
#endif

using System.Linq;
using Cysharp.Threading.Tasks;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable/Attack/Tree")]
public class AttackTree : ScriptableObject
{
    [HideInInspector] 
    public List<AttackNode> attackNodes = new();


#if UNITY_EDITOR
    public void _Editor_CreateAnimator(AnimatorController animatorController, AnimatorControllerLayer animatorLayer)
    {
        attackNodes.ForEach(e =>
        {
            animatorLayer.AddState(e.animationClip, e.uid); 
        });
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(AttackTree))]
public class AttackTreeEditor : Editor
{
    private AttackTree owner;
    private ReorderableList attackNodeList;

    public void OnEnable()
    {
        owner = (AttackTree)target;

        // ReorderableList 초기화
        attackNodeList = new ReorderableList(serializedObject,
            serializedObject.FindProperty("attackNodes"),
            true, true, true, true);

        // 커스텀 스타일 설정
        GUIStyle headerStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12, // 헤더 텍스트 크기 설정
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft
        };

        // 리스트의 헤더 설정
        attackNodeList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Attack Nodes", headerStyle);
        };

        float elementSpacing = 2f; // 요소 간의 간격

        // 요소의 높이를 설정 (기본 높이 + 간격)
        attackNodeList.elementHeightCallback = (index) =>
        {
            return EditorGUIUtility.singleLineHeight + elementSpacing;
        };

        // 리스트의 각 요소 그리기
        attackNodeList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += elementSpacing / 2; // 요소 상단 간격
            rect.height -= elementSpacing; // 요소 하단 간격을 포함해 높이 줄이기

            var element = attackNodeList.serializedProperty.GetArrayElementAtIndex(index);
            var attackNode = element.objectReferenceValue as AttackNode;

            if (attackNode != null)
            {
                // Display the rest of the fields if necessary
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            }
        };

        // 리스트에서 + 버튼 클릭 시 동작
        attackNodeList.onAddCallback = (ReorderableList list) =>
        {
            var type = typeof(AttackNode);
            var attackNode = CreateInstance(type) as AttackNode;
            attackNode.name = type.Name;
            attackNode.uid = GUID.Generate().ToString();
            attackNode.hideFlags = HideFlags.None;

            // Add the condition as a sub-asset to the AttackNode
            AssetDatabase.AddObjectToAsset(attackNode, owner);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            owner.attackNodes.Add(attackNode);
            EditorUtility.SetDirty(attackNode);
        };

        // 리스트에서 - 버튼 클릭 시 동작
        attackNodeList.onRemoveCallback = (ReorderableList list) =>
        {
            // 조건을 리스트에서 제거하고, 해당 에셋을 삭제
            try
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(list.index);
                if (element != null && element.objectReferenceValue != null)
                {
                    var condition = element.objectReferenceValue;
                    DestroyImmediate(condition, true);
                }
            } 
            catch { }

            owner.attackNodes.RemoveAt(list.index);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(owner);
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space();
        attackNodeList.DoLayoutList();
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        // 변경 사항을 저장
        if (GUI.changed)
        {
            EditorUtility.SetDirty(owner);
        }
    }
}
#endif