using System.Collections.Generic;
using System.Linq;

namespace GameCore.Statistics;

public abstract class AStatusEffectDB
{
    protected AStatusEffectDB()
    {
        Effects = BuildDB();
    }

    public IReadOnlyCollection<StatusEffectData> Effects { get; }

    public StatusEffectData? GetEffectData(int type)
    {
        return Effects.FirstOrDefault(effect => effect.EffectType.Equals(type));
    }

    protected abstract StatusEffectData[] BuildDB();
}
