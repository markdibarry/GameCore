using System;

namespace GameCore.Actors;

public static class ActorsLocator
{
    private static IActorBodyPathDB? s_actorBodyDB;
    private static readonly NullActorBodyPathDB s_nullActorBodyPathDB = new();
    private static IActorDataDB? s_actorDataDB;
    private static readonly NullActorDataDB s_nullActorDataDB = new();
    public static IActorBodyPathDB ActorBodyDB => s_actorBodyDB ?? s_nullActorBodyPathDB;
    public static IActorDataDB ActorDataDB => s_actorDataDB ?? s_nullActorDataDB;

    public static void ProvideActorBodyDB(IActorBodyPathDB actorBodyDB) => s_actorBodyDB = actorBodyDB;

    public static void ProvideActorDataDB(IActorDataDB actorDataDB) => s_actorDataDB = actorDataDB;

    private class NullActorBodyPathDB : IActorBodyPathDB
    {
        public string? GetById(string bodyId) => null;
    }

    private class NullActorDataDB : IActorDataDB
    {
        public bool TryGetData<T>(string key, out T? value) where T : BaseActorData
        {
            value = null;
            return false;
        }

        public T? GetData<T>(string id) where T : BaseActorData => null;

        public string[] GetKeys() => Array.Empty<string>();
    }
}
