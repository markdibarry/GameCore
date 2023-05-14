using System.Collections.Generic;
using GameCore.Actors;
using GameCore.GUI;
using GameCore.Utility;
using Godot;

namespace GameCore.AreaScenes;

public partial class BaseAreaScene : Node2D
{
    public CanvasLayer FocusLayer { get; private set; } = null!;
    public CanvasLayer ActorsContainer { get; private set; } = null!;
    public ColorAdjustment ColorAdjustment { get; private set; } = null!;
    public Node2D EventContainer { get; private set; } = null!;
    public AHUD HUD { get; private set; } = null!;
    public Node2D SpawnPointContainer { get; private set; } = null!;

    public override void _Ready()
    {
        SetNodeReferences();
    }

    public override void _ExitTree()
    {
        foreach (var actorBody in ActorsContainer.GetChildren<BaseActorBody>())
            actorBody.CleanUpActorBody();
    }

    public void AddActorBody(BaseActorBody actorBody)
    {
        HUD.SubscribeActorBodyEvents(actorBody);
        ActorsContainer.AddChild(actorBody);
    }

    public void MoveToActorContainer(BaseActorBody actorBody)
    {
        FocusLayer.RemoveChild(actorBody);
        ActorsContainer.AddChild(actorBody);
    }

    public void MoveToFocusLayer(BaseActorBody actorBody)
    {
        ActorsContainer.RemoveChild(actorBody);
        FocusLayer.AddChild(actorBody);
    }

    public IEnumerable<BaseActorBody> GetAllActorBodies()
    {
        return ActorsContainer.GetChildren<BaseActorBody>();
    }

    public IEnumerable<BaseActorBody> GetAllActorBodiesWithinView()
    {
        GameCamera gameCamera = Locator.Root.GameCamera;
        return gameCamera.FilterInView(GetAllActorBodies());
    }

    public Vector2 GetSpawnPoint(int spawnPointIndex)
    {
        var spawnPoint = SpawnPointContainer.GetChild<Marker2D>(spawnPointIndex);
        return spawnPoint?.GlobalPosition ?? new Vector2(100, 100);
    }

    public void Init(AHUD hud)
    {
        HUD = hud;
        ConnectHUDToExistingActors();
        ConnectSpawners();
    }

    public void Pause() => SetDeferred(PropertyName.ProcessMode, (long)ProcessModeEnum.Disabled);

    public void Resume() => SetDeferred(PropertyName.ProcessMode, (long)ProcessModeEnum.Inherit);

    public void RemoveActorBody(BaseActorBody actorBody)
    {
        ActorsContainer.RemoveChild(actorBody);
        HUD.UnsubscribeActorBodyEvents(actorBody);
    }

    public void OnGameStateChanged(GameState gameState)
    {
        foreach (BaseActorBody actor in ActorsContainer.GetChildren<BaseActorBody>())
            actor.OnGameStateChanged(gameState);
    }

    public void StartActionSequence(IEnumerable<BaseActor> actors)
    {
        Pause();
        foreach (BaseActor actor in actors)
        {
            if (actor.ActorBody is not BaseActorBody actorBody)
                continue;
            MoveToFocusLayer(actorBody);
            actorBody.SetForActionSequence(true);
        }

        FocusLayer.SetDeferred(PropertyName.ProcessMode, (long)ProcessModeEnum.Always);
    }

    public void StopActionSequence()
    {
        foreach (BaseActorBody actorBody in FocusLayer.GetChildren<BaseActorBody>())
        {
            MoveToActorContainer(actorBody);
            actorBody.SetForActionSequence(false);
        }
        FocusLayer.SetDeferred(PropertyName.ProcessMode, (long)ProcessModeEnum.Inherit);
        Resume();
    }

    private void OnSpawnRequested(BaseSpawner spawner)
    {
        if (!spawner.SpawnPending)
            return;
        BaseActorBody? actorBody = spawner.Spawn();
        if (actorBody != null)
            AddActorBody(actorBody);
    }

    private void ConnectHUDToExistingActors()
    {
        foreach (BaseActorBody actorBody in ActorsContainer.GetChildren<BaseActorBody>())
            HUD.SubscribeActorBodyEvents(actorBody);
    }

    private void ConnectSpawners()
    {
        foreach (BaseSpawner spawner in ActorsContainer.GetChildren<BaseSpawner>())
        {
            spawner.SpawnRequested += OnSpawnRequested;
            OnSpawnRequested(spawner);
        }
    }

    private void SetNodeReferences()
    {
        FocusLayer = GetNode<CanvasLayer>("ActionSequence");
        ActorsContainer = GetNode<CanvasLayer>("Actors");
        ColorAdjustment = GetNode<ColorAdjustment>("ColorAdjustment");
        SpawnPointContainer = GetNode<Node2D>("SpawnPoints");
        EventContainer = GetNode<Node2D>("Events");
    }
}
