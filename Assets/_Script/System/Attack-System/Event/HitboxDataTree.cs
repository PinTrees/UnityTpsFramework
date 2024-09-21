using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using Random = UnityEngine.Random;

[Serializable]
public class HitboxTimeline
{
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData hitboxSpawnNormailzeTime;
}

[Serializable]
public class HitboxCustomHitSetting
{
    public bool useCustomHit;
    public bool useRootMotion;
    public AnimationClip animationClip;
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData playNormalizeTime;
    public float animationSpeed = 1;

    [Header("Target Transform Setting - Legacy")]
    public bool lookAtAttacker;
    [Range(0, 360)]
    public float eulerAngleYOffset;
    public float distanceFromAttacker; 

    [Header("Override Animation Setting")]
    public bool useOverrideAnimation;
    public ScriptableAnimatorAvataMaskType overrideMaskType;
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData overrideIdlePlayNormalizeTime;
}

public enum HitboxKnockbackType
{
    None,
    Back,       // �ڷ� �и�
    Fly,        // ����
}

[Serializable]
public class HitboxKnockbackSetting
{
    public bool useKnockback;

    [Header("Fixed Transform Setting")]
    public bool useAttackerDirection;
    public bool useYPosition;
    public bool useXZPosition;

    [Header("Knockback Setting")]
    public HitboxKnockbackType knockbackType;
    public Vector3 knockbackAmount;
    public float duration;

    [Header("Animation Setting")]
    public bool useAnimationTime;
    [AnimationTime(0, 1)]
    public AnimationInNormalizeTimeData animationNormalizeDuration;
}

[Serializable]
public class HitData
{
    public HitDirectionType hitDirection;
    public HitboxHitEndMotionType hitEndMotionType;
    public CharacterActorBase ownerCharacter;

    public HitboxCustomHitSetting customHitSetting;
    public HitboxKnockbackSetting knockbackSetting;

    public List<BuffDebuffEventBase> buffDebuffEvents;
}

public enum HitboxHitEndMotionType
{
    None,
    LieDown_Up,         // �ϴ��� ���� ����
    LieDown_Down,       // �ٴ��� ���� ����
}

[CreateAssetMenu(menuName = "Scriptable/Database/HitboxTree")]
public class HitboxDataTree : ScriptableObject
{
    public bool onlyOneHitbox;
    public bool useFixedTarget;
    public LayerMask targetLayerMask;
    public HitboxHitEndMotionType hitEndMotionType;


    [HideInInspector]
    public List<HitboxTimeline> hitboxTimelines = new();

    [HideInInspector]
    public List<HitboxEvent> hitboxs = new();

    [Header("Knockback Setting")]
    public HitboxKnockbackSetting knockbackSetting;

    [Header("Custom Hit Setting")]
    public HitboxCustomHitSetting customHitSetting;

    [Header("Override Time Stop Setting")]
    public bool useTimeStopEvent;
    public float timeStopDuration = 0.1f;

    [Header("BuffDebuff Setting")]
    [HideInInspector]
    public List<BuffDebuffEventBase> buffDebuffEvents;


    public void Enter(CharacterActorBase owner)
    {
        owner.targetController.hitTargets.Clear();

        if (useFixedTarget)
        {
            var target = owner.targetController.forcusTarget;

            target.OnHit(new HitData()
            {
                hitDirection = HitDirectionType.None,
                ownerCharacter = owner,
                customHitSetting = customHitSetting,
            });

            return;
        }

        hitboxTimelines.ForEach(e =>
        {
            UpdateHitbox(e, owner).Forget();
        });
    }

    private async UniTask UpdateHitbox(HitboxTimeline timeline, CharacterActorBase owner)
    {
        bool isStart = false;
        bool isUpdate = false;
        int index = hitboxTimelines.IndexOf(timeline);
        hitboxs[index].detectorSetting.detectLayer = targetLayerMask;

        // ���� �ð� Ÿ�Ӿƿ� ���� (��: 5��)
        float timeoutDuration = 5f; // 5�� Ÿ�Ӿƿ�
        float elapsedTime = 0f; // ��� �ð� ����

        while (true)
        {
            await UniTask.Yield();
            elapsedTime += Time.deltaTime; 

            if (elapsedTime > timeoutDuration)
            {
                hitboxs[index].Exit();
                Debug.LogWarning("Hitbox update timed out.");
                break;
            }

            if (!isStart && owner.animator.IsPlayedOverTime(timeline.hitboxSpawnNormailzeTime.start))
            {
                hitboxs[index].Enter();
                isStart = true;
            }

            if (isStart)
            {
                var targets = hitboxs[index].FindHit(owner, this);
                isUpdate = true;        // �ּ� 1������ ���� ����

                if (targets != null && targets.Count > 0)
                {
                    foreach (var target in targets)
                    {
                        var isTagMatch = target.characterTags.Any(targetTag => owner.targetTags.Any(ownerTag => ownerTag.tag == targetTag.tag));
                        if (!isTagMatch)
                            continue;
                        if (onlyOneHitbox && owner.targetController.hitTargets.Contains(target))
                            continue;

                        var vfx = VfxObject.Create("VfxBlood2");
                        vfx.transform.position = target.GetCenterOfMass() + Random.insideUnitSphere * 0.1f;
                        vfx.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);

                        var vfxCenter = VfxObject.Create("VfxBlood7");
                        vfxCenter.transform.position = target.GetCenterOfMass() + Random.insideUnitSphere * 0.1f;
                        vfxCenter.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);

                        target.OnHit(new HitData()
                        {
                            hitDirection = hitboxs[index].hitDirectionType,
                            hitEndMotionType = hitEndMotionType,
                            ownerCharacter = owner,
                            customHitSetting = customHitSetting,
                            knockbackSetting = knockbackSetting,
                            buffDebuffEvents = buffDebuffEvents,
                        });

                        if (useTimeStopEvent)
                            TimeEx.Stop(timeStopDuration).Forget();

                        owner.targetController.hitTargets.Add(target);
                    }
                }
            }

            if (isStart && isUpdate && owner.animator.IsPlayedOverTime(timeline.hitboxSpawnNormailzeTime.exit))
            {
                hitboxs[index].Exit();
                break;
            }
        }
    }

    public void Exit()
    {
    }

#if UNITY_EDITOR
    public void _Editor_AddBuffDebuffEvent(Type type)
    {
        var newEvent = ScriptableObject.CreateInstance(type) as BuffDebuffEventBase;
        newEvent.name = type.Name;
        newEvent.hideFlags = HideFlags.None;

        if (newEvent != null)
        {
            // ���� �������� �߰�
            AssetDatabase.AddObjectToAsset(newEvent, this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            buffDebuffEvents.Add(newEvent);

            EditorUtility.SetDirty(this);
        }
        else
        {
            Debug.LogWarning("Cannot add BuffDebuffEvent to an unsaved asset.");
        }
    }

    public void DrawGizmo(CharacterActorBase ownerCharacter)
    {
        hitboxs.ForEach(e => e.OnDrawGizmo(ownerCharacter.baseObject.transform));
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(HitboxDataTree))]
public class HitboxDataTreeEditor : Editor
{
    private ReorderableList hitboxList;
    private ReorderableList timelineList;
    private ReorderableList buffDebuffList;
    private HitboxDataTree hitboxDataTree;


    private void OnEnable()
    {
        hitboxDataTree = (HitboxDataTree)target;
        hitboxList = new ReorderableList(serializedObject,
                                         serializedObject.FindProperty("hitboxs"),
                                         true, true, true, true);

        // ����Ʈ ��� ����
        hitboxList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Hitbox Events");
        };

        // ����Ʈ�� �� ��� �׸���
        hitboxList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = hitboxList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, GUIContent.none);
        };

        // ����Ʈ���� + ��ư Ŭ�� �� ����
        hitboxList.onAddCallback = (ReorderableList list) =>
        {
            // ���ο� ���� ���� ����
            HitboxEvent newHitbox = CreateInstance<HitboxEvent>();
            newHitbox.name = $"Hitbox {list.count + 1}";
            AssetDatabase.AddObjectToAsset(newHitbox, hitboxDataTree);
            AssetDatabase.SaveAssets();

            // ����Ʈ�� �߰�
            hitboxDataTree.hitboxs.Add(newHitbox);
            EditorUtility.SetDirty(hitboxDataTree);
        };

        // ����Ʈ���� - ��ư Ŭ�� �� ����
        hitboxList.onRemoveCallback = (ReorderableList list) =>
        {
            // ���� ���� ����
            var element = list.serializedProperty.GetArrayElementAtIndex(list.index).objectReferenceValue;
            hitboxDataTree.hitboxs.RemoveAt(list.index);
            DestroyImmediate(element, true); // ������Ʈ���� ����
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(hitboxDataTree);
        };




        // Timeline ����Ʈ
        timelineList = new ReorderableList(serializedObject,
                                           serializedObject.FindProperty("hitboxTimelines"),
                                           true, true, true, true);

        timelineList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Hitbox Timelines");
        };
        timelineList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = timelineList.serializedProperty.GetArrayElementAtIndex(index);
            var timeline = element.FindPropertyRelative("hitboxSpawnNormailzeTime");

            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                                    timeline, GUIContent.none);
        };
        timelineList.elementHeightCallback = (index) =>
        {
            var element = timelineList.serializedProperty.GetArrayElementAtIndex(index);
            var timeline = element.FindPropertyRelative("hitboxSpawnNormailzeTime");

            float propertyHeight = EditorGUI.GetPropertyHeight(timeline, true);
            return propertyHeight + EditorGUIUtility.standardVerticalSpacing;
        };


        buffDebuffList = new ReorderableList(serializedObject,
                                 serializedObject.FindProperty("buffDebuffEvents"),
                                 true, true, true, true);
        buffDebuffList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "BuffDebuff Events");
        };
        buffDebuffList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = buffDebuffList.serializedProperty.GetArrayElementAtIndex(index);
            var buffDebuffEvent = element.objectReferenceValue as BuffDebuffEventBase;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
        buffDebuffList.elementHeightCallback = (index) =>
        {
            var element = timelineList.serializedProperty.GetArrayElementAtIndex(index);
            return EditorGUIUtility.singleLineHeight;
        };
        buffDebuffList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
        {
            GenericMenu menu = new GenericMenu();

            // BuffDebuffEventBase�� ��ӹ��� ��� ���� Ŭ���� �˻�
            Type[] derivedTypes = Assembly.GetAssembly(typeof(BuffDebuffEventBase))
                                          .GetTypes()
                                          .Where(t => t.IsSubclassOf(typeof(BuffDebuffEventBase)) && !t.IsAbstract)
                                          .ToArray();

            foreach (var type in derivedTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => 
                {
                    hitboxDataTree._Editor_AddBuffDebuffEvent(type);
                });
            }

            menu.ShowAsContext();
        };
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        hitboxList.DoLayoutList();
        EditorGUILayout.Space();
        timelineList.DoLayoutList();
        EditorGUILayout.Space();
        buffDebuffList.DoLayoutList();
        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        // ���� ������ ����
        if (GUI.changed)
        {
            EditorUtility.SetDirty(hitboxDataTree);
        }
    }
}
#endif