namespace GameCore.Statistics;

public class StatusEffect : IStatusEffect
{
    public StatusEffect(BaseStats stats, StatusEffectData effectData)
    {
        Stats = stats;
        EffectData = effectData;
        if (effectData.TickCondition != null)
        {
            _tickCondition = effectData.TickCondition.Clone();
            _tickCondition.SetStats(stats);
        }
    }

    private readonly Condition? _tickCondition;
    public StatusEffectData EffectData { get; }
    public int EffectType => EffectData.EffectType;
    public BaseStats Stats { get; }

    public void CallEffectTick()
    {
        if (_tickCondition == null || !_tickCondition.CheckConditions())
            return;
        EffectData.TickEffect?.Invoke(this);
        _tickCondition.Reset();
    }

    public void SubscribeCondition()
    {
        _tickCondition?.Subscribe(CallEffectTick);
    }

    public void UnsubscribeCondition()
    {
        _tickCondition?.Unsubscribe();
    }
}
