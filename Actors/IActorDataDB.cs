namespace GameCore.Actors;

public interface IActorDataDB
{
    bool TryGetData<T>(string key, out T? value) where T : BaseActorData;
    T? GetData<T>(string id) where T : BaseActorData;
    string[] GetKeys();
}
