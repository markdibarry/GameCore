using System.Text.Json.Serialization;

namespace GameCore.GUI;

public class LineData : IStatement
{
    [JsonConstructor]
    public LineData(ushort[] instructionIndices, GoTo next, ushort[] speakerIndices, string text)
    {
        InstructionIndices = instructionIndices;
        Next = next;
        SpeakerIndices = speakerIndices;
        Text = text;
    }

    public ushort[] InstructionIndices { get; set; }
    public GoTo Next { get; set; }
    public ushort[] SpeakerIndices { get; set; }
    public string Text { get; set; }
}
