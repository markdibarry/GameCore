using Godot;

namespace GameCore.GUI;

[Tool]
public abstract partial class OptionCursor : Sprite2D
{
    private bool _isHidden;
    public bool IsHidden
    {
        get => _isHidden;
        set
        {
            _isHidden = value;
            Visible = false;
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        AnimateIdle(delta);
    }

    public virtual void EnableSelectionMode() { }

    public virtual void DisableSelectionMode() { }

    public abstract void MoveToTarget(Control target);

    protected virtual void AnimateIdle(double delta) { }
}
