using Godot;

namespace GameCore.Actors;

public abstract partial class SubActor : CharacterBody2D
{
    protected SubActor()
    {
    }

    public BaseActorBody? ParentActor { get; set; }

    public override void _Ready()
    {
    }
}
