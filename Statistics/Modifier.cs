using System;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using Gictionary = Godot.Collections.Dictionary;

namespace GameCore.Statistics;

[Tool, GlobalClass]
public partial class Modifier : Resource
{
    public Modifier() { }

    [JsonConstructor]
    public Modifier(
        int statType,
        ModOp op,
        int value,
        SourceType sourceType = default,
        Godot.Collections.Array<Condition>? conditions = null,
        bool isHidden = false)
    {
        StatType = statType;
        Op = op;
        Value = value;
        SourceType = sourceType;
        Conditions = conditions ?? new();
        IsHidden = isHidden;
    }

    public Modifier(Modifier mod)
    {
        StatType = mod.StatType;
        Op = mod.Op;
        IsHidden = mod.IsHidden;
        SourceType = mod.SourceType;
        Value = mod.Value;
        Conditions = new(mod.Conditions?.Select(x => x.Clone()));
    }

    private int _statType;
    [Export] public bool IsHidden { get; set; }
    [Export] public ModOp Op { get; set; }
    [Export] public SourceType SourceType { get; set; }
    [Export] public Godot.Collections.Array<Condition> Conditions { get; set; } = new();
    public bool IsActive { get; set; }
    public int StatType
    {
        get => _statType;
        set
        {
            _statType = value;
            NotifyPropertyListChanged();
        }
    }
    public int Value { get; set; }
    public event Action<Condition>? ConditionUpdated;
    public event Action<Modifier, Condition>? ConditionChanged;

    // TODO Add special case handling i.e. +5% for every 100 enemies killed
    public int Apply(int baseValue) => Op.Compute(baseValue, Value);

    public bool IsConditionRemovable(Condition condition)
    {
        return condition.ResultType == ConditionResultType.Remove ||
            (condition.ResultType.HasFlag(ConditionResultType.Remove) && SourceType == SourceType.Independent);
    }

    public void ResetConditions()
    {
        foreach (Condition condition in Conditions)
            condition.Reset();
    }

    public bool ShouldDeactivate(BaseStats stats)
    {
        return Conditions.Any(x => x.ResultType.HasFlag(ConditionResultType.Deactivate) && x.CheckIfConditionsMet(stats));
    }

    public bool ShouldRemove(BaseStats stats)
    {
        return Conditions.Any(x => x.ResultType.HasFlag(ConditionResultType.Remove) && x.CheckIfConditionsMet(stats));
    }

    public void SubscribeConditions(BaseStats stats)
    {
        foreach (Condition condition in Conditions)
        {
            Condition? nextCondition = condition;
            while (nextCondition != null)
            {
                nextCondition.SubscribeEvents(stats);
                nextCondition.UpdatedDelegate = new(OnConditionUpdated);
                nextCondition.StatusChangedDelegate = new(OnConditionChanged);
                nextCondition = nextCondition.AdditionalCondition;
            }
        }
    }

    public void UnsubscribeConditions(BaseStats stats)
    {
        foreach (Condition condition in Conditions)
        {
            Condition? nextCondition = condition;
            while (nextCondition != null)
            {
                nextCondition.UnsubscribeEvents(stats);
                nextCondition.UpdatedDelegate = null;
                nextCondition.StatusChangedDelegate = null;
                nextCondition = nextCondition.AdditionalCondition;
            }
        }
    }

    public override Godot.Collections.Array<Gictionary> _GetPropertyList()
    {
        return StatsLocator.StatTypeDB.GetStatPropertyList(_statType);
    }

    private void OnConditionUpdated(Condition condition) => ConditionUpdated?.Invoke(condition);

    private void OnConditionChanged(Condition condition) => ConditionChanged?.Invoke(this, condition);

}
