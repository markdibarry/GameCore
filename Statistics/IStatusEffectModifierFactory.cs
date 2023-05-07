namespace GameCore.Statistics;

public interface IStatusEffectModifierFactory
{
    Modifier GetStatusEffectModifier(int statusEffectType);
}
