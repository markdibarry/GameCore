using System.Collections.Generic;

namespace GameCore.Actors;

public static class ActorsLocator
{
    private static AActorBodyDB s_actorBodyDB = null!;
    private static AActorDataDB s_actorDataDB = null!;
    public static AActorBodyDB ActorBodyDB => s_actorBodyDB;
    public static AActorDataDB ActorDataDB => s_actorDataDB;

    public static void ProvideActorBodyDB(AActorBodyDB actorBodyDB) => s_actorBodyDB = actorBodyDB;

    public static void ProvideActorDataDB(AActorDataDB actorDataDB) => s_actorDataDB = actorDataDB;

    public static List<string> CheckReferences()
    {
        List<string> unsetRefs = new();
        if (s_actorBodyDB == null)
            unsetRefs.Add("ActorBody DB");
        if (s_actorDataDB == null)
            unsetRefs.Add("ActorData DB");
        return unsetRefs;
    }
}
