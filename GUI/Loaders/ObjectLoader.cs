using System;
using System.Threading.Tasks;

namespace GameCore.GUI;

public abstract class ObjectLoader
{
    protected ObjectLoader(string path, Action reportProgress)
    {
        Path = path;
        ReportProgress = reportProgress;
    }

    public object? LoadedObject { get; set; }
    public int Progress { get; set; }
    public string Path { get; set; }
    public Action ReportProgress { get; set; }
    public abstract Task<object?> LoadAsync();
}
