using Godot;

namespace GameCore.GUI;

public partial class TransitionLayer : CanvasLayer
{
    [Export] public Control LoadingScreenContainer { get; set; } = null!;
}
