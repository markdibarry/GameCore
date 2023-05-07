using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCore.Actors;
using GameCore.Audio;
using GameCore.GUI;
using GameCore.Input;
using GameCore.Items;
using GameCore.Statistics;
using GameCore.Utility;
using Godot;

namespace GameCore;

public abstract partial class AGameRoot : Node
{
    protected AGameRoot(string gameSessionScenePath, string titleMenuScenePath)
    {
        GameSessionScenePath = gameSessionScenePath;
        TitleMenuScenePath = titleMenuScenePath;
    }

    public string GameSessionScenePath { get; }
    public string TitleMenuScenePath { get; }
    public AAudioController AudioController { get; protected set; } = null!;
    public GameCamera GameCamera { get; protected set; } = null!;
    public Node2D GameDisplay { get; set; } = null!;
    public Node2D GameSessionContainer { get; set; } = null!;
    public GUIController GUIController { get; protected set; } = null!;
    public TransitionLayer Transition { get; protected set; } = null!;
    public AGameSession? GameSession { get; private set; }
    public GameState GameState { get; } = new();
    public abstract GUIInputHandler MenuInput { get; }
    public abstract InputHandler PlayerOneInput { get; }
    public abstract ATransitionController TransitionController { get; }

    public override void _Ready()
    {
        SetNodeReferences();
        Locator.ProvideGameRoot(this);
        List<string> unsetRefs = Locator.CheckReferences();
        unsetRefs.AddRange(ActorsLocator.CheckReferences());
        unsetRefs.AddRange(StatsLocator.CheckReferences());
        unsetRefs.AddRange(ItemsLocator.CheckReferences());
        if (unsetRefs.Count > 0)
        {
            string errMessage = "The following static Locator references have not been set: " + string.Join(", ", unsetRefs)
                + ". Please create an autoload and load them in the _Ready method.";
            throw new System.Exception(errMessage);
        }
        Init();
    }

    protected virtual void SetNodeReferences()
    {
        GameDisplay = GetNode<Node2D>("GameDisplay");
        GUIController = GameDisplay.GetNode<GUIController>("GUIController");
        AudioController = GameDisplay.GetNode<AAudioController>("AudioController");
        GameSessionContainer = GameDisplay.GetNode<Node2D>("GameSessionContainer");
        Transition = GameDisplay.GetNode<TransitionLayer>("Transition");
        GameCamera = GameDisplay.GetNode<GameCamera>("GameCamera");
    }

    protected virtual void Init()
    {
        GameState.Init(GUIController);
        GameState.GameStateChanged += OnGameStateChanged;
        StartRoot();
    }

    protected virtual void StartRoot() => ResetToTitleScreen();

    public override void _Process(double delta)
    {
        HandleInput(delta);
        TransitionController.Update();
    }

    public virtual async Task RemoveSession()
    {
        if (GameSession == null)
            return;

        GameSessionContainer.RemoveChild(GameSession);
        GameSession.QueueFree();
        GameSession = null;
        await GUIController.CloseAllLayersAsync(true);
    }

    public virtual async Task StartNewSession(IGameSave gameSave)
    {
        GameSession = GDEx.Instantiate<AGameSession>(GameSessionScenePath);
        GameSessionContainer.AddChild(GameSession);
        GameSession.Init(GUIController, gameSave);
        await GUIController.CloseAllLayersAsync(true);
    }

    public virtual void ResetToTitleScreen() => ResetToTitleScreen(string.Empty, string.Empty, string.Empty);

    public virtual void ResetToTitleScreen(string loadingScreenPath, string transitionA, string transitionB)
    {
        TransitionRequest request = new(
            loadingScreenPath: loadingScreenPath,
            transitionType: TransitionType.Game,
            transitionA: transitionA,
            transitionB: transitionB,
            paths: new string[] { TitleMenuScenePath },
            callback: async (loader) =>
            {
                PackedScene? titleMenuScene = loader.GetObject<PackedScene>(TitleMenuScenePath);
                AudioController.Reset();
                await RemoveSession();
                await GUIController.OpenMenuAsync(titleMenuScene, true);
            });
        TransitionController.RequestTransition(request);
    }

    protected void HandleInput(double delta)
    {
        if (Godot.Input.IsActionJustPressed("collect"))
        {
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        GUIController.HandleInput(MenuInput, delta);
        GameSession?.HandleInput(MenuInput, delta);
        MenuInput.Update();
    }

    protected void OnGameStateChanged(GameState gameState)
    {
        AudioController.OnGameStateChanged(gameState);
        GameSession?.OnGameStateChanged(gameState);
    }
}
