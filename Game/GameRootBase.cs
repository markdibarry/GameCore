using System;
using System.Threading.Tasks;
using GameCore.Audio;
using GameCore.GUI;
using GameCore.Input;
using GameCore.Utility;
using Godot;

namespace GameCore;

public abstract partial class GameRootBase(string gameSessionScenePath, string titleMenuScenePath) : Node
{
    public string GameSessionScenePath { get; } = gameSessionScenePath;
    public string TitleMenuScenePath { get; } = titleMenuScenePath;
    public IAudioService AudioController { get; protected set; } = null!;
    public GameCamera GameCamera { get; protected set; } = null!;
    public Node2D GameDisplay { get; set; } = null!;
    public Node2D GameSessionContainer { get; set; } = null!;
    public GUIController GUIController { get; protected set; } = null!;
    public TransitionLayer GameTransition { get; protected set; } = null!;
    public GameSessionBase? GameSession { get; private set; }
    public GameState GameState { get; } = new();
    public abstract IGUIInputHandler MenuInput { get; }
    public abstract IInputHandler PlayerOneInput { get; }
    public TransitionController TransitionController { get; } = new();

    public override void _Ready()
    {
        SetNodeReferences();
        Locator.ProvideGameRoot(this);
        Init();
    }

    protected virtual void SetNodeReferences()
    {
        GameDisplay = GetNode<Node2D>("GameDisplay");
        GUIController = GameDisplay.GetNode<GUIController>("GUIController");
        AudioController = GameDisplay.GetNode<IAudioService>("AudioController");
        GameSessionContainer = GameDisplay.GetNode<Node2D>("GameSessionContainer");
        GameTransition = GameDisplay.GetNode<TransitionLayer>("GameTransition");
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
        GameSession = GDEx.Instantiate<GameSessionBase>(GameSessionScenePath);
        GameSessionContainer.AddChild(GameSession);
        GameSession.Init(GUIController, gameSave);
        await GUIController.CloseAllLayersAsync(true);
    }

    public virtual void ResetToTitleScreen() => ResetToTitleScreen(string.Empty, string.Empty, string.Empty);

    public virtual void ResetToTitleScreen(string loadingScreenPath, string transitionA, string transitionB)
    {
        _ = TransitionController.TransitionInOutAsync(
            transitionType: TransitionType.Game,
            transitionInPath: transitionA,
            transitionOutPath: transitionB,
            loadingScreenPath: loadingScreenPath,
            paths: [TitleMenuScenePath],
            callback: async () =>
            {
                PackedScene? titleMenuScene = (PackedScene)ResourceLoader.Load(TitleMenuScenePath)!;
                AudioController.Reset();
                await RemoveSession();
                await GUIController.OpenMenuAsync(titleMenuScene, true);
            });
    }

    protected void HandleInput(double delta)
    {
        if (Godot.Input.IsActionJustPressed("collect"))
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
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
