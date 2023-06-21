using System.Text.Json.Serialization;
using Godot;
using GCol = Godot.Collections;

namespace GameCore.Statistics;

[Tool, GlobalClass]
public partial class Stat : Resource
{
    public Stat() { }

    [JsonConstructor]
    public Stat(int statType, int value, int maxValue = 999)
    {
        StatType = statType;
        Value = value;
        MaxValue = maxValue;
    }

    public Stat(Stat stat)
        : this(stat.StatType, stat.Value, stat.MaxValue)
    { }

    private int _statType;
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
    [Export] public int MaxValue { get; set; }

    public override GCol.Array<GCol.Dictionary> _GetPropertyList()
    {
        return StatsLocator.StatTypeDB.GetStatPropertyList(_statType);
    }
}
