#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableObject), true)] // 모든 ScriptableObject에 대해 적용
public class ScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // ScriptableObject에 대해 버튼을 추가
        ScriptableObject scriptableObject = (ScriptableObject)target;

        if (GUILayout.Button("Locate in Project"))
        {
            // 에셋의 위치로 이동
            EditorGUIUtility.PingObject(scriptableObject);
            Selection.activeObject = scriptableObject;
        }

        // 기본 인스펙터 그리기
        base.OnInspectorGUI();
    }
}
#endif