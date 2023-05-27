using System.Threading.Tasks;
using GameCore.Audio;
using GameCore.GUI;
using GameCore.Input;
using GameCore.Utility;
using Godot;

namespace GameCore;

public abstract partial class BaseGameRoot : Node
{
    protected BaseGameRoot(
        string gameSessionScenePath,
        string titleMenuScenePath,
        BaseTransitionController? transitionController = null)
    {
        GameSessionScenePath = gameSessionScenePath;
        TitleMenuScenePath = titleMenuScenePath;
        TransitionController = transitionController ?? new BaseTransitionController();
    }

    public string GameSessionScenePath { get; }
    public string TitleMenuScenePath { get; }
    public IAudioService AudioController { get; protected set; } = null!;
    public GameCamera GameCamera { get; protected set; } = null!;
    public Node2D GameDisplay { get; set; } = null!;
    public Node2D GameSessionContainer { get; set; } = null!;
    public GUIController GUIController { get; protected set; } = null!;
    public TransitionLayer Transition { get; protected set; } = null!;
    public BaseGameSession? GameSession { get; private set; }
    public GameState GameState { get; } = new();
    public abstract IGUIInputHandler MenuInput { get; }
    public abstract IInputHandler PlayerOneInput { get; }
    public BaseTransitionController TransitionController { get; }

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
        GameSession = GDEx.Instantiate<BaseGameSession>(GameSessionScenePath);
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
