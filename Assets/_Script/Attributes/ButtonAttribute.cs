using System;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : PropertyAttribute
{
    public string buttonText;

    public ButtonAttribute(string buttonText = null)
    {
        this.buttonText = buttonText;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtonAttributeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // 대상 객체의 타입 가져오기
        Type targetType = target.GetType();

        // 해당 객체의 모든 메서드를 가져오기
        MethodInfo[] methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        // 각 메서드에 대해
        foreach (var method in methods)
        {
            // ButtonAttribute가 적용된 메서드 찾기
            var buttonAttribute = (ButtonAttribute)Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));
            if (buttonAttribute != null)
            {
                // 버튼 텍스트 설정
                string buttonText = string.IsNullOrEmpty(buttonAttribute.buttonText) ? method.Name : buttonAttribute.buttonText;

                // 버튼 그리기
                if (GUILayout.Button(buttonText))
                {
                    // 메서드 실행
                    method.Invoke(target, null);
                }
            }
        }
    }
}
#endif