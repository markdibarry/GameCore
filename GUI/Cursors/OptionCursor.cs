using Godot;

namespace GameCore.GUI;

[Tool]
public abstract partial class OptionCursor : Sprite2D
{
    public override void _PhysicsProcess(double delta)
    {
        AnimateIdle(delta);
    }

    public virtual void EnableSelectionMode() { }

    public virtual void DisableSelectionMode() { }

    public abstract void MoveToTarget(Control target);

    protected virtual void AnimateIdle(double delta) { }
}
