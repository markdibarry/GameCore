using System.Text.Json.Serialization;
using Godot;
using Gictionary = Godot.Collections.Dictionary;

namespace GameCore.Statistics;

[Tool]
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

    public override Godot.Collections.Array<Gictionary> _GetPropertyList()
    {
        return StatsLocator.StatTypeDB.GetStatPropertyList(_statType);
    }
}
