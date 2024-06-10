using System;
using System.Text.Json.Serialization;
using GameCore.Utility;

namespace GameCore.Statistics;

[JsonConverter(typeof(ConditionConverter))]
public abstract record Condition : IPoolable<Condition>
{
    public Condition() { }

    protected Condition(Condition condition)
    {
        ConditionType = condition.ConditionType;

        if (condition.AndCondition != null)
            AndCondition = condition.AndCondition with { };

        if (condition.OrCondition != null)
            OrCondition = condition.OrCondition with { };
    }

    protected Condition(
        string conditionType,
        Condition? andCondition,
        Condition? orCondition)
    {
        ConditionType = conditionType;

        if (andCondition != null)
            AndCondition = andCondition with { };

        if (orCondition != null)
            OrCondition = orCondition with { };
    }

    private bool _conditionMet;
    private bool _initialized;
    [JsonPropertyOrder(-5)]
    public string ConditionType { get; set; } = string.Empty;
    [JsonConverter(typeof(ConditionConverter))]
    [JsonPropertyOrder(20)]
    public Condition? AndCondition { get; set; }
    [JsonConverter(typeof(ConditionConverter))]
    [JsonPropertyOrder(21)]
    public Condition? OrCondition { get; set; }
    public event Action<Condition>? Changed;

    public bool CheckAllConditions(StatsBase? stats = null)
    {
        if (stats != null)
        {
            if (IsConditionMet(stats))
                return AndCondition?.IsConditionMet(stats) ?? true;
            else
                return OrCondition?.IsConditionMet(stats) ?? false;
        }
        else
        {
            if (!_initialized)
                throw new Exception("Condition not initialized.");

            if (_conditionMet)
                return AndCondition?.CheckAllConditionsCached() ?? true;
            else
                return OrCondition?.CheckAllConditionsCached() ?? false;
        }
    }

    public void InitializeConditions(StatsBase stats)
    {
        if (_initialized)
            return;

        UpdateCondition(stats);

        AndCondition?.InitializeConditions(stats);
        OrCondition?.InitializeConditions(stats);

        _initialized = true;
    }

    public virtual void Reset()
    {
        AndCondition?.Reset();
        OrCondition?.Reset();
    }

    public void Init(Condition? condition)
    {
        _initialized = false;
        _conditionMet = false;
        ConditionType = condition?.ConditionType ?? string.Empty;

        if (condition?.AndCondition != null)
            AndCondition = condition.AndCondition with { };
        else
            AndCondition = default;

        if (condition?.OrCondition != null)
            OrCondition = condition.OrCondition with { };
        else
            OrCondition = default;

        Changed = default;
        ResetCondition();
    }

    /// <summary>
    /// Updates the _conditionMet flag and returns true if the result is different
    /// than the previous value.
    /// </summary>
    /// <param name="stats"></param>
    /// <returns></returns>
    public bool UpdateCondition(StatsBase stats)
    {
        bool result = IsConditionMet(stats);

        if (result != _conditionMet)
        {
            _conditionMet = result;
            return true;
        }

        return false;
    }

    public abstract void SubscribeEvents(StatsBase stats);
    public abstract void UnsubscribeEvents(StatsBase stats);
    protected abstract bool IsConditionMet(StatsBase stats);
    protected abstract bool ResetCondition();
    protected void RaiseConditionChanged()
    {
        Changed?.Invoke(this);
    }

    private bool CheckAllConditionsCached()
    {
        if (!_initialized)
            throw new Exception("Condition not initialized.");

        if (_conditionMet)
            return AndCondition?.CheckAllConditionsCached() ?? true;
        else
            return OrCondition?.CheckAllConditionsCached() ?? false;
    }
}
