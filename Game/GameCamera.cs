using System.Collections.Generic;
using System.Linq;
using GameCore.AreaScenes;
using Godot;
using GameCore.Utility;

namespace GameCore;

public partial class GameCamera : Camera2D
{
    public Node2D? CurrentTarget
    {
        get => _currentTarget;
        set => SetCurrentTarget(value);
    }
    private Node2D? _currentTarget;
    private Vector2 _viewSize;
    private readonly float _limitUpdateSpeed = 3 * 60;
    private Tween? _limitTween;
    private bool _switching;
    private float _switchSpeed;

    public override void _Ready()
    {
        _viewSize = GetViewportRect().Size;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Godot.Input.IsActionJustPressed("test"))
        {
            if (CurrentTarget == GetNode<Node2D>("../GameSessionContainer/GameSession/AreaSceneContainer/DemoLevel1/Actors/Twosen"))
            {
                CurrentTarget = GetNode<Node2D>("../GameSessionContainer/GameSession/AreaSceneContainer/DemoLevel1/Scenery/Environment/Barrels/Barrel5");
            }
            else
            {
                CurrentTarget = GetNode<Node2D>("../GameSessionContainer/GameSession/AreaSceneContainer/DemoLevel1/Actors/Twosen");
            }

        }

        if (CurrentTarget == null)
            return;

        if (!IsInstanceValid(CurrentTarget))
        {
            CurrentTarget = null;
            return;
        }

        if (_switching)
        {
            GlobalPosition = GlobalPosition.MoveToward(CurrentTarget.GlobalPosition.Round(), _switchSpeed);

            if (GlobalPosition.DistanceTo(CurrentTarget.GlobalPosition.Round()) < 1)
            {
                _switching = false;
                _switchSpeed = 0;
            }
        }
        else
        {
            GlobalPosition = CurrentTarget.GlobalPosition.Round();
        }
    }

    public Rect2 GetRect()
    {
        return new(GetScreenCenterPosition() - _viewSize * 0.5f, _viewSize);
    }

    public IEnumerable<T> FilterInView<T>(IEnumerable<T> node) where T : Node2D
    {
        Rect2 rect = GetRect();
        return node.Where(x => rect.HasPoint(x.GlobalPosition));
    }

    public void SetLimit(Node2D node2d)
    {
        AreaCameraLimit? acl = node2d.GetFirstAreaAtGlobalPosition<AreaCameraLimit>(AreaCameraLimit.CameraLimitLayer);

        if (acl == null)
            return;

        SetLimit(acl.LimitTop, acl.LimitRight, acl.LimitBottom, acl.LimitLeft);
    }

    public void SetLimit(int top, int right, int bottom, int left)
    {
        _limitTween?.Kill();
        _limitTween = null;

        LimitTop = top;
        LimitRight = right;
        LimitBottom = bottom;
        LimitLeft = left;
    }

    public void TweenLimit(Node2D node2d)
    {
        AreaCameraLimit? acl = node2d.GetFirstAreaAtGlobalPosition<AreaCameraLimit>(AreaCameraLimit.CameraLimitLayer);

        if (acl == null)
            return;

        TweenLimit(acl.LimitTop, acl.LimitRight, acl.LimitBottom, acl.LimitLeft);
    }

    public void TweenLimit(int top, int right, int bottom, int left)
    {
        _limitTween?.Kill();
        _limitTween = CreateTween().SetParallel();

        Rect2 cameraRect = GetRect();

        if (top > cameraRect.Position.Y)
            LimitTop = (int)cameraRect.Position.Y;

        if (right < cameraRect.End.X)
            LimitRight = (int)cameraRect.End.X;

        if (bottom < cameraRect.End.Y)
            LimitBottom = (int)cameraRect.End.Y;

        if (left > cameraRect.Position.X)
            LimitLeft = (int)cameraRect.Position.X;

        AddTweenProperty(_limitTween, Camera2D.PropertyName.LimitTop, top, cameraRect.Position.Y);
        AddTweenProperty(_limitTween, Camera2D.PropertyName.LimitRight, right, cameraRect.End.X);
        AddTweenProperty(_limitTween, Camera2D.PropertyName.LimitBottom, bottom, cameraRect.End.Y);
        AddTweenProperty(_limitTween, Camera2D.PropertyName.LimitLeft, left, cameraRect.Position.X);

        void AddTweenProperty(Tween tween, string propertyName, int target, float current)
        {
            float time = Mathf.Abs(target - current) / _limitUpdateSpeed;
            tween.TweenProperty(this, propertyName, target, time);
        }
    }

    private void SetCurrentTarget(Node2D? node2d)
    {
        _currentTarget = node2d;
        _switchSpeed = 0;
        _switching = false;

        if (_currentTarget == null)
            return;

        float distance = GlobalPosition.DistanceTo(_currentTarget.GlobalPosition.Round());

        if (distance < 1)
            return;

        _switching = true;
        _switchSpeed = distance / 60;

    }
}
