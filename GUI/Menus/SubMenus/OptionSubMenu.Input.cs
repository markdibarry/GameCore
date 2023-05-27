using GameCore.Input;

namespace GameCore.GUI;

public partial class OptionSubMenu : SubMenu
{
    private Direction _currentDirection;
    private double _rapidScrollTimer;
    private bool _rapidScrollTimerEnabled;
    private readonly double _rapidScrollDelay = 0.4;
    private readonly double _rapidScrollInterval = 0.05;

    public override void HandleInput(IGUIInputHandler menuInput, double delta)
    {
        base.HandleInput(menuInput, delta);

        if (CurrentContainer == null)
            return;
        if (menuInput.Accept.IsActionJustPressed)
        {
            OnSelectPressedInternal();
            return;
        }
        //else if (menuInput.Cancel.IsActionJustPressed && !PreventCancel)
        //{
        //    _ = CloseSubMenuAsync();
        //    return;
        //}
        //else if (menuInput.Start.IsActionJustPressed && !PreventCloseAll)
        //{
        //    _ = CloseMenuAsync();
        //    return;
        //}

        HandleRapidScroll(delta, GetDirection(menuInput));
    }

    protected static Direction GetDirection(IInputHandler menuInput)
    {
        Direction newDirection = Direction.None;

        if (menuInput.Up.IsActionPressed)
            newDirection = Direction.Up;
        else if (menuInput.Down.IsActionPressed)
            newDirection = Direction.Down;
        else if (menuInput.Left.IsActionPressed)
            newDirection = Direction.Left;
        else if (menuInput.Right.IsActionPressed)
            newDirection = Direction.Right;
        return newDirection;
    }

    private void HandleRapidScroll(double delta, Direction newDirection)
    {
        if (newDirection == _currentDirection)
        {
            if (!_rapidScrollTimerEnabled)
                return;
            if (_rapidScrollTimer > 0)
            {
                _rapidScrollTimer -= delta;
                return;
            }

            _rapidScrollTimer = _rapidScrollInterval;
            CurrentContainer?.FocusDirection(_currentDirection);
            return;
        }

        _currentDirection = newDirection;
        if (newDirection == Direction.None)
        {
            _rapidScrollTimerEnabled = false;
            return;
        }

        _rapidScrollTimerEnabled = true;
        _rapidScrollTimer = _rapidScrollDelay;
        CurrentContainer?.FocusDirection(_currentDirection);
    }
}
