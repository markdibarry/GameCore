using System;
using Godot;

namespace GameCore.Statistics;

public abstract partial class BaseHurtBox : AreaBox
{
    public event Action<BaseDamageRequest>? DamageRequested;

    public override void _Ready()
    {
        base._Ready();
        AreaEntered += OnAreaEntered;
    }

    public void OnAreaEntered(Area2D area2D)
    {
        if (area2D is not BaseHitBox hitBox)
            return;
        DamageRequested?.Invoke(hitBox.GetDamageRequest());
    }

    public abstract void SetHurtboxRole(int role);
}
