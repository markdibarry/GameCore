using System;
using System.Collections.Generic;
using System.Linq;

namespace GameCore.Statistics;

public class ModifierRef
{
    public ModifierRef(Modifier mod, object? source = null)
        : this(mod, mod.Conditions, source)
    { }

    public ModifierRef(ModifierRef modRef)
    : this(modRef.Modifier, modRef.Conditions, modRef.Source)
    { }

    public ModifierRef(Modifier mod, IEnumerable<Condition> conditions, object? source = null)
    {
        Modifier = mod;
        Conditions = conditions.Select(x => x.Clone()).ToList();
        Source = source;
    }

    public List<Condition> Conditions { get; }
    public bool IsActive { get; set; }
    public bool IsHidden => Modifier.IsHidden;
    public Modifier Modifier { get; }
    public ModOp Op => Modifier.Op;
    public object? Source { get; }
    public int Value => Modifier.Value;
    public event Action<Condition>? ConditionUpdated;
    public event Action<ModifierRef, Condition>? ConditionChanged;

    // TODO Add special case handling i.e. +5% for every 100 enemies killed
    public int Apply(int baseValue) => Modifier.Apply(baseValue);

    public bool IsRemovable(Condition condition)
    {
        return condition.ResultType == ConditionResultType.Remove ||
            (condition.ResultType.HasFlag(ConditionResultType.Remove) && Source == null);
    }

    public void ResetConditions()
    {
        foreach (Condition condition in Conditions)
            condition.Reset();
    }

    public bool ShouldDeactivate(StatsBase stats) => Modifier.ShouldDeactivate(stats, Conditions);

    public bool ShouldRemove(StatsBase stats) => Modifier.ShouldRemove(stats, Conditions);

    public void SubscribeConditions(StatsBase stats)
    {
        foreach (Condition condition in Conditions)
        {
            Condition? nextCondition = condition;
            while (nextCondition != null)
            {
                nextCondition.SubscribeEvents(stats);
                nextCondition.Updated += OnConditionUpdated;
                nextCondition.StatusChanged += OnConditionChanged;
                nextCondition = nextCondition.AdditionalCondition;
            }
        }
    }

    public void UnsubscribeConditions(StatsBase stats)
    {
        foreach (Condition condition in Conditions)
        {
            Condition? nextCondition = condition;
            while (nextCondition != null)
            {
                nextCondition.UnsubscribeEvents(stats);
                nextCondition.Updated -= OnConditionUpdated;
                nextCondition.StatusChanged -= OnConditionChanged;
                nextCondition = nextCondition.AdditionalCondition;
            }
        }
    }

    private void OnConditionUpdated(Condition condition) => ConditionUpdated?.Invoke(condition);

    private void OnConditionChanged(Condition condition) => ConditionChanged?.Invoke(this, condition);
}
