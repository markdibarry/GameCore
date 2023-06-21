using System;
using System.Text.Json.Serialization;
using Godot;

namespace GameCore.Statistics;

[GlobalClass]
[JsonConverter(typeof(ConditionConverter))]
public abstract partial class Condition : Resource
{
    protected Condition() { }

    protected Condition(Condition condition)
        : this(condition.ResultType, condition.AdditionalLogicOp, condition.AdditionalCondition)
    {
    }

    protected Condition(
        ConditionResultType resultType,
        LogicOp additionalLogicOp,
        Condition? additionalCondition)
    {
        ResultType = resultType;
        AdditionalLogicOp = additionalLogicOp;
        AdditionalCondition = additionalCondition?.Clone();
    }

    public abstract int ConditionType { get; }
    [Export]
    public ConditionResultType ResultType { get; set; } = ConditionResultType.Remove;
    [Export]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(ConditionConverter))]
    public Condition? AdditionalCondition { get; set; }
    [Export]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public LogicOp AdditionalLogicOp { get; set; }
    protected bool ConditionMet { get; set; }
    public event Action<Condition>? StatusChanged;
    public event Action<Condition>? Updated;

    public bool CheckIfConditionsMet(BaseStats stats)
    {
        ConditionMet = CheckIfConditionMet(stats);
        if (ConditionMet)
        {
            if (AdditionalCondition?.AdditionalLogicOp == LogicOp.And)
                return AdditionalCondition.CheckIfConditionsMet(stats);
            return true;
        }
        else
        {
            if (AdditionalCondition?.AdditionalLogicOp == LogicOp.Or)
                return AdditionalCondition.CheckIfConditionsMet(stats);
            return false;
        }
    }

    public virtual void Reset() => AdditionalCondition?.Reset();

    public void UpdateCondition(BaseStats stats)
    {
        bool result = CheckIfConditionMet(stats);
        if (result != ConditionMet)
        {
            ConditionMet = result;
            StatusChanged?.Invoke(this);
        }
    }

    public abstract Condition Clone();
    public abstract void SubscribeEvents(BaseStats stats);
    public abstract void UnsubscribeEvents(BaseStats stats);
    protected abstract bool CheckIfConditionMet(BaseStats stats);
    protected void RaiseConditionUpdated() => Updated?.Invoke(this);
}
