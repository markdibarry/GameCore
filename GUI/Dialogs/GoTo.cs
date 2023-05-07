using System.Text.Json.Serialization;

namespace GameCore.GUI;

public readonly struct GoTo
{
    [JsonConstructor]
    public GoTo(StatementType type, int index)
    {
        Type = type;
        Index = index;
    }

    public StatementType Type { get; }
    public int Index { get; }

    public static readonly GoTo Default = new GoTo();
}
