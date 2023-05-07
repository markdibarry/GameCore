using System.Text.Json.Serialization;

namespace GameCore.GUI;

public class Choice : IStatement
{
    [JsonConstructor]
    public Choice(GoTo next, string text)
    {
        Next = next;
        Text = text;
    }

    public GoTo Next { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool Disabled { get; set; }
}
