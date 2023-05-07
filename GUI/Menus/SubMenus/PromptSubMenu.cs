using GameCore.Input;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class PromptSubMenu : SubMenu
{
    [Export]
    private double _timeDuration;
    private bool _timerEnabled;

    public override void HandleInput(GUIInputHandler menuInput, double delta)
    {
        base.HandleInput(menuInput, delta);

        if (menuInput.Accept.IsActionJustPressed)
            Confirm();

        if (_timerEnabled)
        {
            if (_timeDuration < 0)
                OnTimeOut();
            else
                _timeDuration -= delta;
        }
    }

    protected virtual void Confirm() { }

    protected override void CustomSetup()
    {
        if (_timeDuration > 0)
            _timerEnabled = true;
    }

    protected virtual void OnTimeOut()
    {
        _timerEnabled = false;
    }
}
