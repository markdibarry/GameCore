using System.Collections.Generic;
using GameCore.Actors;
using GameCore.GUI;
using GameCore.Utility;
using Godot;

namespace GameCore.AreaScenes;

public partial class AAreaScene : Node2D
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
        foreach (var actorBody in ActorsContainer.GetChildren<AActorBody>())
            actorBody.CleanUpActorBody();
    }

    public void AddActorBody(AActorBody actorBody)
    {
        HUD.SubscribeActorBodyEvents(actorBody);
        ActorsContainer.AddChild(actorBody);
    }

    public void MoveToActorContainer(AActorBody actorBody)
    {
        FocusLayer.RemoveChild(actorBody);
        ActorsContainer.AddChild(actorBody);
    }

    public void MoveToFocusLayer(AActorBody actorBody)
    {
        ActorsContainer.RemoveChild(actorBody);
        FocusLayer.AddChild(actorBody);
    }

    public IEnumerable<AActorBody> GetAllActorBodies()
    {
        return ActorsContainer.GetChildren<AActorBody>();
    }

    public IEnumerable<AActorBody> GetAllActorBodiesWithinView()
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

    public void RemoveActorBody(AActorBody actorBody)
    {
        ActorsContainer.RemoveChild(actorBody);
        HUD.UnsubscribeActorBodyEvents(actorBody);
    }

    public void OnGameStateChanged(GameState gameState)
    {
        foreach (AActorBody actor in ActorsContainer.GetChildren<AActorBody>())
            actor.OnGameStateChanged(gameState);
    }

    public void StartActionSequence(IEnumerable<AActor> actors)
    {
        Pause();
        foreach (AActor actor in actors)
        {
            if (actor.ActorBody is not AActorBody actorBody)
                continue;
            MoveToFocusLayer(actorBody);
            actorBody.SetForActionSequence(true);
        }

        FocusLayer.SetDeferred(PropertyName.ProcessMode, (long)ProcessModeEnum.Always);
    }

    public void StopActionSequence()
    {
        foreach (AActorBody actorBody in FocusLayer.GetChildren<AActorBody>())
        {
            MoveToActorContainer(actorBody);
            actorBody.SetForActionSequence(false);
        }
        FocusLayer.SetDeferred(PropertyName.ProcessMode, (long)ProcessModeEnum.Inherit);
        Resume();
    }

    private void OnSpawnRequested(ASpawner spawner)
    {
        if (!spawner.SpawnPending)
            return;
        AActorBody? actorBody = spawner.Spawn();
        if (actorBody != null)
            AddActorBody(actorBody);
    }

    private void ConnectHUDToExistingActors()
    {
        foreach (AActorBody actorBody in ActorsContainer.GetChildren<AActorBody>())
            HUD.SubscribeActorBodyEvents(actorBody);
    }

    private void ConnectSpawners()
    {
        foreach (ASpawner spawner in ActorsContainer.GetChildren<ASpawner>())
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
