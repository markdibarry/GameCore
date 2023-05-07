namespace GameCore.GUI;

public interface IStorageContext
{
    void Clear();
    bool Contains(string key);
    void SetValue(string key, string value);
    void SetValue(string key, float value);
    void SetValue(string key, bool value);
    bool TryGetValue(string key, out object? result);
    bool TryGetValue<T>(string key, out T? result) where T : notnull;
}
