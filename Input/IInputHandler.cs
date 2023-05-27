using Godot;

namespace GameCore.Input;

public interface IInputHandler
{
    IInputAction Up { get; }
    IInputAction Down { get; }
    IInputAction Left { get; }
    IInputAction Right { get; }
    bool UserInputDisabled { get; set; }
    public Vector2 GetLeftAxis();
    public void SetLeftAxis(Vector2 newVector);
    public void Update();
    public void AddActionToClear(IInputAction action);
}
