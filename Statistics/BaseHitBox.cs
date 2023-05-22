using System;

namespace GameCore.Statistics;

public abstract partial class BaseHitBox : AreaBox
{
    public Func<IDamageRequest> GetDamageRequest { get; set; } = null!;

    public abstract void SetHitboxRole(int role);
}
