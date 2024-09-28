using Cysharp.Threading.Tasks;
using Fsm;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class PlayerFsmLayer
{
    public const string MovementLayer = "MovementLayer";
    public const string AttackLayer = "AttackLayer";
    public const string DodgeLayer = "DodgeLayer";
    public const string HitLayer = "HitLayer";
    public const string BodyLayer = "BodyLayer";
}

public class PlayerCharacterActorBase : CharacterActorBase
{
    public UserCharacterData userdata;

    [Header("Player Controller Setting")]
    public LockOnController lockOnController;
    public PlayerAttackController attackController;
    public PlayerInputController inputController;


    protected override void OnInit()
    {
        base.OnInit();

        navMeshAgent.radius = 0.01f;

        lockOnController = gameObject.GetComponent<LockOnController>();
        lockOnController.Init(this);
        attackController = baseObject.GetComponent<PlayerAttackController>();
        attackController.Init(this);
        inputController = baseObject.GetComponent<PlayerInputController>();
        inputController.Init(this);

        InitFsm();

        userdata.equipWeaponDatas.ForEach(e =>
        {
            var weapon = e.weaponData.Create();
            weaponController.EquipWeapon(weapon, e.parentBone);
        });
    }

    protected void InitFsm()
    {
        var hitLayer = fsmContext.CreateLayer(PlayerFsmLayer.HitLayer);
        hitLayer.AddState(new PlayerHitState_None());
        hitLayer.AddState(new PlayerHitState_HitLow());
        hitLayer.AddState(new PlayerHitState_HitHard());

        var dodgeLayer = fsmContext.CreateLayer(PlayerFsmLayer.DodgeLayer);
        dodgeLayer.AddState(new PlayerDodgeState_None());
        dodgeLayer.AddState(new PlayerDodgeState_Roll());
        dodgeLayer.AddState(new PlayerDodgeState_Slide());
        dodgeLayer.AddState(new PlayerDodgeState_JustDodge());

        var attackLayer = fsmContext.CreateLayer(PlayerFsmLayer.AttackLayer);
        attackLayer.AddState(new PlayerAttackState_None());
        attackLayer.AddState(new PlayerAttackState_Attack());
        attackLayer.FindState(PlayerAttackStateType.None).onUpdate = () =>
        {
            attackController.AttackUpdate();
        };

        var movementLayer = fsmContext.CreateLayer(PlayerFsmLayer.MovementLayer);
        movementLayer.AddState(new PlayerMovementState_None());
        movementLayer.AddState(new PlayerMovementState_Idle());
        movementLayer.AddState(new PlayerMovementState_Walk());
        movementLayer.AddState(new PlayerMovementState_Run());

        fsmContext.ChangeStateNow(PlayerFsmLayer.HitLayer, PlayerHitStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.AttackLayer, PlayerAttackStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.DodgeLayer, PlayerDodgeStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        float horizontalInput = Input.GetAxis("Horizontal"); 
        float verticalInput = Input.GetAxis("Vertical");   

        var currentDir = new Vector3(horizontalInput, 0, verticalInput).normalized;

        if (movementDir != Vector3.zero && currentDir == Vector3.zero)
            lastMovementDir = movementDir;
        if (currentDir != Vector3.zero)
            lastMovementDir = Vector3.zero;

        movementDir = currentDir;

        if(lastMovementDir != Vector3.zero)
        {
            animator.SetFloat("x", lastMovementDir.x);
            animator.SetFloat("y", lastMovementDir.z);
        }
        else
        {
            animator.SetFloat("x", movementDir.x);
            animator.SetFloat("y", movementDir.z);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    public override bool CanMove()
    {
        if (IsDeath) return false;
        if (IsJustDodge) return false;
        if (IsHit) return false;
        if (IsDodge) return false;
        if (IsAttack) return false;
        return true;
    }

    public bool CanDodge()
    {
        if (IsDeath) return false;
        if (IsJustDodge) return false;
        if (IsHit) return false;
        if (IsDodge) return false;
        if (IsAttack) return false;
        return true;
    }

    public bool CanAttack()
    {
        if (IsDeath) return false;
        if (IsHit) return false;
        if (IsDodge) return false;
        if (IsAttack) return false;
        if (IsJustDodge) return false;
        return true;
    }

    public override bool CanHit()
    {
        if (IsDeath) return false;
        if (IsDodge) return false;
        if (IsJustDodge) return false;
        return true;
    }

    public override void OnAttack()
    {
        base.OnAttack();

        IsAttack = true;
        fsmContext.ChangeStateNow(PlayerFsmLayer.DodgeLayer, PlayerDodgeStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.None); 
        fsmContext.ChangeStateNow(PlayerFsmLayer.AttackLayer, PlayerAttackStateType.Attack);
    }

    public void OnDodgeStop()
    {
        fsmContext.ChangeStateNow(PlayerFsmLayer.DodgeLayer, PlayerDodgeStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
    }

    public void OnAttackStop()
    {
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
        fsmContext.ChangeStateNow(PlayerFsmLayer.AttackLayer, PlayerAttackStateType.None);
    }

    public override void OnIdle()
    {
        IsIdle = true;
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
    }

    public void OnStop()
    {
        fsmContext.ChangeStateNow(PlayerFsmLayer.DodgeLayer, PlayerDodgeStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Idle);
    }

    public override void OnRun()
    {
        IsRun = true;
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Run);
    }

    public override void OnWalk()
    {
        IsWalk = true;
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.Walk);
    }

    public override void OnDodgeRoll()
    {
        IsDodge = true;
        fsmContext.ChangeStateNow(PlayerFsmLayer.AttackLayer, PlayerAttackStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.DodgeLayer, PlayerDodgeStateType.Roll);
    }

    public override void OnHit(HitData hitData)
    {
        IsHit = true;
        fsmContext.ChangeStateNow(PlayerFsmLayer.AttackLayer, PlayerAttackStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.DodgeLayer, PlayerDodgeStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.MovementLayer, PlayerMovementStateType.None);
        fsmContext.ChangeStateNow(PlayerFsmLayer.HitLayer, PlayerHitStateType.HitHard, new HitStateData()
        {
            hitData = hitData,
        });
    }
}
