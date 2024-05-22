using System;
using System.Threading.Tasks;
using GameCore.Game;
using GameCore.Utility;

namespace GameCore.GUI;

public class TransitionController
{
    public bool IsTransitioning { get; private set; }
    private readonly Loader _loader = new();
    private LoadingScreen? _loadingScreen;
    private Transition? _transitionIn;
    private TransitionLayer _transitionLayer = default!;
    private string _transitionInPath = string.Empty;

    public async Task TransitionInOutAsync(
        TransitionType transitionType,
        string transitionInOutPath,
        string[] paths,
        Func<Task> callback)
    {
        await TransitionInOutAsync(transitionType, transitionInOutPath, string.Empty, string.Empty, paths, callback);
    }

    public async Task TransitionInOutAsync(
        TransitionType transitionType,
        string transitionInOutPath,
        string loadingScreenPath,
        string[] paths,
        Func<Task> callback)
    {
        await TransitionInOutAsync(transitionType, transitionInOutPath, string.Empty, loadingScreenPath, paths, callback);
    }

    public async Task TransitionInOutAsync(
        TransitionType transitionType,
        string transitionInPath,
        string transitionOutPath,
        string loadingScreenPath,
        string[] paths,
        Func<Task> callback)
    {
        if (IsTransitioning)
            return;

        await TransitionInAsync(transitionType, transitionInPath, loadingScreenPath);
        await LoadAsync(paths);
        await callback.Invoke();
        await TransitionOutAsync(transitionOutPath);
    }

    public async Task LoadAsync(string[] paths)
    {
        if (!IsTransitioning)
            return;

        if (_loadingScreen == null)
            Loader.Load(paths);
        else
            await _loadingScreen.LoadAsync(_loader, paths);
    }

    public async Task TransitionInAsync(
        TransitionType transitionType,
        string transitionInPath,
        string loadingScreenPath)
    {
        if (IsTransitioning)
            return;

        IsTransitioning = true;
        _transitionLayer = GetTransitionLayer(transitionType);
        _transitionInPath = transitionInPath;

        if (transitionInPath != string.Empty)
            _transitionIn = GDEx.Instantiate<Transition>(transitionInPath);
        if (loadingScreenPath != string.Empty)
            _loadingScreen = GDEx.Instantiate<LoadingScreen>(loadingScreenPath);

        if (_loadingScreen != null && _transitionIn != null)
        {
            _transitionLayer.AddChild(_transitionIn);
            await _transitionIn.TransistionFrom();
            _transitionLayer.LoadingScreenContainer.AddChild(_loadingScreen);
            await _transitionIn.TransitionTo();
            _transitionLayer.RemoveChild(_transitionIn);
        }
        else if (_loadingScreen == null) // Use transition in for both in and out
        {
            _transitionLayer.AddChild(_transitionIn);
            await _transitionIn!.TransistionFrom();
        }
        else if (_transitionIn == null) // Just use the loading screen's transition
        {
            _transitionLayer.LoadingScreenContainer.AddChild(_loadingScreen);
            await _loadingScreen.TransistionFrom();
        }
    }

    public async Task TransitionOutAsync(string transitionOutPath = "")
    {
        if (!IsTransitioning)
            return;

        Transition? transitionOut;

        if (transitionOutPath == string.Empty || transitionOutPath == _transitionInPath)
            transitionOut = _transitionIn;
        else
            transitionOut = GDEx.Instantiate<Transition>(transitionOutPath);

        if (_loadingScreen != null && transitionOut != null)
        {
            _transitionLayer.AddChild(transitionOut);
            await transitionOut.TransistionFrom();
            _transitionLayer.LoadingScreenContainer.RemoveChild(_loadingScreen);
            await transitionOut.TransitionTo();
            _transitionLayer.RemoveChild(transitionOut);
        }
        else if (_loadingScreen == null) // Reuse transition in to transition out
        {
            await transitionOut!.TransitionTo();
            _transitionLayer.RemoveChild(_transitionIn);
        }
        else if (transitionOut == null)  // Just use the loading screen's transition
        {
            await _loadingScreen.TransitionTo();
            _transitionLayer.LoadingScreenContainer.RemoveChild(_loadingScreen);
        }

        if (_transitionIn != transitionOut)
            transitionOut?.QueueFree();

        _transitionIn?.QueueFree();
        _loadingScreen?.QueueFree();
        _transitionIn = null;
        _transitionInPath = string.Empty;
        _loadingScreen = null;
        IsTransitioning = false;
    }

    private static TransitionLayer GetTransitionLayer(TransitionType transitionType)
    {
        return transitionType switch
        {
            TransitionType.Game => Locator.Root.GameTransition,
            TransitionType.Session => Locator.Session?.SessionTransition!,
            _ => throw new NotImplementedException()
        };
    }
}

public enum TransitionType
{
    Game,
    Session
}
