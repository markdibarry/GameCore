using System;

namespace GameCore.Statistics;

public static class StatsLocator
{
    private static IConditionTypeDB? s_conditionTypeDB;
    private static readonly NullConditionTypeDB s_nullConditionTypeDB = new();
    private static BaseStatTypeDB? s_statTypeDB;
    private static readonly NullStatTypeDB s_nullStatTypDB = new();
    private static IStatusEffectDB? s_statusEffectDB;
    private static readonly NullStatusEffectDB s_nullStatusEffectDB = new();
    private static IStatusEffectModifierFactory? s_statusEffectModifierFactory;
    private static readonly NullStatusEffectModifierFactory s_nullStatusEffectModifierFactory = new();
    public static IConditionTypeDB ConditionTypeDB => s_conditionTypeDB ?? s_nullConditionTypeDB;
    public static BaseStatTypeDB StatTypeDB => s_statTypeDB ?? s_nullStatTypDB;
    public static IStatusEffectDB StatusEffectDB => s_statusEffectDB ?? s_nullStatusEffectDB;
    public static IStatusEffectModifierFactory StatusEffectModifierFactory => s_statusEffectModifierFactory ?? s_nullStatusEffectModifierFactory;

    public static void ProvideConditionLookup(IConditionTypeDB conditionLookup)
    {
        s_conditionTypeDB = conditionLookup;
    }

    public static void ProvideStatTypeDB(BaseStatTypeDB statTypeDB) => s_statTypeDB = statTypeDB;

    public static void ProvideStatusEffectDB(IStatusEffectDB statusEffectDB) => s_statusEffectDB = statusEffectDB;

    public static void ProvideStatusEffectModifierFactory(IStatusEffectModifierFactory factory)
    {
        s_statusEffectModifierFactory = factory;
    }

    private class NullStatusEffectDB : IStatusEffectDB
    {
        public StatusEffectData? GetEffectData(int type) => null;
    }

    private class NullStatTypeDB : BaseStatTypeDB
    {
        public override string[] GetTypeNames() => Array.Empty<string>();
        public override string[]? GetValueEnumOptions(int statType) => Array.Empty<string>();
    }

    private class NullConditionTypeDB : IConditionTypeDB
    {
        public Type? GetConditionType(int conditionType) => null;
    }

    private class NullStatusEffectModifierFactory : IStatusEffectModifierFactory
    {
        public Modifier GetStatusEffectModifier(int statusEffectType) => new(
            statType: 0,
            op: ModOp.None,
            value: 1,
            conditions: null,
            isHidden: false);
    }
}
