using System.Collections.Generic;
using UnityEngine;

public class RandomSystem : Singleton<RandomSystem>
{
    // ���� Ű ���
    // Ʃ���� �̿��� ���� Ű
    // ���ڿ��� �̿��� ���� Ű
    // ������ ��ųʸ� ������ ���� �ذ�å
    private Dictionary<(int, int), float> randomInstanceTimeMap = new();

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    public override void Init()
    {
        base.Init();

        randomInstanceTimeMap.Clear();
    }

    public void AddInstanceTime(Object instanceObject, int id = 0)
    {
        randomInstanceTimeMap[(instanceObject.GetInstanceID(), id)] = Time.time;
    }

    public void RemoveInstanceTime(Object instanceObject, int id = 0) 
    {
        randomInstanceTimeMap.Remove((instanceObject.GetInstanceID(), id));
    }

    public bool IsInstancedTime(Object instanceObject, int id = 0)
    {
        return randomInstanceTimeMap.ContainsKey((instanceObject.GetInstanceID(), id));
    }

    public bool IsOverInstancedTime(Object instanceObject, float frequency, int id = 0)
    {
        var targetTime = randomInstanceTimeMap[(instanceObject.GetInstanceID(), id)] + frequency;
        return targetTime < Time.time;
    }
}
