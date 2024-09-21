using Cysharp.Threading.Tasks;
using Fsm;
using System.Linq;
using UnityEngine;

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

    protected override void OnInit()
    {
        base.OnInit();

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
        movementLayer.AddState(new NpcMovementState_Idle());
        movementLayer.AddState(new NpcMovementState_Walk());
        // Combat Movement State
        movementLayer.AddState(new NpcMovementState_Trace());
        movementLayer.AddState(new NpcMovementState_Confronting());
        movementLayer.AddState(new NpcMovementState_ConfrontingTrace());

        var attackLayer = fsmContext.CreateLayer(NpcFsmLayer.AttackLayer);
        attackLayer.AddState(new NpcAttackState_None());
        attackLayer.AddState(new NpcAttackState_Attack());

        fsmContext.ChangeStateNow(NpcFsmLayer.BodyLayer, NpcBodyStateType.Stand);
        fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.Idle);
        fsmContext.ChangeStateNow(NpcFsmLayer.HitLayer, NpcHitStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.AttackLayer, NpcAttackStateType.None);
        fsmContext.ChangeStateNow(NpcFsmLayer.DodgeLayer, NpcDodgeStateType.None);

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

    public override void OnDeath()
    {
        base.OnDeath();

        var bodyLayer = fsmContext.FindLayer(NpcFsmLayer.BodyLayer);
        bodyLayer.ChangeStateNow(NpcBodyStateType.Death);

        indicator?.Close();
    }

    public override bool OnConfrontingTrace()
    {
        if (!base.OnConfrontingTrace())
            return false;

        var movementLayer = fsmContext.FindLayer(NpcFsmLayer.MovementLayer);
        if (movementLayer.ContainsState(NpcMovementStateType.ConfrontingTrace))
            return false;

        Debug.Log("Npc OnConfonting Trace");
        movementLayer.ChangeStateNow(NpcMovementStateType.ConfrontingTrace);

        return true;
    }

    public override void OnHit(HitData data)
    {
        base.OnHit(data);

        if (IsDeath)
            return;

        healthController.TakeDamage(new vDamage() 
        { 
            damage = 250,
        });

        if(data.customHitSetting.useCustomHit)
        {
            IsHit = true;
            fsmContext.ChangeStateNow(NpcFsmLayer.HitLayer, NpcHitStateType.Custom, new HitStateData()
            {
                hitData = data,
            });
        }
        else
        {
            IsHit = true;
            fsmContext.ChangeStateNow(NpcFsmLayer.HitLayer, NpcHitStateType.HitHard, new HitStateData()
            {
                hitData = data,
            });
        }
    }
}
