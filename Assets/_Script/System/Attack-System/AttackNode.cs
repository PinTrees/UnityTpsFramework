using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

// Legacy System
// �������� ����Ʈ������ ����
public enum AttackDirectionType
{
    None,

    Right,
    Left,
    Back,

    Up,
    Down,
}

// Legacy System
[Serializable]
public class VfxSlashEvent
{
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData vfxPlayNormalizeTime;
    public GameObject vfxObject;
    public Vector3 offsetLocalPosition;
    public Vector3 offsetLocalRotation;

    // Scale
    public bool useOverrideScale;
    public Vector3 overrideScale = Vector3.one;

    // Arc
    public bool useOverrideArc;
    public float overrideArcAngle = 180;

    // Emission
    public bool useOverrideEmission;
    public float overrideEmission;
    
    // Alpha
    public bool useOverrideAlpha;
    [Range(0, 1)]
    public float overrideAlpha = 1;
}

// Legacy System
[Serializable]
public class CameraShakeEvent
{
    [Range(0, 1)]
    public float eventNormalizeTime = 0.5f;
    public CameraShakeData shakeData = new() 
    {
        positionIntensity = 0.5f,
        rotationIntensity = 1.0f,
        frequency = 5.0f,
    }; 
    public float duration = 0.5f;
}

[Serializable]
public class AttackerTransformSetting
{
    public bool useAttackerTransform;
    public bool lookAtTarget;
    public float distanceFromTarget;
    public float distanceFromTargetOver;
    public float transitionDuration;
    public float moveSpeedPerSec;

    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData canMoveableNormalizeTime;
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData canRotateableNormalizeTime; 
}

public enum SuperAmorType
{
    None,
    SuperAmor,
}
[Serializable]
public class SuperAmorSetting
{
    public bool useSuperAmor;
    public SuperAmorType superAmorType;
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData superAmorNormalizeTime;
}

[Serializable]
public class AttackNode : ScriptableObject
{
    [UID]
    public string uid;
    public int priorityRank = 0;

    [Space]
    [HideInInspector]
    public List<AttackCondition> conditions = new();
    public List<AttackEventContainer> events = new();
    public List<VfxSlashEvent> vfxEvents = new();
    public List<CameraShakeEvent> cameraEvents = new();
    public HitboxDataTree hitboxTree = new();

    [Header("Attack Data Setting")]
    public AttackDirectionType attackDirectionType;

    [Header("Attacker Transform Setting")]
    public AttackerTransformSetting attackerTransformSetting;

    [Header("Attacker Hittarget Setting")]
    public bool canJustDodge;
    public float canJustDodgeDistance;
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData canJustDodgeNormailzetime;

    [Header("Animation Setting")]
    public bool useRootMotion = true;
    public bool useLegIK = true;
    public AnimationClip animationClip;
    public float animationSpeed = 1;
    
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData animationPlayNormailzeTime;
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData nextAttackNormalizeTime;

    [Header("Time Effect Setting")]
    public bool useTimeScale;
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData timeScaleNormailzeTime;
    public float timeScale = 1;

    [Header("Super Armor Setting")]
    public SuperAmorSetting superArmorSetting;


    public void Init()
    {
        events.ForEach(e => e.Init());
    }

    public bool CanAttack(CharacterActorBase owner)
    {
        foreach(AttackCondition condition in conditions)
        {
            if(condition.Check(owner) == false)
                return false;
        }

        return true;
    }

    public void UpdateEvent(CharacterActorBase owner)
    {
        events.ForEach(e =>
        {
            e.UpdateEvent(this, owner);
        });
    }

    public void Enter(CharacterActorBase owner)
    {
        if (useTimeScale)
            UpdateTimeScleEffect(owner).Forget();
    }
    public void Exit(CharacterActorBase owner)
    {
        Time.timeScale = 1;
    }

    public async UniTaskVoid UpdateTimeScleEffect(CharacterActorBase owner)
    {
        bool isTimeScaled = false;

        while(true)
        {
            await UniTask.Yield();

            if(!isTimeScaled && owner.animator.IsPlayedOverTime(timeScaleNormailzeTime.start))
            {
                isTimeScaled = true;
                Time.timeScale = timeScale;
            }

            if(isTimeScaled && owner.animator.IsPlayedOverTime(timeScaleNormailzeTime.exit))
            {
                Time.timeScale = 1;
                return;
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AttackNode))]
public class AttackNodeEditor : Editor
{
    private AttackNode attackNode;
    private List<Type> conditionTypes;
    private ReorderableList conditionList;

    private List<Type> eventTypes;
    private ReorderableList eventList;

    private void OnEnable()
    {
        attackNode = (AttackNode)target;
        conditionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(AttackCondition)) && !type.IsAbstract)
            .ToList();

        eventTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(AttackEvent)) && !type.IsAbstract)
            .ToList();
        
        // ReorderableList �ʱ�ȭ
        conditionList = new ReorderableList(serializedObject, serializedObject.FindProperty("conditions"), true, true, true, true);
        eventList = new ReorderableList(serializedObject, serializedObject.FindProperty("events"), true, true, true, true);

        // Ŀ���� ��Ÿ�� ����
        GUIStyle headerStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 12, // ��� �ؽ�Ʈ ũ�� ����
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft
        };

        // ����Ʈ�� ��� ����
        conditionList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Attack Conditions", headerStyle);
        };
        eventList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Attack Events", headerStyle);
        };

        float elementSpacing = 2f; // ��� ���� ����

        // ����� ���̸� ���� (�⺻ ���� + ����)
        conditionList.elementHeightCallback = (index) =>
        {
            return EditorGUIUtility.singleLineHeight + elementSpacing;
        };
        eventList.elementHeightCallback = (index) =>
        {
            return EditorGUIUtility.singleLineHeight;
        };

        // ����Ʈ�� �� ��� �׸���
        conditionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            rect.y += elementSpacing / 2; // ��� ��� ����
            rect.height -= elementSpacing; // ��� �ϴ� ������ ������ ���� ���̱�

            var element = conditionList.serializedProperty.GetArrayElementAtIndex(index);
            var condition = element.objectReferenceValue as AttackCondition;

            if (condition != null)
            {
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            }
        };

         // ����Ʈ���� + ��ư Ŭ�� �� ����
         conditionList.onAddCallback = (ReorderableList list) =>
         {
            GenericMenu menu = new GenericMenu();
            foreach (var type in conditionTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    var condition = CreateInstance(type) as AttackCondition;
                    condition.name = attackNode.name + $" ({type.Name})";
                    condition.hideFlags = HideFlags.None;

                    // Add the condition as a sub-asset to the AttackNode
                    AssetDatabase.AddObjectToAsset(condition, attackNode);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    attackNode.conditions.Add(condition);
                    EditorUtility.SetDirty(attackNode);
                });
            }
            menu.ShowAsContext();
        };

        // ����Ʈ���� - ��ư Ŭ�� �� ����
        conditionList.onRemoveCallback = (ReorderableList list) =>
        {
            // ������ ����Ʈ���� �����ϰ�, �ش� ������ ����
            var element = list.serializedProperty.GetArrayElementAtIndex(list.index);
            if (element != null && element.objectReferenceValue != null)
            {
                var condition = element.objectReferenceValue;
                attackNode.conditions.RemoveAt(list.index);
                DestroyImmediate(condition, true);  // ������ ������Ʈ���� ����
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(attackNode);
            }
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(18);
        conditionList.DoLayoutList();
        EditorGUILayout.Space();
        eventList.DoLayoutList();
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.Space();
        GUILayout.EndVertical();

        // ���� ������ ����
        if (GUI.changed)
        {
            EditorUtility.SetDirty(attackNode);
        }
    }
}
#endif