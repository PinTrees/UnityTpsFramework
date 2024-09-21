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

        // ReorderableList �ʱ�ȭ
        attackNodeList = new ReorderableList(serializedObject,
            serializedObject.FindProperty("attackNodes"),
            true, true, true, true);

        // Ŀ���� ��Ÿ�� ����
        GUIStyle headerStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12, // ��� �ؽ�Ʈ ũ�� ����
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft
        };

        // ����Ʈ�� ��� ����
        attackNodeList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Attack Nodes", headerStyle);
        };

        float elementSpacing = 2f; // ��� ���� ����

        // ����� ���̸� ���� (�⺻ ���� + ����)
        attackNodeList.elementHeightCallback = (index) =>
        {
            return EditorGUIUtility.singleLineHeight + elementSpacing;
        };

        // ����Ʈ�� �� ��� �׸���
        attackNodeList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += elementSpacing / 2; // ��� ��� ����
            rect.height -= elementSpacing; // ��� �ϴ� ������ ������ ���� ���̱�

            var element = attackNodeList.serializedProperty.GetArrayElementAtIndex(index);
            var attackNode = element.objectReferenceValue as AttackNode;

            if (attackNode != null)
            {
                // Display the rest of the fields if necessary
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            }
        };

        // ����Ʈ���� + ��ư Ŭ�� �� ����
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

        // ����Ʈ���� - ��ư Ŭ�� �� ����
        attackNodeList.onRemoveCallback = (ReorderableList list) =>
        {
            // ������ ����Ʈ���� �����ϰ�, �ش� ������ ����
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

        // ���� ������ ����
        if (GUI.changed)
        {
            EditorUtility.SetDirty(owner);
        }
    }
}
#endif