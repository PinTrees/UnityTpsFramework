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

    public override void OnKnockDown(KnockDownMotionType motionType)
    {
        base.OnKnockDown(motionType);

        IsKnockDown = true;
        fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.Idle);

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

        IsConfrontingTrace = true;
        movementLayer.ChangeStateNow(NpcMovementStateType.ConfrontingTrace);

        return true;
    }

    public override void OnAttack()
    {
        base.OnAttack();

        targetController.lockDetectUpdate = true;
        var target = targetController.forcusTarget;

        TaskSystem.CoroutineUpdateLost(() =>
        {
            if (IsDeath || IsHit)
            {
                IsReadyToAttack = false;
                IsStartToAttack = false;
                target.targetController.activeAttackers.Remove(this);
                return true;
            }

            if (IsStartToAttack)
            {
                IsAttack = true;
                IsReadyToAttack = false;
                IsStartToAttack = false;
                fsmContext.ChangeStateNow(NpcFsmLayer.AttackLayer, NpcAttackStateType.Attack);
                return true;
            }

            if (IsRunToAttack)
                return false;

            // 플레이어와의 거리 확인
            if (combatController.currentAttackNode.attackerTransformSetting.useAttackerTransform)
            {
                var attackerTransformSetting = combatController.currentAttackNode.attackerTransformSetting;
                var distancFromTarget = attackerTransformSetting.distanceFromTarget;
                var distance = Vector3.Distance(targetController.forcusTarget.baseObject.transform.position, baseObject.transform.position);

                if(distance > distancFromTarget)
                {
                    IsRunToAttack = true;
                    fsmContext.ChangeStateNow(NpcFsmLayer.MovementLayer, NpcMovementStateType.RunToAttack, distancFromTarget);
                }
            }
            return false;
        }, 0.1f);
    }

    public override bool OnHit(HitData data)
    {
       if (!base.OnHit(data))
            return false;

        healthController.TakeDamage(new vDamage()
        {
            damage = 250,
        });

        IsHit = true;
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

        return true;
    }
}
