using Godot;

namespace GameCore.AreaScenes;

public partial class AreaCameraLimit : Area2D
{
    [Export] public int LimitTop { get; set; } = -10000000;
    [Export] public int LimitRight { get; set; } = 10000000;
    [Export] public int LimitBottom { get; set; } = 10000000;
    [Export] public int LimitLeft { get; set; } = -10000000;
    public const int CameraLimitLayer = 256;

    public override void _Ready()
    {
        BodyEntered += OnEntered;
    }

    public void TweenLimit(Node2D node2d)
    {
        GameCamera? camera = Locator.Root?.GameCamera;

        if (camera == null || node2d != camera.CurrentTarget)
            return;

        camera.TweenLimit(LimitTop, LimitRight, LimitBottom, LimitLeft);
    }

    private void OnEntered(Node2D node2d)
    {
        TweenLimit(node2d);
    }
}
