namespace GameCore.Actors;

public interface IActorDataDB
{
    bool TryGetData<T>(string key, out T? value) where T : AActorData;
    T? GetData<T>(string id) where T : AActorData;
    string[] GetKeys();
}
