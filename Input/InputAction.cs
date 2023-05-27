using System;

namespace GameCore.Input;

public class InputAction : IInputAction
{
    public InputAction(IInputHandler inputHandler, string alias)
        : this(inputHandler)
    {
        Alias = alias;
    }

    public InputAction(IInputHandler inputHandler)
    {
        InputHandler = inputHandler;
    }

    public float ActionStrength
    {
        get
        {
            if (SimulatedStrength != 0)
                return SimulatedStrength;
            if (IsActionPressed)
                return 1;
            if (InputHandler.UserInputDisabled || UserInputDisabled)
                return 0;
            return Godot.Input.GetActionStrength(Alias);
        }
        set => SimulatedStrength = Math.Clamp(value, 0, 1);
    }
    public string Alias { get; set; } = string.Empty;
    public bool IsActionPressed
    {
        get
        {
            if (SimulatedPress || SimulatedJustPressed)
                return true;
            if (InputHandler.UserInputDisabled || UserInputDisabled)
                return false;
            return Godot.Input.IsActionPressed(Alias);
        }
        set
        {
            if (value && !SimulatedPress)
            {
                SimulatedPress = true;
                SimulatedJustPressed = true;
                InputHandler.AddActionToClear(this);
            }
            else if (!value && SimulatedPress)
            {
                SimulatedPress = false;
                SimulatedJustReleased = true;
                InputHandler.AddActionToClear(this);
            }
        }
    }
    public bool IsActionJustPressed
    {
        get
        {
            if (SimulatedJustPressed)
                return true;
            if (InputHandler.UserInputDisabled || UserInputDisabled)
                return false;
            return Godot.Input.IsActionJustPressed(Alias);
        }
        set
        {
            SimulatedJustPressed = value;
            InputHandler.AddActionToClear(this);
        }
    }
    public bool IsActionJustReleased
    {
        get
        {
            if (SimulatedJustReleased)
                return true;
            if (InputHandler.UserInputDisabled || UserInputDisabled)
                return false;
            return Godot.Input.IsActionJustReleased(Alias);
        }
    }
    public bool UserInputDisabled { get; set; }
    public bool ShouldClear => !SimulatedJustPressed && !SimulatedJustReleased;
    protected bool SimulatedJustPressed { get; set; }
    protected bool SimulatedJustReleased { get; set; }
    protected bool SimulatedPress { get; set; }
    protected float SimulatedStrength { get; set; }
    protected IInputHandler InputHandler { get; }

    public void ClearOneTimeActions()
    {
        SimulatedJustReleased = SimulatedJustPressed && !SimulatedPress;
        SimulatedJustPressed = false;
    }
}
