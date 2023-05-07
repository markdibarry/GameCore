using System.Collections.Generic;
using System.Linq;
using GameCore.Utility;
using Godot;

namespace GameCore;

public partial class GameCamera : Camera2D
{
    public Node2D? CurrentTarget { get; set; }
    private Vector2 _viewSize;
    private int _goalLimitTop;
    private int _goalLimitBottom;
    private int _goalLimitLeft;
    private int _goalLimitRight;
    private bool _limitsDirty;
    private readonly int _limitUpdateSpeed = 3;

    public override void _Ready()
    {
        _viewSize = GetViewportRect().Size;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (CurrentTarget != null)
        {
            if (IsInstanceValid(CurrentTarget))
                GlobalPosition = CurrentTarget.GlobalPosition;
            else
                CurrentTarget = null;
        }

        if (_limitsDirty)
            UpdateLimits();
    }

    public Rect2 GetRect()
    {
        Vector2 center = GetScreenCenterPosition();
        return new(new(center.X - (_viewSize.X * 0.5f), center.Y - (_viewSize.Y * 0.5f)), _viewSize);
    }

    public bool IsInView(Vector2 position) => GetRect().HasPoint(position);

    public IEnumerable<T> FilterInView<T>(IEnumerable<T> node) where T : Node2D
    {
        Rect2 rect = GetRect();
        return node.Where(x => rect.HasPoint(x.GlobalPosition));
    }

    public void UpdateLimits()
    {
        if (Locator.Root.GameState.MenuActive)
            return;
        bool finishedUpdating = true;
        if (_goalLimitTop != LimitTop)
        {
            LimitTop = LimitTop.MoveTowards(_goalLimitTop, _limitUpdateSpeed);
            finishedUpdating = false;
        }
        if (_goalLimitRight != LimitRight)
        {
            LimitRight = LimitRight.MoveTowards(_goalLimitRight, _limitUpdateSpeed);
            finishedUpdating = false;
        }
        if (_goalLimitBottom != LimitBottom)
        {
            LimitBottom = LimitBottom.MoveTowards(_goalLimitBottom, _limitUpdateSpeed);
            finishedUpdating = false;
        }
        if (_goalLimitLeft != LimitLeft)
        {
            LimitLeft = LimitLeft.MoveTowards(_goalLimitLeft, _limitUpdateSpeed);
            finishedUpdating = false;
        }
        if (finishedUpdating)
            _limitsDirty = false;
    }

    public void SetGoalLimits(int top, int right, int bottom, int left)
    {
        _limitsDirty = true;
        Vector2 cameraPosition = GetScreenCenterPosition() - _viewSize * 0.5f;
        _goalLimitTop = top;
        _goalLimitRight = right;
        _goalLimitBottom = bottom;
        _goalLimitLeft = left;

        if (cameraPosition.Y < _goalLimitTop)
            LimitTop = (int)cameraPosition.Y;

        if (cameraPosition.X + _viewSize.X > _goalLimitRight)
            LimitRight = (int)(cameraPosition.X + _viewSize.X);

        if (cameraPosition.Y + _viewSize.Y > _goalLimitBottom)
            LimitBottom = (int)(cameraPosition.Y + _viewSize.Y);

        if (cameraPosition.X < _goalLimitLeft)
            LimitLeft = (int)cameraPosition.X;
    }
}
