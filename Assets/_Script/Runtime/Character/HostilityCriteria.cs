using UnityEngine;
using System;

[Serializable]
public class HostilityCriteria 
{
    public float Morality;              // ������: -1(��)���� 1(��)
    public float Theft;                 // ����: 0���� 1
    public float RacialBias;            // ������ ���: 0���� 1
    public float SocialStatus;          // ��ȸ�� ����: 0���� 1
    public float PoliticalRelation;     // ��ġ�� ����: -1(����)���� 1(����)
    public float PersonalGrudge;        // ������ ����: 0���� 1
}

[Serializable]
public class HostilityCriteriaWeight
{
    public float Morality;   
    public float Theft;         
    public float RacialBias;    
    public float SocialStatus;    
    public float PoliticalRelation; 
    public float PersonalGrudge; 
}

public static class HostilityCriteriaCalculate
{
    public static bool CalculateHostility(HostilityCriteria data, HostilityCriteriaWeight weight, float limitHostility)
    {
        // ������ ��ҿ� ����ġ�� �ο��Ͽ� �� ���밨�� ���
        float hostility = 0f;
        hostility += data.Morality * weight.Morality;
        hostility += data.Theft * weight.Theft;
        hostility += data.RacialBias * weight.RacialBias;
        hostility += data.SocialStatus * weight.SocialStatus;
        hostility += data.PoliticalRelation * weight.PoliticalRelation;
        hostility += data.PersonalGrudge * weight.PersonalGrudge;

        return hostility > limitHostility;
    }
}