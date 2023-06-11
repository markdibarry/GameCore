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
    public Action<BaseStats, IStatusEffect>? EnterEffect { get; init; }
    public Action<BaseStats, IStatusEffect>? ExitEffect { get; init; }
    public IReadOnlyCollection<Modifier> EffectModifiers { get; init; } = Array.Empty<Modifier>();
    public Condition? TickCondition { get; init; }
    public Action<BaseStats, IStatusEffect>? TickEffect { get; init; }
}
