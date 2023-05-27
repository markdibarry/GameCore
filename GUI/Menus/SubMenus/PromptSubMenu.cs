using GameCore.Input;
using Godot;

namespace GameCore.GUI;

[Tool]
public partial class PromptSubMenu : SubMenu
{
    [Export]
    private double _timeDuration;
    private bool _timerEnabled;

    public override void HandleInput(IGUIInputHandler menuInput, double delta)
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

    protected sealed override void OnSetupInternal()
    {
        if (_timeDuration > 0)
            _timerEnabled = true;
        base.OnSetupInternal();
    }

    protected virtual void OnTimeOut()
    {
        _timerEnabled = false;
    }
}
