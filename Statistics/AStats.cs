using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using GameCore.Utility;

namespace GameCore.Statistics;

public abstract class AStats
{
    protected AStats(IDamageable damageable, IEnumerable<Stat> statLookup, IEnumerable<Modifier> mods)
    {
        StatsOwner = damageable;
        DamageToProcess = new();
        Modifiers = new();
        StatusEffects = new();
        StatLookup = new();
        foreach (var stat in statLookup)
            StatLookup[stat.StatType] = new Stat(stat);
        foreach (var modifier in mods)
            AddMod(new Modifier(modifier));
    }

    /// <summary>
    /// For menu stat mocking
    /// </summary>
    /// <param name="stats"></param>
    protected AStats(AStats stats)
        : this(null!, Array.Empty<Stat>(), Array.Empty<Modifier>())
    {
        foreach (var pair in stats.StatLookup)
            StatLookup[pair.Key] = pair.Value;
        foreach (var pair in stats.Modifiers)
            Modifiers[pair.Key] = pair.Value.ToList();
    }

    [JsonIgnore]
    public Queue<ADamageRequest> DamageToProcess { get; }
    public ADamageResult? CurrentDamageResult { get; private set; }
    [JsonConverter(typeof(ModifierLookupConverter))]
    public Dictionary<int, List<Modifier>> Modifiers { get; }
    public Dictionary<int, Stat> StatLookup { get; }
    public IDamageable StatsOwner { get; }
    protected List<IStatusEffect> StatusEffects { get; }
    protected static IStatusEffectDB StatusEffectDB { get; } = StatsLocator.StatusEffectDB;
    public event Action<ADamageResult>? DamageReceived;
    public event Action<Modifier, ModChangeType>? ModChanged;
    public event Action<double>? Processed;
    public event Action? StatChanged;
    public event Action<int, ModChangeType>? StatusEffectChanged;

    public virtual void AddMod(Modifier mod)
    {
        mod.InitConditions(this);
        if (mod.ShouldRemove() && mod.SourceType == SourceType.Independent)
            return;
        List<Modifier> mods = Modifiers.GetOrAddNew(mod.StatType);

        // Reset existing independent mod if already exists
        if (mod.SourceType == SourceType.Independent && mods.Any(x => x.SourceType == SourceType.Independent))
        {
            Modifier existingTempMod = mods.First(x => x.SourceType == SourceType.Independent);
            existingTempMod.ResetConditions();
            return;
        }

        mod.IsActive = !mod.ShouldDeactivate();

        mods.Add(mod);
        mod.SubscribeConditions(OnActivationConditionMet, OnRemovalConditionMet);
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

    public void OnDamageReceived(ADamageRequest damageRequest) => ReceiveDamageRequest(damageRequest);

    public void Process(double delta, bool processEffects)
    {
        if (processEffects)
            Processed?.Invoke(delta);
        CurrentDamageResult = DamageToProcess.Count > 0 ? HandleDamage(DamageToProcess.Dequeue()) : null;
    }

    public void ReceiveDamageRequest(ADamageRequest damageRequest)
    {
        DamageToProcess.Enqueue(damageRequest);
    }

    public virtual void RemoveMod(Modifier mod)
    {
        if (!Modifiers.TryGetValue(mod.StatType, out List<Modifier>? mods))
            return;
        if (!mods.Contains(mod))
            return;
        mod.UnsubscribeConditions();
        mods.Remove(mod);
        if (mods.Count == 0)
            Modifiers.Remove(mod.StatType);
        UpdateSpecialCategory(mod.StatType);
        RaiseModChanged(mod, ModChangeType.Remove);
    }

    protected abstract ADamageResult HandleDamage(ADamageRequest damageData);

    protected void OnActivationConditionMet(Modifier mod)
    {
        UpdateSpecialCategory(mod.StatType);
    }

    protected void OnRemovalConditionMet(Modifier mod)
    {
        if (mod.SourceType == SourceType.Independent)
            RemoveMod(mod);
    }

    protected void RaiseModChanged(Modifier mod, ModChangeType modChange) => ModChanged?.Invoke(mod, modChange);

    protected void RaiseDamageReceived(ADamageResult damageResult) => DamageReceived?.Invoke(damageResult);

    protected void RaiseStatChanged() => StatChanged?.Invoke();

    protected virtual void UpdateSpecialCategory(int statType) { }

    protected void AddStatusEffect(int statusEffectType)
    {
        if (StatusEffects.Any(x => x.EffectType == statusEffectType))
            return;
        StatusEffectData? effectData = StatusEffectDB.GetEffectData(statusEffectType);
        if (effectData == null)
            return;
        StatusEffect statusEffect = new(this, effectData);
        statusEffect.SubscribeCondition();
        StatusEffects.Add(statusEffect);
        statusEffect.EffectData.EnterEffect?.Invoke(statusEffect);
        StatusEffectChanged?.Invoke(statusEffectType, ModChangeType.Add);
    }

    protected void RemoveStatusEffect(int statusEffectType)
    {
        IStatusEffect? statusEffect = StatusEffects.FirstOrDefault(x => x.EffectType == statusEffectType);
        if (statusEffect == null)
            return;
        statusEffect.UnsubscribeCondition();
        statusEffect.EffectData.ExitEffect?.Invoke(statusEffect);
        StatusEffects.Remove(statusEffect);
        StatusEffectChanged?.Invoke(statusEffectType, ModChangeType.Remove);
    }
}
