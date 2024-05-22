using System;
using System.Collections.Generic;

namespace GameCore.Statistics;

public class StatusEffectData
{
    public string Name { get; init; } = string.Empty;
    public string AbbrName { get; init; } = string.Empty;
    public string PastTenseName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int EffectType { get; init; }
    public Action<StatsBase, IStatusEffect>? EnterEffect { get; init; }
    public Action<StatsBase, IStatusEffect>? ExitEffect { get; init; }
    public IReadOnlyCollection<Modifier> EffectModifiers { get; init; } = [];
    public Condition? TickCondition { get; init; }
    public Action<StatsBase, IStatusEffect>? TickEffect { get; init; }
}
