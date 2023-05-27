namespace GameCore.Input;

public interface IGUIInputHandler : IInputHandler
{
    IInputAction Accept { get; }
    IInputAction Cancel { get; }
    IInputAction Start { get; }
}
