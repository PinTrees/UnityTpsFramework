using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIObjectBase : MonoBehaviour
{
    [SerializeField] 
    public GameObject baseObject;

    public RectTransform rectTransform { get; private set; }
    public Canvas canvas { get; private set; }

    [field: SerializeField]
    public UIObjectBase parent { get; private set; }

    [HideInInspector] 
    public List<UIObjectBase> children = new();

    public bool HasChildren() => children.Count > 0;
    public void SetRectTransform(RectTransform rectTransform) { this.rectTransform = rectTransform; }

    bool _init = false;
    bool _isInitializing = false;

    public bool isPointFocus = false;


    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        Init();
    }

    /// <summary>
    /// 명시적으로 호줄하지 않을 시 Start에서 자동으로 호출됨
    /// </summary>
    public void Init()
    {
        if (_isInitializing)
        {
            _init = true;
            _isInitializing = false;
            //Debug.Log("무한 재귀 오류.");
            return;
        }
        if (_init)
            return;

        if(baseObject == null)
            baseObject = this.gameObject;

        if (canvas == null)
            canvas = baseObject.GetComponentInParent<Canvas>();

        if (rectTransform == null)
            rectTransform = baseObject.GetComponent<RectTransform>();
        
        if(rectTransform == null)
            rectTransform = baseObject.AddComponent<RectTransform>();

        OnInit();

        _init = true;
        _isInitializing = false;
    }

    /// <summary>
    /// 재정의 가능 조기화 메서드
    /// </summary>
    protected virtual void OnInit()
    {
        _isInitializing = true;
    }


    public bool IsShowed()
    {
        if (baseObject == null)
            return true;

        return baseObject.activeSelf;
    }


    public virtual void Show()
    {
        if (!_init)
            Init();

        if (baseObject == null)
            return;

        baseObject.SetActive(true);
    }

    public virtual void Close()
    {
        if (!_init)
            Init();

        if (baseObject == null)
            return;

        baseObject.SetActive(false);
    }

    public virtual void AddChildren(List<UIObjectBase> uis)
    {
        foreach(var ui in uis)
        {
            ui.transform.SetParent(transform, true);
            ui.transform.localScale = Vector3.one;
            ui.transform.localPosition = Vector3.zero;
            ui.parent = this;

            children.Add(ui);  
        }
    }
    public virtual void AddChild(UIObjectBase ui)
    {
        ui.transform.SetParent(transform, true);
        ui.transform.localScale = Vector3.one;
        ui.transform.localPosition = Vector3.zero;
        ui.parent = this;

        children.Add(ui);
    }

    protected void OnUpdateParent()
    {
        Init();

        if (baseObject.transform.parent != null)
            parent = baseObject.transform.parent.GetComponent<UIObjectBase>();
        else
            parent = null;
    }


    protected virtual void LateUpdate()
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null))
        {
            if (!isPointFocus)
            {
                isPointFocus = true;
                OnMouseRectEnter();
            }
        }
        else
        {
            if (isPointFocus)
            {
                isPointFocus = false;
                OnMouseRectExit();
            }
        }
    }

    protected virtual void OnMouseRectEnter()
    {
        //Debug.Log("Mouse entered the rect boundary");
    }

    protected virtual void OnMouseRectExit()
    {
        //Debug.Log("Mouse exited the rect boundary");
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(UIObjectBase), true)]
public class UIObjectBaseEditor : Editor
{
    UIObjectBase owner;

    public void OnEnable()
    {
        owner = target as UIObjectBase; 
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI(); 

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Show"))
        {
            owner.Show();
            Debug.Log("Show"); 
        }
        if (GUILayout.Button("Close"))
        {
            owner.Close();
            Debug.Log("Close");
        }

        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck()) 
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif