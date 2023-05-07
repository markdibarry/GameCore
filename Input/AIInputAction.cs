namespace GameCore.Input;

public class AIInputAction : InputAction
{
    public override float ActionStrength
    {
        get
        {
            if (SimulatedStrength != 0)
                return SimulatedStrength;
            return IsActionPressed ? 1 : 0;
        }
        set => SimulatedStrength = value;
    }
    public override bool IsActionPressed
    {
        get => SimulatedPress || SimulatedJustPressed;
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
        get => SimulatedJustPressed;
        set => SimulatedJustPressed = value;
    }
    public override bool IsActionJustReleased => SimulatedJustReleased;
}
