using UnityEngine;

public class RandomCondition : AttackCondition
{
    // ���� Ű
    public int id;      
    public float frequency = 1.0f;      // �ֱ�
    [Range(0, 1)]
    public float activePercent = 0;     // Ȯ��

    // �� �����Ӹ��� ȣ���
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
