using Godot;

namespace GameCore.Statistics;

public interface IDamageResult
{
    string RecieverName { get; set; }
    string SourceName { get; set; }
    Vector2 SourcePosition { get; set; }
    int TotalDamage { get; set; }
}
