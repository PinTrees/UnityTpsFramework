using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UILayoutBase : UIObjectBase
{
    protected override void Awake()
    {

    }
    protected override void Start()
    {
        Init();
    }


    /// <summary>
    /// 재정의 가능 초기화자 입니다.
    /// </summary>
    protected override void OnInit()
    {
        base.OnInit();
    }


    protected override void LateUpdate()
    {
        OnUpdateLayout();
    }


    /// <summary>
    /// 레이아웃을 실행할 재정의 가능 메서드 입니다.
    /// </summary>
    public virtual void SetLayout()
    {
        Init();
    }

    /// <summary>
    /// </summary>
    public virtual void OnUpdateLayout()
    {
    }

    private void OnTransformChildrenChanged()
    {
        base.OnUpdateParent();

        SetLayout();
        OnUpdateLayout();
    }

    // 오버라이드 안됨
    // 최하위 객체에서 재정의
    private void OnTransformParentChanged()
    {
        base.OnUpdateParent();

        SetLayout();
        OnUpdateLayout();
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(UILayoutBase), true)]
public class UILayoutBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        /*UILayoutBase layoutBase = (UILayoutBase)target;

        if (GUILayout.Button("Init"))
        {
            layoutBase.Init();
        }
        if (GUILayout.Button("Set Layout"))
        {
            layoutBase.SetLayout();
        }*/
    }
}
#endif