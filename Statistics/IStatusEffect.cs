using System;

namespace GameCore.Statistics;

public interface IStatusEffect
{
    StatusEffectData EffectData { get; }
    int EffectType { get; }
    void HandleChanges(BaseStats stats, Condition condition);
    void SubscribeConditions(BaseStats stats);
    void UnsubscribeConditions(BaseStats stats);
    event Action<Condition>? ConditionUpdated;
    event Action<IStatusEffect, Condition>? ConditionChanged;
}
