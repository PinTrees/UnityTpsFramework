
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public class InputCondition : AttackCondition
{
    [Header("Key Datas")]
    public List<KeyCode> inputDownKey = new();
    public List<KeyCode> inputUpKey = new();
    public List<KeyCode> inputHoldKey = new();

    // ĳ�̵� Ű ����
    private Dictionary<KeyCode, bool> downKeyState = new();
    private Dictionary<KeyCode, bool> upKeyState = new();
    private Dictionary<KeyCode, bool> holdKeyState = new();


    public override bool Check(CharacterActorBase owner) 
    {
        if (base.Check(owner))
            return true;

        UpdateKeyStates();

        // �Էµ� ��� ������ Ȯ���մϴ�.
        bool downCheck = inputDownKey.TrueForAll(key => downKeyState.TryGetValue(key, out bool state) && state);
        bool upCheck = inputUpKey.TrueForAll(key => upKeyState.TryGetValue(key, out bool state) && state);
        bool holdCheck = inputHoldKey.TrueForAll(key => holdKeyState.TryGetValue(key, out bool state) && state);

        return downCheck && upCheck && holdCheck;
    }

    private void UpdateKeyStates()
    {
        // KeyDown ���� ������Ʈ
        foreach (var key in inputDownKey)
        {
            downKeyState[key] = Input.GetKeyDown(key);
        }

        // KeyUp ���� ������Ʈ
        foreach (var key in inputUpKey)
        {
            upKeyState[key] = Input.GetKeyUp(key);
        }

        // KeyHold ���� ������Ʈ
        foreach (var key in inputHoldKey)
        {
            holdKeyState[key] = Input.GetKey(key);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(InputCondition))]
public class InputConditionEditor : Editor
{
    public InputCondition owner;

    private readonly KeyCode[] commonKeys =
    {
        KeyCode.Mouse0,
        KeyCode.Mouse1,
        KeyCode.W,
        KeyCode.A,
        KeyCode.S,
        KeyCode.D,
        KeyCode.Space,
        KeyCode.LeftShift,
        KeyCode.LeftControl
    };
    private const int buttonWidth = 80;
    private const int buttonsPerRow = 4;

    public void OnEnable()
    {
        owner = (InputCondition)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();


        EditorGUILayout.Space();

        GUILayout.Label("Down Key - Hot", EditorStyles.boldLabel);

        for (int i = 0; i < commonKeys.Length; i++)
        {
            if (i % buttonsPerRow == 0)
            {
                EditorGUILayout.BeginHorizontal();
            }

            if (GUILayout.Button(commonKeys[i].ToString(), GUILayout.Width(buttonWidth)))
            {
                if (!owner.inputDownKey.Contains(commonKeys[i]))
                {
                    owner.inputDownKey.Add(commonKeys[i]);
                }
            }

            if ((i + 1) % buttonsPerRow == 0 || i == commonKeys.Length - 1)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();
        GUILayout.Label("Hold Key - Hot", EditorStyles.boldLabel);

        // �׸��� ���̾ƿ����� ��ư ǥ��
        for (int i = 0; i < commonKeys.Length; i++)
        {
            if (i % buttonsPerRow == 0)
            {
                EditorGUILayout.BeginHorizontal();
            }

            if (GUILayout.Button(commonKeys[i].ToString(), GUILayout.Width(buttonWidth)))
            {
                if (!owner.inputHoldKey.Contains(commonKeys[i]))
                {
                    owner.inputHoldKey.Add(commonKeys[i]);
                }
            }

            if ((i + 1) % buttonsPerRow == 0 || i == commonKeys.Length - 1)
            {
                EditorGUILayout.EndHorizontal();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif