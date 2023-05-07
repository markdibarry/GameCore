using System.Collections.Generic;

namespace GameCore.Actors;

public abstract class AActorDataDB
{
    protected AActorDataDB()
    {
        ActorData = BuildDB();
    }

    public IReadOnlyDictionary<string, AActorData> ActorData { get; }

    public bool TryGetData<T>(string key, out T? value) where T : AActorData
    {
        if (ActorData.TryGetValue(key, out AActorData? actorData) && actorData is T t)
        {
            value = t;
            return true;
        }
        value = default;
        return false;
    }

    public T? GetData<T>(string id) where T : AActorData
    {
        if (ActorData.TryGetValue(id, out AActorData? actorData) && actorData is T t)
            return t;
        return null;
    }

    protected abstract Dictionary<string, AActorData> BuildDB();
}
