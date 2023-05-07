using System.Collections.Generic;

namespace GameCore.ActionEffects;

public abstract class AActionEffectDB
{
    protected AActionEffectDB()
    {
        Effects = BuildDB();
    }

    protected IReadOnlyDictionary<int, IActionEffect> Effects { get; }

    public IActionEffect? GetEffect(int type)
    {
        if (Effects.TryGetValue(type, out IActionEffect? effect))
            return effect;
        return null;
    }

    protected virtual IReadOnlyDictionary<int, IActionEffect> BuildDB() => new Dictionary<int, IActionEffect>();
}
