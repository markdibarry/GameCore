using System.Collections.Generic;

namespace GameCore.Actors;

public abstract class AActorBodyDB
{
    public abstract IReadOnlyDictionary<string, string> ActorBodies { get; }

    public string? ById(string bodyId)
    {
        if (ActorBodies.TryGetValue(bodyId, out string? result))
            return result;
        return default;
    }
}
