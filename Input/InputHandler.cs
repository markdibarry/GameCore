using System.Collections.Generic;
using Godot;

namespace GameCore.Input;

public abstract class InputHandler : IInputHandler
{
    protected InputHandler(string up, string down, string left, string right)
    {
        Up = new InputAction(this, up);
        Down = new InputAction(this, down);
        Left = new InputAction(this, left);
        Right = new InputAction(this, right);
    }

    private readonly HashSet<IInputAction> _pendingActionsToClear = new();
    public IInputAction Up { get; }
    public IInputAction Down { get; }
    public IInputAction Left { get; }
    public IInputAction Right { get; }
    public bool UserInputDisabled { get; set; }

    public Vector2 GetLeftAxis()
    {
        Vector2 vector;
        vector.X = Right.ActionStrength - Left.ActionStrength;
        vector.Y = Down.ActionStrength - Up.ActionStrength;
        return vector.Normalized();
    }

    public void SetLeftAxis(Vector2 newVector)
    {
        Down.ActionStrength = Mathf.Max(newVector.Y, 0);
        Up.ActionStrength = Mathf.Min(newVector.Y, 0) * -1;
        Right.ActionStrength = Mathf.Max(newVector.X, 0);
        Left.ActionStrength = Mathf.Min(newVector.X, 0) * -1;
    }

    public void Update()
    {
        foreach (IInputAction action in _pendingActionsToClear)
            action.ClearOneTimeActions();
        _pendingActionsToClear.RemoveWhere(x => x.ShouldClear);
    }

    public void AddActionToClear(IInputAction action) => _pendingActionsToClear.Add(action);
}
