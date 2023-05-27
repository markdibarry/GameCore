using System;

namespace GameCore.GUI;

public interface ILoaderFactory
{
    ObjectLoader GetLoader(string path, Action reportCallback)
    {
        return new ResourceLoader(path, reportCallback);
    }
}
