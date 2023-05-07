using System;
using System.Threading.Tasks;

namespace GameCore.GUI;

public class ResourceLoader : ObjectLoader
{
    public ResourceLoader(string path, Action reportProgress)
        : base(path, reportProgress) { }

    public override async Task<object?> LoadAsync()
    {
        Godot.ResourceLoader.ThreadLoadStatus loadStatus;
        Godot.Collections.Array loadProgress = new();
        Godot.ResourceLoader.LoadThreadedRequest(Path);

        loadStatus = Godot.ResourceLoader.LoadThreadedGetStatus(Path, loadProgress);
        Progress = (int)((double)loadProgress[0] * 100);
        ReportProgress();

        while (loadStatus == Godot.ResourceLoader.ThreadLoadStatus.InProgress)
        {
            await Task.Delay(100);
            loadStatus = Godot.ResourceLoader.LoadThreadedGetStatus(Path, loadProgress);
            Progress = (int)((double)loadProgress[0] * 100);
            ReportProgress();
        }
        if (loadStatus == Godot.ResourceLoader.ThreadLoadStatus.Loaded)
            LoadedObject = Godot.ResourceLoader.LoadThreadedGet(Path);
        return LoadedObject;
    }
}
