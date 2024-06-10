namespace GameCore.Statistics;

public interface IStatusEffectModifierFactory
{
    Modifier GetStatusEffectModifier(string statusEffectTypeId);
}
