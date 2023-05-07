namespace GameCore.Input;

public sealed class UserInputAction : InputAction
{
    public UserInputAction(InputHandler inputHandler, string alias)
    {
        _inputHandler = inputHandler;
        Alias = alias;
    }

    private readonly InputHandler _inputHandler;
    public override float ActionStrength
    {
        get
        {
            if (SimulatedStrength != 0)
                return SimulatedStrength;
            if (IsActionPressed)
                return 1;
            if (_inputHandler.UserInputDisabled || UserInputDisabled)
                return 0;
            return Godot.Input.GetActionStrength(Alias);
        }
        set => SimulatedStrength = value;
    }
    public override bool IsActionPressed
    {
        get
        {
            if (SimulatedPress || SimulatedJustPressed)
                return true;
            if (_inputHandler.UserInputDisabled || UserInputDisabled)
                return false;
            return Godot.Input.IsActionPressed(Alias);
        }
        set
        {
            if (value && !SimulatedPress)
            {
                SimulatedPress = true;
                SimulatedJustPressed = true;
            }
            else if (!value && SimulatedPress)
            {
                SimulatedPress = false;
                SimulatedJustReleased = true;
            }
        }
    }
    public override bool IsActionJustPressed
    {
        get
        {
            if (SimulatedJustPressed)
                return true;
            if (_inputHandler.UserInputDisabled || UserInputDisabled)
                return false;
            return Godot.Input.IsActionJustPressed(Alias);
        }
        set => SimulatedJustPressed = value;
    }
    public override bool IsActionJustReleased
    {
        get
        {
            if (SimulatedJustReleased)
                return true;
            if (_inputHandler.UserInputDisabled || UserInputDisabled)
                return false;
            return Godot.Input.IsActionJustReleased(Alias);
        }
    }
}
