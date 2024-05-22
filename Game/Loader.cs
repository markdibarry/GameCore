using System;
using System.Linq;
using System.Threading.Tasks;
using RL = Godot.ResourceLoader;

namespace GameCore.Game;

public class Loader
{
    private string[] _paths = [];
    private readonly Godot.Collections.Array _loadProgress = [];
    public State CurrentState { get; private set; } = State.Inactive;
    private TaskCompletionSource<bool> _tcs = new();

    public enum State
    {
        Inactive,
        Loading
    }

    public static void Load(string[] paths)
    {
        foreach (string path in paths)
            RL.Load(path);
    }

    public async Task LoadAsync(string[] paths)
    {
        _tcs = new();
        CurrentState = State.Loading;
        _paths = paths.Where(x => !RL.HasCached(x)).ToArray();

        foreach (string path in _paths)
            RL.LoadThreadedRequest(path, useSubThreads: true);

        await _tcs.Task;
    }

    public double Update()
    {
        if (CurrentState != State.Loading)
            return 0;

        if (_paths.Length == 0)
        {
            CurrentState = State.Inactive;
            _tcs.SetResult(true);
            return 100;
        }

        string[] paths = _paths;
        bool loaded = true;
        double progressValue = 0;

        foreach (string path in paths)
        {
            RL.ThreadLoadStatus status = RL.LoadThreadedGetStatus(path, _loadProgress);
            progressValue += (double)_loadProgress[0] * 100;

            if (status == RL.ThreadLoadStatus.InProgress)
            {
                loaded = false;
            }
            else if (status != RL.ThreadLoadStatus.Loaded)
            {
                throw new Exception($"{path} could not be loaded.");
            }
        }

        if (loaded)
        {
            CurrentState = State.Inactive;
            _tcs.SetResult(true);
            _paths = [];
        }

        return double.Round(progressValue / paths.Length);
    }
}
