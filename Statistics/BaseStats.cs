using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Arenbee.Statistics;
using GameCore.Utility;

namespace GameCore.Statistics;

public abstract class BaseStats
{
    protected BaseStats(IDamageable damageable, IEnumerable<Stat> statLookup, IEnumerable<Modifier> mods)
    {
        StatsOwner = damageable;
        DamageToProcess = new();
        Modifiers = new();
        StatusEffects = new();
        StatLookup = statLookup.ToDictionary(x => x.StatType, x => new Stat(x));
        foreach (Modifier modifier in mods)
            AddMod(new(modifier));
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
        foreach (KeyValuePair<int, List<Modifier>> pair in stats.Modifiers)
            Modifiers[pair.Key] = pair.Value.ToList();
    }

    [JsonIgnore]
    public Queue<IDamageRequest> DamageToProcess { get; }
    public IDamageResult? CurrentDamageResult { get; private set; }
    [JsonConverter(typeof(ModifierLookupConverter))]
    public Dictionary<int, Stat> StatLookup { get; }
    public IDamageable StatsOwner { get; }
    protected Dictionary<int, List<Modifier>> Modifiers { get; }
    protected List<IStatusEffect> StatusEffects { get; }
    protected static IStatusEffectDB StatusEffectDB { get; } = StatsLocator.StatusEffectDB;
    public event Action<IDamageResult>? DamageReceived;
    public event Action<Modifier, ModChangeType>? ModChanged;
    public event Action<double>? Processed;
    public event Action? StatChanged;
    public event Action<int, ModChangeType>? StatusEffectChanged;

    public virtual void AddMod(Modifier mod)
    {
        if (mod.SourceType == SourceType.Independent && mod.ShouldRemove(this))
            return;
        List<Modifier> mods = Modifiers.GetOrAddNew(mod.StatType);

        // Reset existing independent mod if already exists
        if (mod.SourceType == SourceType.Independent && mods.Any(x => x.SourceType == SourceType.Independent))
        {
            Modifier existingTempMod = mods.First(x => x.SourceType == SourceType.Independent);
            existingTempMod.ResetConditions();
            return;
        }

        mods.Add(mod);
        mod.IsActive = !mod.ShouldDeactivate(this);
        mod.ConditionUpdated += OnConditionUpdated;
        mod.ConditionChanged += OnConditionChanged;
        mod.SubscribeConditions(this);
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

    public List<Modifier> GetModifiers(bool ignoreDependentMods = false)
    {
        List<Modifier> mods = new();

        if (ignoreDependentMods)
        {
            foreach (KeyValuePair<int, List<Modifier>> pair in Modifiers)
                mods.AddRange(pair.Value
                    .Where(x => x.SourceType != (int)SourceType.Dependent));
            return mods;
        }

        foreach (KeyValuePair<int, List<Modifier>> pair in Modifiers)
            mods.AddRange(pair.Value);
        return mods;
    }

    public IReadOnlyCollection<Modifier> GetModifiersByType(int statType)
    {
        return Modifiers.TryGetValue(statType, out List<Modifier>? mod) ? mod : Array.Empty<Modifier>();
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

    public virtual void RemoveMod(Modifier mod)
    {
        if (!Modifiers.TryGetValue(mod.StatType, out List<Modifier>? mods) || !mods.Contains(mod))
            return;
        mod.ConditionUpdated -= OnConditionUpdated;
        mod.ConditionChanged -= OnConditionChanged;
        mod.UnsubscribeConditions(this);
        mods.Remove(mod);
        if (mods.Count == 0)
            Modifiers.Remove(mod.StatType);
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

    private void OnConditionChanged(Modifier mod, Condition condition)
    {
        if (mod.IsConditionRemovable(condition))
        {
            if (mod.ShouldRemove(this))
                RemoveMod(mod);
        }
        else
        {
            bool isActive = !mod.ShouldDeactivate(this);
            if (mod.IsActive != isActive)
            {
                mod.IsActive = isActive;
                UpdateSpecialCategory(mod.StatType);
            }
        }
    }

    private void OnStatusEffectConditionChanged(IStatusEffect statusEffect, Condition condition)
    {
        statusEffect.HandleChanges(this, condition);
    }
}
