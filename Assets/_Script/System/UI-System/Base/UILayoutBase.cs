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
    /// ������ ���� �ʱ�ȭ�� �Դϴ�.
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
    /// ���̾ƿ��� ������ ������ ���� �޼��� �Դϴ�.
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

    // �������̵� �ȵ�
    // ������ ��ü���� ������
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