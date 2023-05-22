using Godot;

namespace GameCore.Statistics;

public interface IDamageRequest
{
    string SourceName { get; set; }
    Vector2 SourcePosition { get; set; }
    int Value { get; set; }
}
