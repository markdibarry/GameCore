using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using GameCore.Items;
using GameCore.Utility;

namespace GameCore.Statistics;

public abstract class BaseStats
{
    protected BaseStats(IDamageable damageable, IEnumerable<Stat> statLookup, IEnumerable<Modifier> mods)
    {
        StatsOwner = damageable;
        DamageToProcess = new();
        ModifierRefs = new();
        StatusEffects = new();
        StatLookup = statLookup.ToDictionary(x => x.StatType, x => new Stat(x));
        foreach (Modifier modifier in mods)
            AddMod(modifier);
    }

    /// <summary>
    /// For menu stat mocking
    /// </summary>
    /// <param name="stats"></param>
    protected BaseStats(BaseStats stats)
        : this(null!, Array.Empty<Stat>(), Array.Empty<Modifier>())
    {
        foreach (KeyValuePair<int, Stat> pair in stats.StatLookup)
            StatLookup[pair.Key] = new(pair.Value);
        foreach (KeyValuePair<int, List<ModifierRef>> pair in stats.ModifierRefs)
            ModifierRefs[pair.Key] = pair.Value.ToList();
    }

    [JsonIgnore]
    public Queue<IDamageRequest> DamageToProcess { get; }
    public IDamageResult? CurrentDamageResult { get; private set; }
    public Dictionary<int, Stat> StatLookup { get; }
    public IDamageable StatsOwner { get; }
    protected Dictionary<int, List<ModifierRef>> ModifierRefs { get; }
    protected List<IStatusEffect> StatusEffects { get; }
    protected static IStatusEffectDB StatusEffectDB { get; } = StatsLocator.StatusEffectDB;
    public event Action<IDamageResult>? DamageReceived;
    public event Action<Modifier, ModChangeType>? ModChanged;
    public event Action<double>? Processed;
    public event Action? StatChanged;
    public event Action<int, ModChangeType>? StatusEffectChanged;

    public virtual void AddMod(Modifier mod, object? source = null)
    {
        if (source == null && Condition.ShouldRemove(this, mod.Conditions))
            return;
        List<ModifierRef> modRefs = ModifierRefs.GetOrAddNew(mod.StatType);

        // Reset existing independent mod if already exists
        if (source == null && modRefs.Any(x => x.Source == null))
        {
            ModifierRef existingTempMod = modRefs.First(x => x.Source == null);
            existingTempMod.ResetConditions();
            return;
        }

        ModifierRef modRef = new(mod, source);
        modRefs.Add(modRef);
        modRef.IsActive = !modRef.ShouldDeactivate(this);
        modRef.ConditionUpdated += OnConditionUpdated;
        modRef.ConditionChanged += OnConditionChanged;
        modRef.SubscribeConditions(this);
        UpdateSpecialCategory(mod.StatType);
        RaiseModChanged(mod, ModChangeType.Add);
    }

    public abstract int CalculateStat(int statType, bool ignoreHidden = false);

    public Dictionary<int, Stat> CloneStatLookup()
    {
        Dictionary<int, Stat> statLookup = new();
        foreach (var pair in StatLookup)
            statLookup[pair.Key] = new Stat(pair.Value);
        return statLookup;
    }

    public List<ModifierRef> GetModifiers(bool ignoreDependentMods = false)
    {
        List<ModifierRef> modRefs = new();

        if (ignoreDependentMods)
        {
            foreach (KeyValuePair<int, List<ModifierRef>> pair in ModifierRefs)
                modRefs.AddRange(pair.Value.Where(x => x.Source != null));
            return modRefs;
        }

        foreach (KeyValuePair<int, List<ModifierRef>> pair in ModifierRefs)
            modRefs.AddRange(pair.Value);
        return modRefs;
    }

    public IReadOnlyCollection<ModifierRef> GetModifiersByType(int statType)
    {
        return ModifierRefs.TryGetValue(statType, out List<ModifierRef>? mod) ? mod : Array.Empty<ModifierRef>();
    }

    public Stat? GetStat(int statType) => StatLookup.TryGetValue(statType, out Stat? stat) ? stat : default;

    public bool HasStatusEffect(int statusEffectType)
    {
        return StatusEffects.Any(x => x.EffectType == statusEffectType);
    }

    public void OnDamageReceived(IDamageRequest damageRequest) => ReceiveDamageRequest(damageRequest);

    public void Process(double delta, bool processEffects)
    {
        if (processEffects)
            Processed?.Invoke(delta);
        CurrentDamageResult = DamageToProcess.Count > 0 ? HandleDamage(DamageToProcess.Dequeue()) : null;
    }

    public void ReceiveDamageRequest(IDamageRequest damageRequest)
    {
        DamageToProcess.Enqueue(damageRequest);
    }

    public virtual void RemoveMod(ModifierRef modRef, object? source = null) => RemoveMod(modRef.Modifier, source);

    public virtual void RemoveMod(Modifier mod, object? source = null, bool unsubscribe = true)
    {
        if (!ModifierRefs.TryGetValue(mod.StatType, out List<ModifierRef>? modRefs))
            return;
        ModifierRef? modRef = modRefs.FirstOrDefault(x => x.Modifier == mod && x.Source == source);
        if (modRef == null)
            return;

        if (unsubscribe)
        {
            modRef.ConditionUpdated -= OnConditionUpdated;
            modRef.ConditionChanged -= OnConditionChanged;
            modRef.UnsubscribeConditions(this);
        }

        modRefs.Remove(modRef);
        if (modRefs.Count == 0)
            ModifierRefs.Remove(mod.StatType);
        UpdateSpecialCategory(mod.StatType);
        RaiseModChanged(mod, ModChangeType.Remove);
    }

    protected abstract IDamageResult HandleDamage(IDamageRequest damageData);

    protected void RaiseModChanged(Modifier mod, ModChangeType modChange) => ModChanged?.Invoke(mod, modChange);

    protected void RaiseDamageReceived(IDamageResult damageResult) => DamageReceived?.Invoke(damageResult);

    protected void RaiseStatChanged() => StatChanged?.Invoke();

    protected virtual void UpdateSpecialCategory(int statType) { }

    protected void AddStatusEffect(int statusEffectType)
    {
        if (StatusEffects.Any(x => x.EffectType == statusEffectType))
            return;
        StatusEffectData? effectData = StatusEffectDB.GetEffectData(statusEffectType);
        if (effectData == null)
            return;
        IStatusEffect statusEffect = new StatusEffect(effectData);
        statusEffect.ConditionUpdated += OnConditionUpdated;
        statusEffect.ConditionChanged += OnStatusEffectConditionChanged;
        statusEffect.SubscribeConditions(this);
        StatusEffects.Add(statusEffect);
        statusEffect.EffectData.EnterEffect?.Invoke(this, statusEffect);
        StatusEffectChanged?.Invoke(statusEffectType, ModChangeType.Add);
    }

    protected void RemoveStatusEffect(int statusEffectType)
    {
        IStatusEffect? statusEffect = StatusEffects.FirstOrDefault(x => x.EffectType == statusEffectType);
        if (statusEffect == null)
            return;
        statusEffect.ConditionUpdated -= OnConditionUpdated;
        statusEffect.ConditionChanged -= OnStatusEffectConditionChanged;
        statusEffect.UnsubscribeConditions(this);
        statusEffect.EffectData.ExitEffect?.Invoke(this, statusEffect);
        StatusEffects.Remove(statusEffect);
        StatusEffectChanged?.Invoke(statusEffectType, ModChangeType.Remove);
    }

    private void OnConditionUpdated(Condition condition) => condition.UpdateCondition(this);

    private void OnConditionChanged(ModifierRef modRef, Condition condition)
    {
        if (modRef.IsConditionRemovable(condition))
        {
            if (modRef.ShouldRemove(this))
                RemoveMod(modRef);
        }
        else
        {
            bool isActive = !modRef.ShouldDeactivate(this);
            if (modRef.IsActive != isActive)
            {
                modRef.IsActive = isActive;
                UpdateSpecialCategory(modRef.Modifier.StatType);
            }
        }
    }

    private void OnStatusEffectConditionChanged(IStatusEffect statusEffect, Condition condition)
    {
        statusEffect.HandleChanges(this, condition);
    }
}
