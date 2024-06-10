using System;
using System.Diagnostics.CodeAnalysis;

namespace GameCore.Statistics;

public static class StatsLocator
{
    private static IConditionTypeDB? s_conditionTypeDB;
    private static readonly NullConditionTypeDB s_nullConditionTypeDB = new();
    private static IStatTypeDB? s_statTypeDB;
    private static readonly NullStatTypeDB s_nullStatTypDB = new();
    private static IStatusEffectDB? s_statusEffectDB;
    private static readonly NullStatusEffectDB s_nullStatusEffectDB = new();
    private static IStatusEffectModifierFactory? s_statusEffectModifierFactory;
    private static readonly NullStatusEffectModifierFactory s_nullStatusEffectModifierFactory = new();
    public static IConditionTypeDB ConditionTypeDB => s_conditionTypeDB ?? s_nullConditionTypeDB;
    public static IStatTypeDB StatTypeDB => s_statTypeDB ?? s_nullStatTypDB;
    public static IStatusEffectDB StatusEffectDB => s_statusEffectDB ?? s_nullStatusEffectDB;
    public static IStatusEffectModifierFactory StatusEffectModifierFactory => s_statusEffectModifierFactory ?? s_nullStatusEffectModifierFactory;

    public static void ProvideConditionLookup(IConditionTypeDB conditionLookup)
    {
        s_conditionTypeDB = conditionLookup;
    }

    public static void ProvideStatTypeDB(IStatTypeDB statTypeDB) => s_statTypeDB = statTypeDB;

    public static void ProvideStatusEffectDB(IStatusEffectDB statusEffectDB) => s_statusEffectDB = statusEffectDB;

    public static void ProvideStatusEffectModifierFactory(IStatusEffectModifierFactory factory)
    {
        s_statusEffectModifierFactory = factory;
    }

    private class NullStatusEffectDB : IStatusEffectDB
    {
        public bool TryGetEffectData(string statTypeId, [MaybeNullWhen(false)] out StatusEffectData data)
        {
            data = default;
            return false;
        }
    }

    private class NullStatTypeDB : IStatTypeDB
    {
        public string[] GetTypeNames() => [];
        public string[]? GetValueEnumOptions(string statTypeId) => [];
    }

    private class NullConditionTypeDB : IConditionTypeDB
    {
        public Condition CloneCondition(Condition condition)
        {
            throw new NotImplementedException();
        }

        public Type? GetConditionType(string conditionType)
        {
            throw new NotImplementedException();
        }
    }

    private class NullStatusEffectModifierFactory : IStatusEffectModifierFactory
    {
        public Modifier GetStatusEffectModifier(string statusEffectTypeId) => new(
            statType: string.Empty,
            op: string.Empty,
            value: 1,
            condition: null,
            isHidden: false);
    }
}
