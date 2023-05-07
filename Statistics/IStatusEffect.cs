namespace GameCore.Statistics;

public interface IStatusEffect
{
    StatusEffectData EffectData { get; }
    int EffectType { get; }
    AStats Stats { get; }
    void CallEffectTick();
    void SubscribeCondition();
    void UnsubscribeCondition();
}
