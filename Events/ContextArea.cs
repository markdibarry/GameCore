using GameCore.Actors;
using Godot;

namespace GameCore.Events;

public abstract partial class ContextArea : Area2D, IContextArea
{
    [Export] public bool Automatic { get; set; }
    [Export] public bool IsActive { get; set; } = true;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    public void OnBodyEntered(Node body)
    {
        if (body is not BaseActorBody actor)
            return;

        if (Automatic)
        {
            IsActive = true;
            TriggerContext(actor);
        }
        else
        {
            actor.ContextAreas.Add(this);
        }
    }

    public void OnBodyExited(Node body)
    {
        if (body is not BaseActorBody actor)
            return;

        if (!Automatic)
            actor.ContextAreas.Remove(this);
    }

    public abstract void TriggerContext(BaseActorBody actor);
}
