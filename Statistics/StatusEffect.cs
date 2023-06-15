using System;

namespace GameCore.Statistics;

public class StatusEffect : IStatusEffect
{
    public StatusEffect(StatusEffectData effectData)
    {
        EffectData = effectData;
        TickCondition = effectData.TickCondition?.Clone();
    }

    public Condition? TickCondition { get; }
    public StatusEffectData EffectData { get; }
    public int EffectType => EffectData.EffectType;
    public event Action<Condition>? ConditionUpdated;
    public event Action<IStatusEffect, Condition>? ConditionChanged;

    public void HandleChanges(BaseStats stats, Condition condition)
    {
        if (TickCondition == null || !TickCondition.CheckIfConditionsMet(stats))
            return;
        EffectData.TickEffect?.Invoke(stats, this);
        TickCondition.Reset();
    }

    public void SubscribeConditions(BaseStats stats)
    {
        Condition? nextCondition = TickCondition;
        while (nextCondition != null)
        {
            nextCondition.SubscribeEvents(stats);
            nextCondition.Updated += OnConditionUpdated;
            nextCondition.StatusChanged += OnConditionChanged;
            nextCondition = nextCondition.AdditionalCondition;
        }
    }

    public void UnsubscribeConditions(BaseStats stats)
    {
        Condition? nextCondition = TickCondition;
        while (nextCondition != null)
        {
            nextCondition.UnsubscribeEvents(stats);
            nextCondition.Updated -= OnConditionUpdated;
            nextCondition.StatusChanged -= OnConditionChanged;
            nextCondition = nextCondition.AdditionalCondition;
        }
    }

    private void OnConditionUpdated(Condition condition) => ConditionUpdated?.Invoke(condition);

    private void OnConditionChanged(Condition condition) => ConditionChanged?.Invoke(this, condition);
}
