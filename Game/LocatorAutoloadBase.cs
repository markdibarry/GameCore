using Godot;

namespace GameCore;

[Tool]
public abstract partial class LocatorAutoloadBase : Node
{
    public override void _Process(double delta)
    {
        if (!Locator.Initialized)
            InitializeBase();
    }

    public override void _Ready() => InitializeBase();

    protected abstract void Initialize();

    private void InitializeBase()
    {
        Initialize();
        Locator.SetInitialized();
    }
}
