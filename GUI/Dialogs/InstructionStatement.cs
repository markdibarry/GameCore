using System.Text.Json.Serialization;

namespace GameCore.GUI;

public readonly struct InstructionStatement : IStatement
{
    [JsonConstructor]
    public InstructionStatement(int index, GoTo next)
    {
        Index = index;
        Next = next;
    }

    public int Index { get; }
    public GoTo Next { get; }
}
