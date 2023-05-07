using System.Threading.Tasks;
using Godot;

namespace GameCore.GUI;

public abstract partial class Transition : Control
{
    public virtual Task TransistionFrom()
    {
        return Task.CompletedTask;
    }

    public virtual Task TransitionTo()
    {
        return Task.CompletedTask;
    }
}
