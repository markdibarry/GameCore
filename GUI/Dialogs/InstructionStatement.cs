using System.Text.Json.Serialization;

namespace GameCore.GUI;

public class InstructionStatement : IStatement
{
    [JsonConstructor]
    public InstructionStatement(int index, GoTo next)
    {
        Index = index;
        Next = next;
    }

    public int Index { get; }
    public GoTo Next { get; set; }
}
