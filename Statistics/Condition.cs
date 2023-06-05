using System;
using System.Text.Json.Serialization;
using Godot;

namespace GameCore.Statistics;

[JsonConverter(typeof(ConditionConverter))]
[GlobalClass]
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

    private WeakReference _statsInternal = null!;
    public abstract int ConditionType { get; }
    [Export]
    public ConditionResultType ResultType { get; set; }
    [Export]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter(typeof(ConditionConverter))]
    public Condition? AdditionalCondition { get; set; }
    [Export]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public LogicOp AdditionalLogicOp { get; set; }
    protected bool ConditionMet { get; set; }
    protected virtual BaseStats Stats => (BaseStats)_statsInternal.Target!;
    protected WeakReference? ConditionChangedCallback { get; set; }

    public void SetStats(BaseStats stats)
    {
        _statsInternal = new(stats);
        AdditionalCondition?.SetStats(stats);
    }

    public bool CheckConditions()
    {
        ConditionMet = CheckCondition();
        if (ConditionMet)
        {
            if (AdditionalCondition?.AdditionalLogicOp == LogicOp.And)
                return AdditionalCondition.CheckConditions();
            return true;
        }
        else
        {
            if (AdditionalCondition?.AdditionalLogicOp == LogicOp.Or)
                return AdditionalCondition.CheckConditions();
            return false;
        }
    }

    public virtual void Reset()
    {
        AdditionalCondition?.Reset();
    }

    public void Subscribe(Action handler)
    {
        SubscribeEvents();
        ConditionChangedCallback = new(handler);
        AdditionalCondition?.Subscribe(handler);
    }

    public void Unsubscribe()
    {
        UnsubscribeEvents();
        ConditionChangedCallback = null;
        AdditionalCondition?.Unsubscribe();
    }

    protected void UpdateCondition()
    {
        bool result = CheckCondition();
        if (result != ConditionMet)
        {
            ConditionMet = result;
            (ConditionChangedCallback?.Target as Action)?.Invoke();
        }
    }

    public abstract Condition Clone();
    protected abstract bool CheckCondition();
    protected abstract void SubscribeEvents();
    protected abstract void UnsubscribeEvents();
}
