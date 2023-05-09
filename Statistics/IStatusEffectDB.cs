namespace GameCore.Statistics;

public interface IStatusEffectDB
{
    StatusEffectData? GetEffectData(int type);
}
