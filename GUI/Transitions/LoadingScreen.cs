using System.Threading.Tasks;
using GameCore.Game;
using Godot;

namespace GameCore.GUI;

public abstract partial class LoadingScreen : Control
{
    protected Loader? Loader { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        if (Loader != null && Loader.CurrentState == Loader.State.Loading)
            Loader.Update();
    }

    public virtual async Task LoadAsync(Loader loader, string[] paths)
    {
        Loader = loader;
        await Loader.LoadAsync(paths);
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
