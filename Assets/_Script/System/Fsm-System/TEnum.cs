using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TEnum
{
    public string Name { get; private set; }
    public int Value { get; private set; }


    public TEnum() 
    {
    }

    public TEnum(string name, int value)
    {
        Name = name;
        Value = value;
    }

    public override string ToString() => Name;
}