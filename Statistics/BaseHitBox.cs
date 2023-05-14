using System;

namespace GameCore.Statistics;

public abstract partial class BaseHitBox : AreaBox
{
    public Func<BaseDamageRequest> GetDamageRequest { get; set; } = null!;

    public abstract void SetHitboxRole(int role);
}
