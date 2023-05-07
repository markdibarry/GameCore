using System;
using GameCore.Items;
using GameCore.Statistics;
using GameCore.Utility;

namespace GameCore.Actors;

public abstract class AActor : IDamageable
{
    protected AActor(
        string actorId,
        string actorBodyId,
        string actorName,
        string equipmentSlotPresetId,
        AInventory inventory)
    {
        ActorId = actorId;
        ActorBodyId = actorBodyId;
        Inventory = inventory;
        Name = actorName;
        EquipmentSlotPresetId = equipmentSlotPresetId;
    }

    private AActorBody? _actorBodyInternal;
    public virtual AActorBody? ActorBody => _actorBodyInternal;
    public string ActorBodyId { get; set; }
    public string ActorId { get; set; }
    public int Role { get; protected set; }
    public abstract AEquipment Equipment { get; }
    public string EquipmentSlotPresetId { get; }
    public AInventory Inventory { get; set; }
    public string Name { get; set; }
    public abstract AStats Stats { get; }

    public event Action<AActor>? Defeated;
    public event Action<AActor, ADamageResult>? DamageReceived;
    public event Action<AActor, Modifier, ModChangeType>? ModChanged;
    public event Action<AActor>? StatsChanged;
    public event Action<AActor, int, ModChangeType>? StatusEffectChanged;

    public virtual T CreateBody<T>() where T : AActorBody
    {
        string? bodyPath = ActorsLocator.ActorBodyDB.ById(ActorBodyId);
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

    public virtual void SetActorBody(AActorBody? actorBody) => _actorBodyInternal = actorBody;

    protected void RaiseDefeated() => Defeated?.Invoke(this);

    protected abstract void OnEquipmentSet(EquipmentSlot slot, AItem? oldItem, AItem? newItem);

    private void OnModChanged(Modifier mod, ModChangeType changeType)
    {
        ModChanged?.Invoke(this, mod, changeType);
    }

    private void OnStatsChanged() => StatsChanged?.Invoke(this);

    private void OnDamageRecieved(ADamageResult damageResult)
    {
        damageResult.RecieverName = Name;
        DamageReceived?.Invoke(this, damageResult);
    }

    private void OnStatusEffectChanged(int statusEffectType, ModChangeType changeType)
    {
        StatusEffectChanged?.Invoke(this, statusEffectType, changeType);
    }
}
