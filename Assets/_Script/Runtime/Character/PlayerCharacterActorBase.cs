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


    protected override void OnInit()
    {
        base.OnInit();

        lockOnController = gameObject.GetComponent<LockOnController>();
        lockOnController.Init(this);
        attackController = baseObject.GetOrAddComponent<PlayerAttackController>();
        attackController.Init(this);

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

        var movementLayer = fsmContext.CreateLayer(PlayerFsmLayer.MovementLayer);
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

    public override void OnHit(HitData hitData)
    {
        base.OnHit(hitData);

        if (IsDodge)
            return;
        if (IsJustDodge)
            return;

        Debug.Log("Player Hit");
        IsHit = true;
        fsmContext.ChangeStateNow(PlayerFsmLayer.HitLayer, PlayerHitStateType.HitHard, new HitStateData()
        {
            hitData = hitData,
        });

        //if (hitData.customHitSetting.useCustomHit)
        //{
        //    fsmContext.ChangeStateNowAsync(PlayerFsmLayer.HitLayer, PlayerHitStateType.Custom, new HitStateData()
        //    {
        //        hitData = hitData,
        //    }).Forget();
        //}
        //else
        //{
        //    fsmContext.ChangeStateNowAsync(PlayerFsmLayer.HitLayer, PlayerHitStateType.HitHard, new HitStateData()
        //    {
        //        hitData = hitData,
        //    }).Forget();
        //}
    }
}
