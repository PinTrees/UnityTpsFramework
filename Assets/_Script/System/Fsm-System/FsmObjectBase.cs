using Fsm;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FsmObjectBase : MonoBehaviour
{
    public FsmContext fsmContext;

    [SerializeField] public GameObject baseObject;

    [Header("Components")]
    [HideInInspector] public Animator animator;
    [HideInInspector] public NavMeshAgent navMeshAgent;
    [HideInInspector] public CapsuleCollider characterCollider; 

    public bool _isFsmInitialized { get; private set; }


    /// <summary>
    /// FSM을 초기화하는 메소드입니다.
    /// 명시적으로 초기화를 해주어야 합니다.
    /// </summary>
    public void Init()
    {
        if (_isFsmInitialized) return;

        fsmContext = new FsmContext();
        fsmContext.Initialize(this);

        if (baseObject == null)
            baseObject = gameObject;

        baseObject.TryGetComponent(out navMeshAgent); 
        baseObject.TryGetComponent(out animator);
        characterCollider = baseObject.GetComponentInChildren<CapsuleCollider>();

        OnInit();

        _isFsmInitialized = true;
    }

    // 재정의 가능 초기화자
    protected virtual void OnInit()
    {
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
        if (_isFsmInitialized)
        {
            fsmContext.Update();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (_isFsmInitialized)
        {
            fsmContext.FixedUpdate();
        }
    }

    protected virtual void LateUpdate()
    {
        if (_isFsmInitialized)
        {
            fsmContext.LateUpdate();
        }
    }
}




#if UNITY_EDITOR
[CustomEditor(typeof(FsmObjectBase), true)]
public class FsmObjectBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 렌더링
        DrawDefaultInspector();

        EditorGUILayout.Space();

        var fsmObject = target as FsmObjectBase;

        var fsmDebug = "";

        if(fsmObject.fsmContext != null)
        {
            foreach (var layer in fsmObject.fsmContext.layers)
            {
                fsmDebug += layer.Key + ": ";
                if (layer.Value.currentState != null)
                    fsmDebug += layer.Value.currentState.stateId;
                fsmDebug += "\n";
            }
        }
       
        EditorGUILayout.LabelField("FsmObjectBase\n\n" + fsmDebug, EditorStyles.helpBox);
    }
}
#endif