using System;
using System.Threading.Tasks;
using GCol = Godot.Collections;
using RL = Godot.ResourceLoader;

namespace GameCore.GUI;

public class ResourceLoader : ObjectLoader
{
    public ResourceLoader(string path, Action reportProgress)
        : base(path, reportProgress) { }

    public override async Task<object?> LoadAsync()
    {
        RL.ThreadLoadStatus loadStatus;
        GCol.Array loadProgress = new();
        RL.LoadThreadedRequest(Path);

        loadStatus = RL.LoadThreadedGetStatus(Path, loadProgress);
        Progress = (int)((double)loadProgress[0] * 100);
        ReportProgress();

        while (loadStatus == RL.ThreadLoadStatus.InProgress)
        {
            await Task.Delay(100);
            loadStatus = RL.LoadThreadedGetStatus(Path, loadProgress);
            Progress = (int)((double)loadProgress[0] * 100);
            ReportProgress();
        }
        if (loadStatus == RL.ThreadLoadStatus.Loaded)
            LoadedObject = RL.LoadThreadedGet(Path);
        return LoadedObject;
    }
}
