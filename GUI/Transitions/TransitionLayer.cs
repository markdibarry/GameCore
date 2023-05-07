using Godot;

public partial class TransitionLayer : CanvasLayer
{
    [Export] public Control LoadingScreenContainer { get; set; } = null!;
}
