using System.Threading.Tasks;
using GameCore.GUI;
using Godot;

namespace GameCore.Events;

public abstract partial class BaseSceneChanger : Area2D
{
    protected BaseTransitionController TController { get; } = Locator.TransitionController;
    protected GUIController GUIController { get; } = Locator.Root.GUIController;
    protected BaseGameSession? GameSession { get; } = Locator.Session;
    [Export] public bool Automatic { get; set; }
    [Export(PropertyHint.File)] public string PackedScenePath { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public override void _Ready()
    {
        if (Automatic)
            BodyEntered += OnBodyEntered;
    }

    public void OnBodyEntered(Node body)
    {
        if (IsActive || PackedScenePath == string.Empty)
            return;
        if (!FileAccess.FileExists(PackedScenePath))
            return;
        IsActive = true;
        ChangeScene();
    }

    /// <summary>
    /// Replaces an AreaScene with another
    /// </summary>
    protected abstract Task ChangeScene();
}
