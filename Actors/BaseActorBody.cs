using System;
using System.Collections.Generic;
using GameCore.Audio;
using GameCore.Events;
using GameCore.Input;
using GameCore.Statistics;
using GameCore.Utility;
using Godot;

namespace GameCore.Actors;

/// <summary>
/// Base character object.
/// </summary>
public abstract partial class BaseActorBody : CharacterBody2D
{
    protected BaseActorBody()
    {
        Acceleration = 600;
        AnimationPlayer = null!;
        Body = null!;
        BodySprite = null!;
        CollisionShape2D = null!;
        ContextAreas = new();
        Direction = new(1, 1);
        Friction = 600;
        GroundedGravity = 0.05;
        HurtBoxes = null!;
        HitBoxes = null!;
        StateController = null!;
        UpDirection = Vector2.Up;
        WalkSpeed = 50;
        MaxSpeed = WalkSpeed;
    }

    protected static BaseAudioController Audio { get; } = Locator.Audio;
    private BaseActor? _actorInternal;
    public virtual BaseActor? Actor => _actorInternal;
    public int Role { get; protected set; }
    public AnimationPlayer AnimationPlayer { get; private set; }
    public Sprite2D BodySprite { get; private set; }
    public CollisionShape2D CollisionShape2D { get; private set; }
    public HashSet<IContextArea> ContextAreas { get; set; }
    public AreaBoxContainer HurtBoxes { get; private set; }
    public AreaBoxContainer HitBoxes { get; private set; }
    public bool InActionSequence { get; private set; }
    public IStateController StateController { get; protected set; }
    protected Node2D Body { get; set; } = null!;
    public event Action<BaseActorBody>? Freeing;

    public override void _Ready()
    {
        SetNodeReferences();
        Init();
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = _floatPosition;
        _move = Vector2.Zero;
        Actor?.Stats.Process(delta, !InActionSequence);
        foreach (IContextArea context in ContextAreas)
            context.TriggerContext(this);
        if (!InActionSequence)
        {
            StateController.UpdateStates(delta);
            HandleMove(delta);
        }
        _floatPosition = GlobalPosition;
        GlobalPosition = GlobalPosition.Round();
        InputHandler.Update();
    }

    public override void _ExitTree()
    {
        if (!IsQueuedForDeletion())
            return;

        CleanUpActorBody();
    }

    public void CleanUpActorBody()
    {
        Freeing?.Invoke(this);
        Actor?.SetActorBody(null);
    }

    public void OnGameStateChanged(GameState gameState)
    {
        if (gameState.CutsceneActive)
        {
            HurtBoxes.SetMonitoringDeferred(false);
            InputHandler.UserInputDisabled = true;
        }
        else
        {
            HurtBoxes.SetMonitoringDeferred(true);
            InputHandler.UserInputDisabled = false;
        }
    }

    public void PlaySoundFX(string soundPath)
    {
        Audio.PlaySoundFX(this, soundPath);
    }

    public void PlaySoundFX(AudioStream sound)
    {
        Audio.PlaySoundFX(this, sound);
    }

    public virtual void SetActor(BaseActor? actor) => _actorInternal = actor;

    public void SetForActionSequence(bool enable)
    {
        InActionSequence = enable;
        CollisionShape2D.SetDeferred(CollisionShape2D.PropertyName.Disabled, enable);
        HurtBoxes.SetMonitoringDeferred(!enable);
        HitBoxes.SetMonitoringDeferred(!enable);
    }

    public abstract void SetRole(int role, bool setActorRole = true);

    public virtual void SetNodeReferences()
    {
        Body = GetNode<Node2D>("Body");
        BodySprite = Body.GetNode<Sprite2D>("BodySprite");
        CollisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
        HurtBoxes = BodySprite.GetNode<AreaBoxContainer>("HurtBoxes");
        HitBoxes = BodySprite.GetNode<AreaBoxContainer>("HitBoxes");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    protected virtual void SetHitBoxes() { }

    protected virtual void Init()
    {
        _floatPosition = GlobalPosition;
        SetHitBoxes();
        SetRole(Role);
        InitState();
        InitActor();
    }

    protected void InitActor()
    {
        if (Actor == null)
            return;
        foreach (BaseHurtBox hurtbox in HurtBoxes.GetChildren<BaseHurtBox>())
            hurtbox.DamageRequested += Actor.Stats.OnDamageReceived;
    }

    private void InitState()
    {
        StateController.Init();
        OnGameStateChanged(Locator.Root.GameState);
    }
}
