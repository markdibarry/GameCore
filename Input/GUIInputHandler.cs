namespace GameCore.Input;

public class GUIInputHandler : InputHandler, IGUIInputHandler
{
    public GUIInputHandler(
        string up,
        string down,
        string left,
        string right,
        string accept,
        string cancel,
        string start)
            : base(up, down, left, right)
    {
        Accept = new InputAction(this, accept);
        Cancel = new InputAction(this, cancel);
        Start = new InputAction(this, start);
    }

    public IInputAction Accept { get; }
    public IInputAction Cancel { get; }
    public IInputAction Start { get; }
}
