namespace GameCore.Statistics;

public interface IStatusEffect
{
    StatusEffectData EffectData { get; }
    int EffectType { get; }
    BaseStats Stats { get; }
    void CallEffectTick();
    void SubscribeCondition();
    void UnsubscribeCondition();
}
