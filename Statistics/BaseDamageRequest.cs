using Godot;

namespace GameCore.Statistics;

public abstract class BaseDamageRequest
{
    protected BaseDamageRequest()
    {
        SourceName = string.Empty;
        Value = 1;
    }

    public string SourceName { get; set; }
    public Vector2 SourcePosition { get; set; }
    public int Value { get; set; }
}
