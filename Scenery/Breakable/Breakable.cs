using System;
using Godot;

namespace GameCore.Scenery;

public partial class Breakable : StaticBody2D
{
    public Breakable()
    {
        _hitsToNextStage = 1;
    }

    private CollisionShape2D _collision = null!;
    private AnimatedSprite2D _animatedSprite = null!;
    private Area2D _hurtBox = null!;
    private bool _broken;
    private int _hits;
    private int _currentFrame;
    private int _frameCount;
    private event Action? BreakableDestroyed;

    /// <summary>
    /// The number of hits between texture changes
    /// </summary>
    /// <value></value>
    [Export]
    private int _hitsToNextStage;

    public override void _Ready()
    {
        _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite");
        _frameCount = _animatedSprite.SpriteFrames.GetFrameCount("default");
        _collision = GetNode<CollisionShape2D>("Collision");
        _hurtBox = GetNode<Area2D>("HurtBox");
        _hurtBox.AreaEntered += OnAreaEntered;
    }

    public void OnAreaEntered(Area2D area2D)
    {
        TryBreak();
    }

    private void TryBreak()
    {
        if (_frameCount <= 0 || _broken)
            return;
        _hits++;
        if (_hits < _hitsToNextStage)
            return;
        _currentFrame++;

        if (_currentFrame < _frameCount)
            _animatedSprite.Frame = _currentFrame;
        else
            Destroy();

        _hits = 0;
    }

    private void Destroy()
    {
        _broken = true;
        BreakableDestroyed?.Invoke();
        _hurtBox.SetDeferred(Area2D.PropertyName.Monitoring, false);
        _collision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        _animatedSprite.AnimationFinished += OnAnimationFinished;
        _animatedSprite.Play("destroy");
    }

    private void OnAnimationFinished() => QueueFree();
}
