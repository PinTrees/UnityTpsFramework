using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class NpcFsmLayer
{
    public const string BodyLayer = "BodyLayer";
    public const string MovementLayer = "MovementLayer";
    public const string DodgeLayer = "DodgeLayer";
    public const string HitLayer = "HitLayer";
    public const string AttackLayer = "AttackLayer";
}

public class NpcCharacterActorBase : CharacterActorBase
{
    [Header("Controller Setting")]
    public NpcCombatController combatController;

    [Header("Runtime Value")]
    public bool IsTrace;
    private UI_Npc_Indicator indicator;
    private NpcCharacterData npcCharacterData;


    protected override void OnInit()
    {
        base.OnInit();

        npcCharacterData = GetData<NpcCharacterData>();

        IniFsm();
        InitUI();
        InitController();
    }

    private void IniFsm()
    {
        var bodyLayer = fsmContext.CreateLayer(NpcFsmLayer.BodyLayer);
        bodyLayer.AddState(new NpcBodyState_Stand());
        bodyLayer.AddState(new NpcBodyState_Crouch());
        bodyLayer.AddState(new NpcBodyState_Crawl());
        bodyLayer.AddState(new NpcBodyState_Death());
        bodyLayer.AddState(new NpcBodyState_KnockDown_Up());
        bodyLayer.AddState(new NpcBodyState_KnockDownToStand());

        var dodgeLayer = fsmContext.CreateLayer(NpcFsmLayer.DodgeLayer);
        dodgeLayer.AddState(new NpcDodgeState_None());
        dodgeLayer.AddState(new NpcDodgeState_ConfrontingDodge());

        var hitLayer = fsmContext.CreateLayer(NpcFsmLayer.HitLayer);
        hitLayer.AddState(new NpcHitState_None());
        hitLayer.AddState(new NpcHitState_HitHard());
        hitLayer.AddState(new NpcHitState_Custom());

        var movementLayer = fsmContext.CreateLayer(NpcFsmLayer.MovementLayer);
        movementLayer.AddState(new NpcMovementState_None());
        movementLayer.AddState(new NpcMovementState_Idle());
        movementLayer.AddState(new NpcMovementState_Walk());
        // Combat Movement State
        movementLayer.AddState(new NpcMovementState_Trace());
        movementLayer.AddState(new NpcMovementState_Confronting());
        movementLayer.AddState(new NpcMovementState_ConfrontingTrace());
        movementLayer.AddState(new NpcMovementState_RunToAttack());

        var attackLayer = fsmContext.CreateLayer(NpcFsmLayer.AttackLayer);
        attackLayer.AddState(new NpcAttackState_None());
        attackLayer.AddState(new NpcAttackState_Attack());

        fsmContext.ChangeStateNow(NpcFsmLayer.BodyLayer, NpcBodyStateType.Stand);
        fsmContext.ChangeStateNow(NpcFsmLayer.DodgeLayer, NpcDodgeStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.HitLayer, NpcHitStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.AttackLayer, NpcAttackStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.None);

        healthController.Init(this, 1000);
    }

    private void InitUI()
    {
        // Default Indicator
        if(indicatorTransform != null)
        {
            indicator = UISystemManager.Instance.Create<UI_Npc_Indicator>("Npc Indicator");
            indicator.Show(this);
        }

        // Boss Indicator
        var isMatch = characterTags.Where(e => e.tag == "Boss").FirstOrDefault();
        if(isMatch != null)
        {

        }
    }

    private void InitController()
    {
        combatController = baseObject.GetComponent<NpcCombatController>();
        combatController.Init(this);

        npcCharacterData.equipOnWeaponDatas.ForEach(e =>
        {
            var weapon = e.weaponData.Create();
            weaponController.EquipWeapon(weapon, e.parentBone);
        });
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override bool CanMove()
    {
        if (IsTrace) return false;
        if (IsDeath) return false;
        if (IsHit) return false;
        if (IsAttack) return false;
        if (IsKnockDown) return false;
        if (IsCanNotMove) return false;
        return true;
    }
    public override bool CanHit()
    {
        if (IsDeath) return false;
        if (IsDodge) return false;
        return true;
    }

    public override void OnIdle()
    {
        fsmContext.ChangeStateNow(NpcFsmLayer.HitLayer, NpcHitStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.Idle);
    }

    public override void OnKnockDown(KnockDownMotionType motionType)
    {
        base.OnKnockDown(motionType);

        IsKnockDown = true;
        fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.None);

        if (motionType == KnockDownMotionType.KnockDown_Up)
        {
            fsmContext.ChangeStateNow(NpcFsmLayer.BodyLayer, NpcBodyStateType.KnockDown_Up);
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();

        if (IsDeath)
            return;

        IsDeath = true;
        fsmContext.ChangeStateNow(NpcFsmLayer.HitLayer, NpcHitStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.DodgeLayer, NpcDodgeStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.AttackLayer, NpcAttackStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.BodyLayer, NpcBodyStateType.Death);

        indicator?.Close();
    }

    public override bool OnConfrontingTrace()
    {
        if (!base.OnConfrontingTrace())
            return false;

        var movementLayer = fsmContext.FindLayer(NpcFsmLayer.MovementLayer);
        if (movementLayer.ContainsState(NpcMovementStateType.ConfrontingTrace))
            return false;

        IsConfrontingTrace = true;
        movementLayer.ChangeStateNow(NpcMovementStateType.ConfrontingTrace);

        return true;
    }

    public void CancleAttack()
    {
        var target = targetController.forcusTarget;
        IsAttack = false;
        IsReadyToAttack = false;
        target.targetController.activeAttackers.Remove(this);
        fsmContext.ChangeStateNow(NpcFsmLayer.AttackLayer, NpcAttackStateType.None);
    }

    public override void OnAttack()
    {
        base.OnAttack();
        
        IsAttack = true;
        IsReadyToAttack = false;
        fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.AttackLayer, NpcAttackStateType.Attack);
    }

    public override void OnRunToAttack()
    {
        base.OnRunToAttack();

        if (IsDeath)
        {
            CancleAttack();
            return;
        }

        if(combatController.currentAttackCombo == null)
        {
            CancleAttack();
            return;
        }

        targetController.lockDetectUpdate = true;
        IsRunToAttack = true;
        fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.RunToAttack);
    }

    public override async void OnHit(HitData data)
    {
        base.OnHit(data);

        healthController.TakeDamage(new vDamage()
        {
            damage = 250,
        });

        if(IsDeath)
            return;
        if (IsKnockDown)
            return;

        IsHit = true;
        fsmContext.ChangeStateNow(NpcFsmLayer.AttackLayer, NpcAttackStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.None);

        if (data.customHitSetting.useCustomHit)
        {
            fsmContext.ChangeStateNow(NpcFsmLayer.HitLayer, NpcHitStateType.Custom, new HitStateData()
            {
                hitData = data,
            });
        }
        else
        {
            fsmContext.ChangeStateNow(NpcFsmLayer.HitLayer, NpcHitStateType.HitHard, new HitStateData()
            {
                hitData = data,
            });
        }

        await PostProcessingManager.Instance.ChromaticAberration.CrossFadeIntensity(0.5f, 0.15f);
        await PostProcessingManager.Instance.ChromaticAberration.CrossFadeIntensity(0.0f, 0.5f);
    }
}
