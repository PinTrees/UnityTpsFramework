using Cysharp.Threading.Tasks;
using FIMSpace.FLook;
using FIMSpace.FProceduralAnimation;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class CharacterActorBase : FsmObjectBase
{
    public List<TargetGroupTag> characterTags = new();
    public List<TargetGroupTag> targetTags = new();

    // Components
    public LegsAnimator legsAnimator    { get; private set; }
    public FLookAnimator lookAnimator   { get; private set; }
    public Rigidbody rb                 { get; private set; }
    public NavMeshAgent navMeshAgent    { get; private set; }

    // Controllers
    public TargetController targetController { get; private set; }
    public WeaponController weaponController { get; private set; }
    public HealthController healthController { get; private set; }
    public BuffDebuffController buffDebuffController { get; private set; }

    private Transform lookTargetTransform;

    [Header("Data Setting")]
    public CharacterData characterData;
    public T GetData<T>() where T: CharacterData { return characterData as T; }

    [Header("Animator Setting")]
    public ScriptableAnimatorSetting scriptableAnimatorSetting;

    // Transform Setting
    [Header("Transform Setting")]
    public Transform indicatorTransform;
    public Transform lockOnTransform;

    // Runtime Value
    [Header("Runtime Value")]
    public Vector3 movementDir;
    [HideInInspector] public Vector3 lastMovementDir;

    // Status Value
    public bool IsDodge = false;
    public bool IsRun = false;
    public bool IsWalk = false;
    public bool IsIdle = false;

    public bool IsAttack = false;
    public bool IsJustDodge = false;
    public bool IsHit = false;
    public bool IsMove = false;
    public bool IsConfronting = false;
    public bool IsConfrontingTrace = false;
    public bool IsDeath = false;
    // State - BuffDebuff Value
    public bool IsSuperArmor = false;
    public bool IsSturn = false;
    public bool IsKnockDown = false;
    public bool IsCanNotMove = false;
    public bool IsReadyToAttack = false;
    public bool IsRunToAttack = false;


    protected override void OnInit()
    {
        base.OnInit();

        lookTargetTransform = new GameObject("lookTargetTransform").transform;
        lookTargetTransform.SetParent(baseObject.transform);

        navMeshAgent = baseObject.CreateChildWithComponent<NavMeshAgent>("NavMaehAgent");
        navMeshAgent.height = 1.5f;
        navMeshAgent.speed = 3.0f;
        navMeshAgent.angularSpeed = 180.0f;
        navMeshAgent.acceleration = 99.0f;
        navMeshAgent.stoppingDistance = 0.0f;
        navMeshAgent.radius = 0.5f;

        // Set Components
        {
            legsAnimator = baseObject.GetComponent<LegsAnimator>();
            lookAnimator = baseObject.GetComponent<FLookAnimator>();
            lookAnimator?.SetLookTarget(lookTargetTransform);
            rb = baseObject.GetComponent<Rigidbody>();
        }

        // Set Controllers
        {
            targetController = baseObject.GetComponent<TargetController>();
            weaponController = baseObject.GetComponent<WeaponController>();
            healthController = baseObject.GetOrAddComponent<HealthController>();
            buffDebuffController = baseObject.GetOrAddComponent<BuffDebuffController>();
        }

        // Init Controller
        {
            buffDebuffController?.Init(this);
            targetController?.Init(this);
            weaponController?.Init(this);
        }

        if (scriptableAnimatorSetting)
            animator.CrossFadeAnimatorController(scriptableAnimatorSetting.animatorController);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        healthController?.UpdateHealth();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        navMeshAgent.transform.localPosition = new Vector3(0, navMeshAgent.transform.localPosition.y, 0);
    }

    public virtual void OnKnockDown(KnockDownMotionType motionType)
    {
    }
    public virtual void OnRunToAttack()
    {
    }
    public virtual void OnAttack() { }
    public virtual void OnDodgeRoll()
    {
    }
    public virtual void OnRun()
    {
    }
    public virtual void OnWalk()
    {
    }
    public virtual void OnIdle() { }

    public virtual bool CanMove() { return false; }

    public virtual bool CanHit() { return false; }
    public virtual void OnHit(HitData hitData) { }
    
    public virtual void OnDeath()
    {
    }
    public virtual bool OnConfrontingTrace()
    {
        if (IsDeath) return false;
        if (IsHit) return false;
        if (IsConfronting) return false;
        if (IsCanNotMove) return false;
        if (IsKnockDown) return false;
        if (IsConfrontingTrace) return false;

        return true;
    }


    public Vector3 GetCenterOfMass()
    {
        Vector3 localCenterOfMass = characterCollider.bounds.center - baseObject.transform.position;
        Vector3 worldCenterOfMass = baseObject.transform.TransformPoint(localCenterOfMass);
        return worldCenterOfMass;
    }
}
