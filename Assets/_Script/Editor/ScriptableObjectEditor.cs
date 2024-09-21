#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableObject), true)] // ��� ScriptableObject�� ���� ����
public class ScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // ScriptableObject�� ���� ��ư�� �߰�
        ScriptableObject scriptableObject = (ScriptableObject)target;

        if (GUILayout.Button("Locate in Project"))
        {
            // ������ ��ġ�� �̵�
            EditorGUIUtility.PingObject(scriptableObject);
            Selection.activeObject = scriptableObject;
        }

        // �⺻ �ν����� �׸���
        base.OnInspectorGUI();
    }
}
#endif