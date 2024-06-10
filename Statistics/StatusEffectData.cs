using System;
using System.Collections.Generic;

namespace GameCore.Statistics;

public class StatusEffectData
{
    public string Name { get; init; } = string.Empty;
    public string AbbrName { get; init; } = string.Empty;
    public string PastTenseName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string EffectType { get; init; } = string.Empty;
    public Condition? DurationCondition { get; init; }
    public Action<StatsBase, StatusEffect>? EnterEffect { get; init; }
    public Action<StatsBase, StatusEffect>? ExitEffect { get; init; }
    public IReadOnlyCollection<Modifier> EffectModifiers { get; init; } = [];
    public Condition? IntervalCondition { get; init; }
    public Action<StatsBase, StatusEffect>? IntervalEffect { get; init; }
    public bool Refreshable { get; init; }
    public bool Stackable { get; init; }
}
