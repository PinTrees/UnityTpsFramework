using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable/Database/PatternTree")]
public class CombatPatternTree : ScriptableObject
{
    public List<AttackNode> rootAttacks = new();
}
