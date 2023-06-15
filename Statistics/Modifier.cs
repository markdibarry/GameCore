using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Godot;
using Gictionary = Godot.Collections.Dictionary;

namespace GameCore.Statistics;

[Tool, GlobalClass]
public partial class Modifier : Resource
{
    public Modifier() { }

    [JsonConstructor]
    public Modifier(
        int statType,
        ModOp op,
        int value,
        Godot.Collections.Array<Condition>? conditions = null,
        bool isHidden = false)
    {
        StatType = statType;
        Op = op;
        Value = value;
        Conditions = conditions ?? new();
        IsHidden = isHidden;
    }

    public Modifier(Modifier mod)
        : this(
            statType: mod.StatType,
            op: mod.Op,
            value: mod.Value,
            conditions: new(mod.Conditions.Select(x => x.Clone())),
            isHidden: mod.IsHidden)
    {
    }

    public Modifier(ModifierRef modRef)
        : this(
            statType: modRef.Modifier.StatType,
            op: modRef.Op,
            value: modRef.Value,
            conditions: new(modRef.Conditions.Select(x => x.Clone())),
            isHidden: modRef.IsHidden)
    {
    }

    private int _statType;
    [Export] public bool IsHidden { get; set; }
    [Export] public ModOp Op { get; set; }
    [Export] public Godot.Collections.Array<Condition> Conditions { get; set; } = new();
    public int StatType
    {
        get => _statType;
        set
        {
            _statType = value;
            NotifyPropertyListChanged();
        }
    }
    public int Value { get; set; }

    // TODO Add special case handling i.e. +5% for every 100 enemies killed
    public int Apply(int baseValue) => Op.Compute(baseValue, Value);

    public static bool ShouldDeactivate(BaseStats stats, IEnumerable<Condition> conditions)
    {
        return conditions.Any(x => x.ResultType.HasFlag(ConditionResultType.Deactivate) && x.CheckIfConditionsMet(stats));
    }

    public static bool ShouldRemove(BaseStats stats, IEnumerable<Condition> conditions)
    {
        return conditions.Any(x => x.ResultType.HasFlag(ConditionResultType.Remove) && x.CheckIfConditionsMet(stats));
    }

    public override Godot.Collections.Array<Gictionary> _GetPropertyList()
    {
        return StatsLocator.StatTypeDB.GetStatPropertyList(_statType);
    }
}
