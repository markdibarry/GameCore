using System;

namespace GameCore.Statistics;

public interface IStatusEffect
{
    StatusEffectData EffectData { get; }
    int EffectType { get; }
    void HandleChanges(StatsBase stats, Condition condition);
    void SubscribeConditions(StatsBase stats);
    void UnsubscribeConditions(StatsBase stats);
    event Action<Condition>? ConditionUpdated;
    event Action<IStatusEffect, Condition>? ConditionChanged;
}
