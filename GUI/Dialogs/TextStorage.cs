using System.Collections.Generic;

namespace GameCore.GUI;

public class TextStorage : IStorageContext
{
    private readonly Dictionary<string, object> _tempStorage = new();

    public void Clear() => _tempStorage.Clear();

    public bool Contains(string key) => _tempStorage.ContainsKey(key);

    public void SetValue(string key, string value) => SetValue<string>(key, value);

    public void SetValue(string key, float value) => SetValue<float>(key, value);

    public void SetValue(string key, bool value) => SetValue<bool>(key, value);

    public bool TryGetValue(string key, out object? value)
    {
        return _tempStorage.TryGetValue(key, out value);
    }

    public bool TryGetValue<T>(string key, out T? result) where T : notnull
    {
        if (!_tempStorage.TryGetValue(key, out object? val) || val is not T)
        {
            result = default;
            return false;
        }

        result = (T)val;
        return true;
    }

    private void SetValue<T>(string key, T value) where T : notnull
    {
        if (!_tempStorage.TryGetValue(key, out object? val) || val is T)
            _tempStorage[key] = value;
    }
}
