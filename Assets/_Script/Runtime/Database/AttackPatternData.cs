using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class AttackComboData
{
    public List<AttackCondition> conditions; 
    public List<AttackNode> attackNodes;
}

[CreateAssetMenu(menuName = "Scriptable/Database/AttackPattern")]
public class AttackPatternData : ScriptableObject
{
    public List<AttackComboData> attackCombos;

    public AttackComboData FindAttackCombo(CharacterActorBase owner)
    {
        var targetAttackCombo = attackCombos.Where(e => e.attackNodes.First().CanAttack(owner))
                              .OrderBy(a => a.attackNodes.First().priorityRank)
                              .LastOrDefault();
        return targetAttackCombo;
    }

    public AttackNode FindAttack(CharacterActorBase owner)
    {
        var targetAttackCombo = attackCombos.Where(e => e.attackNodes.First().CanAttack(owner))
                             .OrderBy(a => a.attackNodes.First().priorityRank)
                             .LastOrDefault();

        if (targetAttackCombo == null)
            return null;
        return targetAttackCombo.attackNodes.First();

        //return rootAttacks.Where(e => e.CanAttack(owner))
        //                      .OrderBy(a => a.priorityRank)
        //                      .FirstOrDefault();
    }

#if UNITY_EDITOR
    [ButtonSO("Create Condition")]
    public void _Editor_CreateCondition()
    {
        // 1. GenericMenu ����
        GenericMenu menu = new GenericMenu();

        // 2. AttackCondition�� ��ӹ��� ��� Ŭ���� ã��
        List<Type> conditionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(AttackCondition)) && !type.IsAbstract)
            .ToList();

        // 3. ���ؽ�Ʈ �޴��� ���� �߰�
        foreach (var conditionType in conditionTypes)
        {
            menu.AddItem(new GUIContent(conditionType.Name), false, () =>
            {
                // 4. ���õ� ������ ���� �������� ����
                CreateSubAsset(conditionType);
            });
        }

        // �޴��� ȣ���� ��ġ�� ���ؽ�Ʈ �޴� ǥ��
        menu.ShowAsContext();
    }

    // ���� ���� ���� �Լ�
    private void CreateSubAsset(Type conditionType)
    {
        // ���ο� ���� ���� ����
        var conditionInstance = ScriptableObject.CreateInstance(conditionType) as AttackCondition;
        conditionInstance.name = conditionType.Name; // ������ ���� �̸� ����

        // ���� ScriptableObject (AttackNode)�� ���� �������� �߰�
        AssetDatabase.AddObjectToAsset(conditionInstance, this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ������Ʈ �信�� ���� ���� �ݿ�
        EditorUtility.SetDirty(this);
    }
#endif
}


