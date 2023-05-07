using Godot;

namespace GameCore.AreaScenes;

public partial class AreaCameraLimit : Area2D
{
    [Export] public int LimitTop { get; set; } = -10000000;
    [Export] public int LimitRight { get; set; } = 10000000;
    [Export] public int LimitBottom { get; set; } = 10000000;
    [Export] public int LimitLeft { get; set; } = 0;

    public override void _Ready()
    {
        BodyEntered += OnEntered;
    }

    public void OnEntered(Node2D node2d)
    {
        var camera = Locator.Root?.GameCamera;
        if (node2d != camera?.CurrentTarget)
            return;
        camera.SetGoalLimits(LimitTop, LimitRight, LimitBottom, LimitLeft);
    }
}
