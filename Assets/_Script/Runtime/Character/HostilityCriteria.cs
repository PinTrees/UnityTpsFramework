using UnityEngine;
using System;

[Serializable]
public class HostilityCriteria 
{
    public float Morality;              // 도덕성: -1(악)에서 1(선)
    public float Theft;                 // 절도: 0에서 1
    public float RacialBias;            // 종족적 편견: 0에서 1
    public float SocialStatus;          // 사회적 지위: 0에서 1
    public float PoliticalRelation;     // 정치적 관계: -1(적대)에서 1(동맹)
    public float PersonalGrudge;        // 개인적 원한: 0에서 1
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
        // 각각의 요소에 가중치를 부여하여 총 적대감을 계산
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