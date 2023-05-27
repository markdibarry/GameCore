namespace GameCore.Input;
public interface IInputAction
{
    float ActionStrength { get; set; }
    string Alias { get; set; }
    bool IsActionPressed { get; set; }
    bool IsActionJustPressed { get; set; }
    bool IsActionJustReleased { get; }
    bool UserInputDisabled { get; set; }
    bool ShouldClear { get; }
    void ClearOneTimeActions();
}
