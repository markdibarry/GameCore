using System.Threading.Tasks;
using Godot;

namespace GameCore.GUI;

public abstract partial class LoadingScreen : Control
{
    public ProgressBar ProgressBar { get; set; } = null!;

    public override void _Ready()
    {
        ProgressBar = GetNode<ProgressBar>("ProgressBar");
    }

    public void Update(int progress)
    {
        ProgressBar.Value = progress;
    }

    public virtual Task TransistionFrom()
    {
        return Task.CompletedTask;
    }

    public virtual Task TransitionTo()
    {
        return Task.CompletedTask;
    }
}
