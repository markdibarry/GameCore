using System;
using GameCore.Items;
using GameCore.Statistics;
using GameCore.Utility;

namespace GameCore.Actors;

public abstract class BaseActor : IDamageable
{
    protected BaseActor(
        string actorId,
        string actorBodyId,
        string actorName,
        string equipmentSlotPresetId,
        BaseInventory inventory)
    {
        ActorId = actorId;
        ActorBodyId = actorBodyId;
        Inventory = inventory;
        Name = actorName;
        EquipmentSlotPresetId = equipmentSlotPresetId;
    }

    private BaseActorBody? _actorBodyInternal;
    public virtual BaseActorBody? ActorBody => _actorBodyInternal;
    public string ActorBodyId { get; set; }
    public string ActorId { get; set; }
    public int Role { get; protected set; }
    public abstract BaseEquipment Equipment { get; }
    public string EquipmentSlotPresetId { get; }
    public BaseInventory Inventory { get; set; }
    public string Name { get; set; }
    public abstract BaseStats Stats { get; }

    public event Action<BaseActor>? Defeated;
    public event Action<BaseActor, IDamageResult>? DamageReceived;
    public event Action<BaseActor, Modifier, ModChangeType>? ModChanged;
    public event Action<BaseActor>? StatsChanged;
    public event Action<BaseActor, int, ModChangeType>? StatusEffectChanged;

    public virtual T CreateBody<T>() where T : BaseActorBody
    {
        string? bodyPath = ActorsLocator.ActorBodyDB.GetById(ActorBodyId);

        if (bodyPath == null)
            throw new Exception($"No Body {ActorBodyId} found.");

        T actorBody = GDEx.Instantiate<T>(bodyPath);
        SetActorBody(actorBody);
        actorBody.SetActor(this);

        return actorBody;
    }

    public virtual void Init()
    {
        Equipment.EquipmentSetCallback = OnEquipmentSet;
        InitStats();
    }

    public virtual void InitStats()
    {
        Stats.DamageReceived += OnDamageRecieved;
        Stats.StatChanged += OnStatsChanged;
        Stats.ModChanged += OnModChanged;
        Stats.StatusEffectChanged += OnStatusEffectChanged;
    }

    public abstract void SetRole(int role, bool setActorBodyRole = true);

    public virtual void SetActorBody(BaseActorBody? actorBody) => _actorBodyInternal = actorBody;

    protected void RaiseDefeated() => Defeated?.Invoke(this);

    protected abstract void OnEquipmentSet(EquipmentSlot slot, BaseItem? oldItem, BaseItem? newItem);

    private void OnModChanged(Modifier mod, ModChangeType changeType)
    {
        ModChanged?.Invoke(this, mod, changeType);
    }

    private void OnStatsChanged() => StatsChanged?.Invoke(this);

    private void OnDamageRecieved(IDamageResult damageResult)
    {
        damageResult.RecieverName = Name;
        DamageReceived?.Invoke(this, damageResult);
    }

    private void OnStatusEffectChanged(int statusEffectType, ModChangeType changeType)
    {
        StatusEffectChanged?.Invoke(this, statusEffectType, changeType);
    }
}
