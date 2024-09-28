using Fsm;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum FsmStateChangeType
{
    None,
    Wait,
    Forced,
}

public class FsmObjectBase : MonoBehaviour
{
    public FsmStateChangeType stateChangeType;
    [HideInInspector] public FsmContext fsmContext;

    [SerializeField] public GameObject baseObject;

    [Header("Components")]
    [HideInInspector] public Animator animator;
    [HideInInspector] public CapsuleCollider characterCollider; 

    public bool _isFsmInitialized { get; private set; }


    /// <summary>
    /// FSM을 초기화하는 메소드입니다.
    /// 명시적으로 초기화를 해주어야 합니다.
    /// </summary>
    public void Init()
    {
        if (_isFsmInitialized) return;
        _isFsmInitialized = true;

        fsmContext = new FsmContext();
        fsmContext.Initialize(this);

        if (baseObject == null)
            baseObject = gameObject;

        baseObject.TryGetComponent(out animator);
        characterCollider = baseObject.GetComponentInChildren<CapsuleCollider>();

        OnInit();
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
        var fsmObject = target as FsmObjectBase;

        if (fsmObject.fsmContext != null)
        {
            foreach (var layer in fsmObject.fsmContext.layers)
            {
                string stateChange = "";
                string currentState = layer.Value.currentState == null ? "NULL" : layer.Value.currentState.stateId;
                layer.Value.GetStateChangeList().ForEach(e => stateChange += e.type + ", ");
                EditorGUILayout.LabelField($"{layer.Key}: ({layer.Value.previousStateType} -> {currentState}),({stateChange}) [{layer.Value.isChangeStateLocked}]" +
                    $"\nLastUpdate: [{layer.Value.lastStateUpdateTime}], [{layer.Value.lastStateChangeUpdateTime}]"
                , EditorStyles.helpBox);
            }

            string stateChangeHitsory = "";
            fsmObject.fsmContext.stateChangeHistory.ForEach(e =>
            {
                stateChangeHitsory += $"{e} >";
            });
            EditorGUILayout.LabelField($"[{stateChangeHitsory}]", EditorStyles.helpBox);
        }

        // 기본 인스펙터 렌더링
        EditorGUILayout.Space();
        DrawDefaultInspector();
    }
}
#endif