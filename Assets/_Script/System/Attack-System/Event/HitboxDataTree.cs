using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Reflection;
using UnityEngine.Pool;



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

    [Header("BuffDebuff Setting")]
    [HideInInspector]
    public List<BuffDebuffEventBase> buffDebuffEvents;


    public void Enter(CharacterActorBase owner, string attackUid)
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

        for(int i = 0; i < hitboxTimelines.Count; ++i)
        {
            UpdateHitbox(hitboxTimelines[i], owner, attackUid);
        }
    }

    private void UpdateHitbox(HitboxTimeline timeline, CharacterActorBase owner, string attackUid)
    {
        bool isStart = false;
        bool isUpdate = false;
        int index = hitboxTimelines.IndexOf(timeline);
        hitboxs[index].detectorSetting.detectLayer = targetLayerMask;

        List<CharacterActorBase> hitTargets = ListPool<CharacterActorBase>.Get();

        TaskSystem.CoroutineUpdateLost(() =>
        {
            if (!isStart && owner.animator.IsPlayedOverTime(attackUid, timeline.hitboxSpawnNormailzeTime.start))
            {
                hitboxs[index].Enter();
                isStart = true;
            }

            if (isStart)
            {
                hitboxs[index].FindHit(owner, this, hitTargets);
                isUpdate = true;        // �ּ� 1������ ���� ����

                if (hitTargets != null && hitTargets.Count > 0)
                {
                    foreach (var target in hitTargets)
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

                        if (!target.CanHit())
                            continue;

                        target.OnHit(new HitData()
                        {
                            hitDirection = hitboxs[index].hitDirectionType,
                            hitEndMotionType = hitEndMotionType,
                            ownerCharacter = owner,
                            customHitSetting = customHitSetting,
                            knockbackSetting = knockbackSetting,
                            buffDebuffEvents = buffDebuffEvents,
                        });
                        owner.targetController.hitTargets.Add(target);
                    }
                }
            }

            if (isStart && isUpdate && owner.animator.IsPlayedOverTime(attackUid, timeline.hitboxSpawnNormailzeTime.exit))
            {
                hitboxs[index].Exit();
                return true;
            }

            return false;
        }, delay: 0.05f,
        timeout: 5, timeoutAction: () => 
        {
            hitboxs[index].Exit();
        }, onExit: () =>
        {
            ListPool<CharacterActorBase>.Release(hitTargets);
        });
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