using System.Diagnostics.CodeAnalysis;

namespace GameCore.Statistics;

public interface IStatusEffectDB
{
    bool TryGetEffectData(string typeId, [MaybeNullWhen(false)] out StatusEffectData data);
}
