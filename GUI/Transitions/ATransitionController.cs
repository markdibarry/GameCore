using System;
using System.Threading.Tasks;
using GameCore.Utility;

namespace GameCore.GUI;

public abstract class ATransitionController
{
    private TransitionRequest? _pendingTransition;

    public virtual void Update()
    {
        if (_pendingTransition != null)
            _ = TransitionAsync(_pendingTransition);
    }

    public void RequestTransition(TransitionRequest request)
    {
        _pendingTransition = request;
    }

    public async Task TransitionAsync(TransitionRequest request)
    {
        _pendingTransition = null;
        Loader loader = new(request.Paths);
        TransitionLayer transitionLayer = GetTransitionLayer(request.TransitionType);
        LoadingScreen? loadingScreen = null;
        Transition? transitionA = null;
        Transition? transitionB = null;
        if (request.LoadingScreenPath != string.Empty)
            loadingScreen = GDEx.Instantiate<LoadingScreen>(request.LoadingScreenPath);
        if (request.TransitionAPath != string.Empty)
            transitionA = GDEx.Instantiate<Transition>(request.TransitionAPath);
        if (request.TransitionBPath != string.Empty)
            transitionB = GDEx.Instantiate<Transition>(request.TransitionBPath);

        await TransitionInAsync(transitionLayer, loadingScreen, transitionA);
        await LoadAsync(loadingScreen, loader);
        if (request.Callback != null)
            await request.Callback.Invoke(loader);
        await TransitionOutAsync(transitionLayer, loadingScreen, transitionA, transitionB);

        loadingScreen?.QueueFree();
        transitionA?.QueueFree();
        transitionB?.QueueFree();
    }

    private static async Task LoadAsync(LoadingScreen? loadingScreen, Loader loader)
    {
        await loader.LoadAsync((int progress) => loadingScreen?.Update(progress));
    }

    private static async Task TransitionInAsync(TransitionLayer transitionLayer, LoadingScreen? loadingScreen, Transition? transitionA)
    {
        if (loadingScreen == null)
        {
            if (transitionA == null)
                return;
            transitionLayer.AddChild(transitionA);
            await transitionA.TransistionFrom();
            return;
        }

        if (transitionA == null)
        {
            transitionLayer.LoadingScreenContainer.AddChild(loadingScreen);
            await loadingScreen.TransistionFrom();
            return;
        }

        transitionLayer.AddChild(transitionA);
        await transitionA.TransistionFrom();
        transitionLayer.LoadingScreenContainer.AddChild(loadingScreen);
        await transitionA.TransitionTo();
        transitionLayer.RemoveChild(transitionA);
    }

    private static async Task TransitionOutAsync(TransitionLayer transitionLayer, LoadingScreen? loadingScreen, Transition? transitionA, Transition? transitionB)
    {
        if (loadingScreen == null)
        {
            if (transitionA == null)
                return;
            await transitionA.TransitionTo();
            transitionLayer.RemoveChild(transitionA);
            return;
        }

        if (transitionA == null)
        {
            await loadingScreen.TransitionTo();
            transitionLayer.LoadingScreenContainer.RemoveChild(loadingScreen);
            return;
        }

        Transition transitionOut = transitionB ?? transitionA;

        transitionLayer.AddChild(transitionOut);
        await transitionOut.TransistionFrom();
        transitionLayer.LoadingScreenContainer.RemoveChild(loadingScreen);
        await transitionOut.TransitionTo();
        transitionLayer.RemoveChild(transitionOut);
    }

    private static TransitionLayer GetTransitionLayer(TransitionType transitionType)
    {
        return transitionType switch
        {
            TransitionType.Game => Locator.Root.Transition,
            TransitionType.Session => Locator.Session?.Transition!,
            _ => throw new NotImplementedException()
        };
    }
}
