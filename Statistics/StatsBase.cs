using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using GameCore.Utility;

namespace GameCore.Statistics;

public abstract class StatsBase
{
    protected StatsBase(IDamageable damageable, IEnumerable<Modifier> sourceMods)
    {
        StatsOwner = damageable;
        DamageToProcess = new();
        Modifiers = [];
        StatusEffects = [];

        foreach (Modifier sourceMod in sourceMods)
        {
            if (!Modifiers.TryGetValue(sourceMod.StatType, out List<Modifier>? mods))
                mods = ListPool<Modifier>.Get();

            Modifier mod = ObjectPool<Modifier>.Get();
            mod.Init(sourceMod);
            mods.Add(mod);
        }

        AddDefaultMods();
        SortAllModifiers();
        RegisterAllMods();
    }

    /// <summary>
    /// For menu stat mocking
    /// </summary>
    /// <param name="stats"></param>
    protected StatsBase(StatsBase stats)
        : this(null!, [])
    {
        foreach (KeyValuePair<string, List<Modifier>> pair in stats.Modifiers)
        {
            List<Modifier> mods = ListPool<Modifier>.Get();

            foreach (Modifier sourceMod in pair.Value)
            {
                Modifier mod = ObjectPool<Modifier>.Get();
                mod.Init(sourceMod);
                mods.Add(mod);
            }

            Modifiers[pair.Key] = mods;
        }

        AddDefaultMods();
        SortAllModifiers();
        RegisterAllMods();
    }

    protected static IStatusEffectDB StatusEffectDB { get; } = StatsLocator.StatusEffectDB;

    [JsonIgnore]
    public Queue<IDamageRequest> DamageToProcess { get; }
    public IDamageResult? CurrentDamageResult { get; private set; }
    public IDamageable StatsOwner { get; }
    protected Dictionary<string, List<Modifier>> Modifiers { get; }
    protected Dictionary<string, List<StatusEffect>> StatusEffects { get; }
    public event Action<StatsBase, IDamageResult>? DamageReceived;
    public event Action<StatsBase, Modifier, ModChangeType>? ModChanged;
    public event Action<StatsBase, double>? Processed;
    public event Action<StatsBase>? StatChanged;
    public event Action<StatsBase, string, ModChangeType>? StatusEffectChanged;

    protected virtual void AddDefaultMods() { }

    public bool AddMod(Modifier sourceMod, object? source)
    {
        if (!Modifiers.TryGetValue(sourceMod.StatType, out List<Modifier>? mods))
            mods = ListPool<Modifier>.Get();

        Modifier mod = ObjectPool<Modifier>.Get();
        mod.Init(sourceMod);
        mod.Source = source;
        mod.Condition?.InitializeConditions(this);

        if (source == null && mod.ShouldRemove())
        {
            ObjectPool<Modifier>.Return(mod);
            return false;
        }

        mods.Add(mod);
        SortMods(mods);
        RegisterMod(mod);
        return true;
    }

    public void AddStatusEffect(string effectTypeId, object? source)
    {

        if (!StatusEffectDB.TryGetEffectData(effectTypeId, out StatusEffectData? effectData))
            return;

        if (source == null && effectData.D)

        StatusEffect statusEffect = new(effectData);
        statusEffect.ConditionChanged += OnStatusEffectConditionChanged;
        statusEffect.SubscribeConditions(this);
        StatusEffects.Add(effectTypeId, statusEffect);
        statusEffect.EffectData.EnterEffect?.Invoke(this, statusEffect);
        StatusEffectChanged?.Invoke(this, effectTypeId, ModChangeType.Add);
    }

    public abstract float CalculateStat(string statTypeId, bool ignoreHidden = false);

    protected float CalculateDefault(string statTypeId, bool ignoreHidden)
    {
        float result = default;
        float percentToAdd = default;
        IReadOnlyCollection<Modifier> mods = GetModifiersByType(statTypeId);

        for (int i = 0; i < mods.Count; i++)
        {
            Modifier mod = mods.ElementAt(i);

            if (ignoreHidden && mod.IsHidden)
                continue;

            if (mod.IsRegistered ? !mod.IsActive : mod.ShouldRemove(this))
                continue;

            if (mod.Op == ModOp.PercentAdd)
            {
                percentToAdd = mod.Apply(percentToAdd);

                if (i + 1 >= mods.Count || mods.ElementAt(i + 1).Op != ModOp.PercentAdd)
                    result *= 1 + percentToAdd;
            }
            else
            {
                result = mod.Apply(result);
            }
        }

        return result;
    }

    public List<Modifier> GetModifiers(bool ignoreDependentMods = false)
    {
        List<Modifier> mods = [];

        foreach (KeyValuePair<string, List<Modifier>> pair in Modifiers)
        {
            foreach (Modifier mod in pair.Value)
            {
                if (!ignoreDependentMods || mod.Source == null)
                    mods.Add(mod);
            }
        }

        return mods;
    }

    public IReadOnlyCollection<Modifier> GetModifiersByType(string statTypeId)
    {
        if (Modifiers.TryGetValue(statTypeId, out List<Modifier>? mods))
            return mods;

        return [];
    }

    public bool HasStatusEffect(string statusEffectTypeId)
    {
        return StatusEffects.ContainsKey(statusEffectTypeId);
    }

    public void OnDamageReceived(IDamageRequest damageRequest) => ReceiveDamageRequest(damageRequest);

    public void Process(double delta, bool processEffects)
    {
        if (processEffects)
            Processed?.Invoke(this, delta);

        CurrentDamageResult = DamageToProcess.Count > 0 ? HandleDamage(DamageToProcess.Dequeue()) : null;
    }

    public void ReceiveDamageRequest(IDamageRequest damageRequest)
    {
        DamageToProcess.Enqueue(damageRequest);
    }

    public virtual void RemoveMod(Modifier sourceMod, object? source = null)
    {
        if (!Modifiers.TryGetValue(sourceMod.StatType, out List<Modifier>? mods))
            return;

        Modifier? mod = mods.FirstOrDefault(x =>
            x.Source == source
            && x.Op == sourceMod.Op
            && x.Value == sourceMod.Value);

        if (mod == null)
            return;

        mod.ConditionChanged -= OnConditionChanged;
        mod.UnsubscribeConditions(this);
        mods.Remove(mod);
        ObjectPool<Modifier>.Return(mod);

        if (mods.Count == 0)
        {
            Modifiers.Remove(sourceMod.StatType);
            ListPool<Modifier>.Return(mods);
        }

        UpdateSpecialCategory(sourceMod.StatType);
        RaiseModChanged(sourceMod, ModChangeType.Remove);
    }

    public void RemoveStatusEffect(string statusEffectTypeId)
    {
        if (!StatusEffects.TryGetValue(statusEffectTypeId, out StatusEffect? statusEffect))
            return;

        statusEffect.ConditionChanged -= OnStatusEffectConditionChanged;
        statusEffect.UnsubscribeConditions(this);
        statusEffect.EffectData.ExitEffect?.Invoke(this, statusEffect);
        StatusEffects.Remove(statusEffectTypeId);
        StatusEffectChanged?.Invoke(this, statusEffectTypeId, ModChangeType.Remove);
    }

    protected abstract IDamageResult HandleDamage(IDamageRequest damageData);

    protected void RaiseModChanged(Modifier mod, ModChangeType modChange) => ModChanged?.Invoke(this, mod, modChange);

    protected void RaiseDamageReceived(IDamageResult damageResult) => DamageReceived?.Invoke(this, damageResult);

    protected void RaiseStatChanged() => StatChanged?.Invoke(this);

    protected void SortAllModifiers()
    {
        foreach (KeyValuePair<string, List<Modifier>> pair in Modifiers)
            SortMods(pair.Value);
    }

    protected virtual void SortMods(List<Modifier> mods)
    {
        mods.Sort((x, y) =>
        {
            if (!ModOp.OrderIndexed.TryGetValue(x.Op, out int xi) || !ModOp.OrderIndexed.TryGetValue(y.Op, out int yi))
                return 0;
            if (xi < yi)
                return -1;
            if (xi > yi)
                return 1;
            return 0;
        });
    }

    protected virtual void UpdateSpecialCategory(string statTypeId) { }

    private void OnConditionChanged(Modifier mod, Condition condition)
    {
        if (!condition.UpdateCondition(this))
            return;

        if (mod.ShouldRemove())
        {
            if (mod.Source == null)
            {
                RemoveMod(mod);
            }
            else if (mod.IsActive)
            {
                mod.IsActive = false;
                UpdateSpecialCategory(mod.StatType);
            }
        }
    }

    private void OnStatusEffectConditionChanged(StatusEffect statusEffect, Condition condition)
    {
        if (!condition.UpdateCondition(this))
            return;

        if (condition == statusEffect.DurationCondition)
        {
            if (statusEffect.ShouldRemove())
            {
                RemoveStatusEffect(statusEffect.EffectType);

                if ()
            }
        }
        else if (condition == statusEffect.TickCondition)
        {
            statusEffect.InvokeTick(this, condition);
        }
    }

    private void RegisterAllMods()
    {
        foreach (KeyValuePair<string, List<Modifier>> pair in Modifiers)
        {
            foreach (Modifier mod in pair.Value)
            {
                List<Modifier> mods = pair.Value;
                // initialize first so we can use the cached checks up front.
                mod.Condition?.InitializeConditions(this);

                if (mod.Source == null && mod.ShouldRemove())
                {
                    mods.Remove(mod);
                    ObjectPool<Modifier>.Return(mod);

                    if (mods.Count == 0)
                    {
                        ListPool<Modifier>.Return(mods);
                        Modifiers.Remove(mod.StatType);
                    }
                }
                else
                {
                    RegisterMod(mod);
                }
            }
        }
    }

    private void RegisterMod(Modifier mod)
    {
        if (mod.IsRegistered)
            return;

        mod.IsRegistered = true;
        mod.IsActive = !mod.ShouldRemove();
        mod.ConditionChanged += OnConditionChanged;
        mod.SubscribeConditions(this);
        UpdateSpecialCategory(mod.StatType);
        RaiseModChanged(mod, ModChangeType.Add);
    }
}
