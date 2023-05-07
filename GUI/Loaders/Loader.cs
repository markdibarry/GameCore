using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameCore.GUI;

public class Loader
{
    public Loader(string path)
        : this(new string[] { path })
    { }

    public Loader(string[] paths)
    {
        _objectLoaders = new();
        foreach (string path in paths)
            _objectLoaders.Add(Locator.LoaderFactory.GetLoader(path, OnReport));
    }

    private readonly List<ObjectLoader> _objectLoaders;
    private Action<int>? _progressUpdateCallback;

    public T? GetObject<T>(string path)
    {
        object? result = _objectLoaders.FirstOrDefault(x => x.Path == path)?.LoadedObject;
        return result is T t ? t : default;
    }

    public async Task LoadAsync(Action<int> callback)
    {
        _progressUpdateCallback = callback;
        foreach (ObjectLoader objectLoader in _objectLoaders)
            await objectLoader.LoadAsync();
        _progressUpdateCallback = null;
    }

    public void OnReport()
    {
        _progressUpdateCallback?.Invoke((int)_objectLoaders.Average(x => x.Progress));
    }
}
