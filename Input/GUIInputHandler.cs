namespace GameCore.Input;

public abstract class GUIInputHandler : InputHandler
{
    public abstract InputAction Accept { get; }
    public abstract InputAction Cancel { get; }
    public abstract InputAction Start { get; }

    public override void Update()
    {
        base.Update();
        Accept.ClearOneTimeActions();
        Cancel.ClearOneTimeActions();
        Start.ClearOneTimeActions();
    }
}
