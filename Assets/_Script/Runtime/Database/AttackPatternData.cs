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
        // 1. GenericMenu 생성
        GenericMenu menu = new GenericMenu();

        // 2. AttackCondition을 상속받은 모든 클래스 찾기
        List<Type> conditionTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(AttackCondition)) && !type.IsAbstract)
            .ToList();

        // 3. 컨텍스트 메뉴에 조건 추가
        foreach (var conditionType in conditionTypes)
        {
            menu.AddItem(new GUIContent(conditionType.Name), false, () =>
            {
                // 4. 선택된 조건을 서브 에셋으로 생성
                CreateSubAsset(conditionType);
            });
        }

        // 메뉴를 호출한 위치에 컨텍스트 메뉴 표시
        menu.ShowAsContext();
    }

    // 서브 에셋 생성 함수
    private void CreateSubAsset(Type conditionType)
    {
        // 새로운 서브 에셋 생성
        var conditionInstance = ScriptableObject.CreateInstance(conditionType) as AttackCondition;
        conditionInstance.name = conditionType.Name; // 생성된 에셋 이름 설정

        // 현재 ScriptableObject (AttackNode)에 서브 에셋으로 추가
        AssetDatabase.AddObjectToAsset(conditionInstance, this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 프로젝트 뷰에서 변경 사항 반영
        EditorUtility.SetDirty(this);
    }
#endif
}


