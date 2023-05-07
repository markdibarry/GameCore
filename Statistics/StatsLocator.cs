using System.Collections.Generic;

namespace GameCore.Statistics;

public static class StatsLocator
{
    private static IConditionLookup s_conditionLookup = null!;
    private static AStatTypeDB s_statTypeDB = null!;
    private static AStatusEffectDB s_statusEffectDB = null!;
    private static IStatusEffectModifierFactory s_statusEffectModifierFactory = null!;
    public static IConditionLookup ConditionLookup => s_conditionLookup;
    public static AStatTypeDB StatTypeDB => s_statTypeDB;
    public static AStatusEffectDB StatusEffectDB => s_statusEffectDB;
    public static IStatusEffectModifierFactory StatusEffectModifierFactory => s_statusEffectModifierFactory;

    public static void ProvideConditionLookup(IConditionLookup conditionLookup)
    {
        s_conditionLookup = conditionLookup;
    }

    public static void ProvideStatTypeDB(AStatTypeDB statTypeDB) => s_statTypeDB = statTypeDB;

    public static void ProvideStatusEffectDB(AStatusEffectDB statusEffectDB) => s_statusEffectDB = statusEffectDB;

    public static void ProvideStatusEffectModifierFactory(IStatusEffectModifierFactory factory)
    {
        s_statusEffectModifierFactory = factory;
    }

    public static List<string> CheckReferences()
    {
        List<string> unsetRefs = new();
        if (s_conditionLookup == null)
            unsetRefs.Add("Condition Lookup");
        if (s_statTypeDB == null)
            unsetRefs.Add("Stat Type DB");
        if (s_statusEffectDB == null)
            unsetRefs.Add("Status Effect DB");
        if (s_statusEffectModifierFactory == null)
            unsetRefs.Add("StatusEffectModifier Factory");
        return unsetRefs;
    }
}
