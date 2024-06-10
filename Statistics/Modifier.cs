using System;
using System.Text.Json.Serialization;
using GameCore.Utility;

namespace GameCore.Statistics;

public record Modifier : IPoolable<Modifier>
{
    public Modifier() { }

    [JsonConstructor]
    public Modifier(
        string statType,
        string op,
        float value,
        Condition? condition,
        string customValueId,
        bool isHidden,
        object? source)
    {
        StatType = statType;
        Op = op;
        Value = value;
        CustomValueId = customValueId;
        Condition = condition;
        IsHidden = isHidden;
        Source = source;
    }

    public Modifier(string statType, string op, float value, Condition? condition = null, bool isHidden = false)
        : this(statType, op, value, condition, string.Empty, isHidden, null)
    {
    }

    public Modifier(Modifier mod)
    {
        if (mod.Condition != null)
            Condition = mod.Condition with { };

        CustomValueId = mod.CustomValueId;
        IsHidden = mod.IsHidden;
        Op = mod.Op;
        Source = mod.Source;
        StatType = mod.StatType;
        Value = mod.Value;
    }

    ~Modifier()
    {
        ObjectPool<Modifier>.Return(this);
    }

    [JsonPropertyOrder(5)]
    public Condition? Condition { get; set; }
    [JsonPropertyOrder(-2)]
    public string? CustomValueId { get; set; }
    [JsonIgnore]
    public bool IsActive { get; set; }
    [JsonPropertyOrder(-1)]
    public bool IsHidden { get; set; }
    [JsonIgnore]
    public bool IsRegistered { get; set; }
    [JsonPropertyOrder(-4)]
    public string Op { get; set; } = string.Empty;
    [JsonIgnore]
    public object? Source { get; set; }
    [JsonPropertyOrder(-5)]
    public string StatType { get; set; } = string.Empty;
    [JsonPropertyOrder(-3)]
    public float Value { get; set; }
    public event Action<Modifier, Condition>? ConditionChanged;

    public float Apply(float baseValue) => MathI.Compute(Op, baseValue, Value);

    public void ResetConditions()
    {
        Condition?.Reset();
    }

    public void Init(Modifier? mod)
    {
        if (mod?.Condition != null)
            Condition = mod.Condition with { };
        else
            Condition = default;

        CustomValueId = mod?.CustomValueId ?? default;
        IsActive = false;
        IsHidden = mod?.IsHidden ?? false;
        IsRegistered = false;
        Op = mod?.Op ?? string.Empty;
        Source = default;
        StatType = mod?.StatType ?? string.Empty;
        Value = mod?.Value ?? default;
        ConditionChanged = default;
    }

    public bool ShouldRemove(StatsBase? stats = null)
    {
        if (Condition == null)
            return false;

        return Condition.CheckAllConditions(stats);
    }

    public void SubscribeConditions(StatsBase stats)
    {
        SubscribeCondition(stats, Condition);

        void SubscribeCondition(StatsBase stats, Condition? condition)
        {
            if (condition == null)
                return;

            condition.SubscribeEvents(stats);
            condition.Changed += OnConditionChanged;

            SubscribeCondition(stats, condition.AndCondition);
            SubscribeCondition(stats, condition.OrCondition);
        }
    }

    public void UnsubscribeConditions(StatsBase stats)
    {
        UnsubscribeCondition(stats, Condition);

        void UnsubscribeCondition(StatsBase stats, Condition? condition)
        {
            if (condition == null)
                return;

            condition.UnsubscribeEvents(stats);
            condition.Changed -= OnConditionChanged;

            UnsubscribeCondition(stats, condition.AndCondition);
            UnsubscribeCondition(stats, condition.OrCondition);
        }
    }

    private void OnConditionChanged(Condition condition) => ConditionChanged?.Invoke(this, condition);
}
