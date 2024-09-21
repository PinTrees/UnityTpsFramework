using UnityEngine;

public class RandomCondition : AttackCondition
{
    // 복합 키
    public int id;      
    public float frequency = 1.0f;      // 주기
    [Range(0, 1)]
    public float activePercent = 0;     // 확률

    // 매 프레임마다 호출됨
    public override bool Check(CharacterActorBase owner)
    {
        if (base.Check(owner))
            return true;

        if (!RandomSystem.Instance.IsInstancedTime(owner.baseObject, id))
            RandomSystem.Instance.AddInstanceTime(owner.baseObject, id);

        if(RandomSystem.Instance.IsOverInstancedTime(owner.baseObject, frequency, id))
        {
            var randomPercent = Random.Range(0, 100) * 0.01f;
            RandomSystem.Instance.RemoveInstanceTime(owner.baseObject, id);
            if (randomPercent < activePercent)
            {
                return true;
            }
            return false;
        }

        return false;
    }
}
