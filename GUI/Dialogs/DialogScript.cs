using System.Text.Json.Serialization;

namespace GameCore.GUI;

public class DialogScript
{
    [JsonConstructor]
    public DialogScript(
        string[] speakerIds,
        float[] instFloats,
        string[] instStrings,
        Choice[] choices,
        ushort[][] choiceSets,
        Section[] sections,
        LineData[] lines,
        InstructionStatement[] instructionStmts,
        ushort[][] instructions)
    {
        SpeakerIds = speakerIds;
        InstFloats = instFloats;
        InstStrings = instStrings;
        Choices = choices;
        ChoiceSets = choiceSets;
        Sections = sections;
        Lines = lines;
        InstructionStmts = instructionStmts;
        Instructions = instructions;
    }

    public string[] SpeakerIds { get; set; }
    public float[] InstFloats { get; set; }
    public string[] InstStrings { get; set; }
    public Choice[] Choices { get; set; }
    public ushort[][] ChoiceSets { get; set; }
    public Section[] Sections { get; set; }
    public LineData[] Lines { get; set; }
    public InstructionStatement[] InstructionStmts { get; set; }
    public ushort[][] Instructions { get; set; }
}
