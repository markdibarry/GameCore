using Godot;

namespace GameCore.Statistics;

public abstract class BaseDamageResult
{
    public string RecieverName { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public Vector2 SourcePosition { get; set; }
    public int TotalDamage { get; set; }
}
