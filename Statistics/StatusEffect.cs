using System;
using System.Text.Json.Serialization;

namespace GameCore.Statistics;

public record StatusEffect
{
    public StatusEffect(string effectType, Condition? durationCondition, Condition? tickCondition)
    {
        if (!StatsLocator.StatusEffectDB.TryGetEffectData(effectType, out StatusEffectData? effectData))
            throw new Exception($"Status effect type not found: {effectType}");

        EffectData = effectData;
        DurationCondition = durationCondition;
        TickCondition = tickCondition;
    }

    public StatusEffect(StatusEffectData effectData)
    {
        EffectData = effectData;

        if (effectData.IntervalCondition != null)
            TickCondition = effectData.IntervalCondition with { };

        if (effectData.DurationCondition != null)
            DurationCondition = effectData.DurationCondition with { };
    }

    [JsonIgnore]
    public StatusEffectData EffectData { get; }
    public string EffectType => EffectData.EffectType;
    public Condition? DurationCondition { get; set; }
    public Condition? TickCondition { get; set; }
    [JsonIgnore]
    public bool IsActive { get; set; }
    public event Action<StatusEffect, Condition>? ConditionChanged;

    public void InvokeTick(StatsBase stats, Condition condition)
    {
        if (!condition.CheckAllConditions(stats))
            return;

        EffectData.IntervalEffect?.Invoke(stats, this);
        condition.Reset();
    }

    public bool ShouldRemove(StatsBase? stats = null)
    {
        if (DurationCondition == null)
            return false;

        return DurationCondition.CheckAllConditions(stats);
    }

    public void SubscribeConditions(StatsBase stats)
    {
        SubscribeCondition(stats, TickCondition);
        SubscribeCondition(stats, DurationCondition);

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
        UnsubscribeCondition(stats, TickCondition);
        UnsubscribeCondition(stats, DurationCondition);

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
