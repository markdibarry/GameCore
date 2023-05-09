using System;
using System.Linq;
using GameCore.Utility;
using Godot;
using Gictionary = Godot.Collections.Dictionary;

namespace GameCore.Actors;

[Tool]
public abstract partial class ASpawner : Node2D
{
    protected static IActorDataDB ActorDataDB { get; set; } = ActorsLocator.ActorDataDB;
    private string _actorDataId = string.Empty;
    private bool _ready;
    public bool Respawn { get; set; }
    public bool OffScreen { get; set; }
    public bool CreateUnique
    {
        get => false;
        set => OnCreateUnique();
    }
    public Resource? ActorData { get; set; }
    public string ActorDataId
    {
        get => _actorDataId;
        set
        {
            _actorDataId = value;
            NotifyPropertyListChanged();
        }
    }
    public AActorBody? ActorBody { get; set; }
    public AActorBody? SpawnedActorBody { get; set; }
    public bool SpawnPending { get; set; }
    public VisibleOnScreenNotifier2D VisibleOnScreenNotifier2D { get; private set; } = null!;
    protected abstract int DefaultActorRole { get; }

    public event Action<ASpawner>? SpawnRequested;

    public override Godot.Collections.Array<Gictionary> _GetPropertyList()
    {
        Godot.Collections.Array<Gictionary> props = new()
        {
            new()
            {
                { "name", "Spawning" },
                { "type", (int)Variant.Type.Nil },
                { "usage", (int)PropertyUsageFlags.Group },
            },
            new()
            {
                { "name", nameof(Respawn) },
                { "type", (int)Variant.Type.Bool },
                { "usage", (int)PropertyUsageFlags.Default },
            },
            new()
            {
                { "name", nameof(OffScreen) },
                { "type", (int)Variant.Type.Bool },
                { "usage", (int)PropertyUsageFlags.Default },
            },
            new()
            {
                { "name", "Data" },
                { "type", (int)Variant.Type.Nil },
                { "usage", (int)PropertyUsageFlags.Group },
            },
            new()
            {
                { "name", nameof(ActorDataId) },
                { "type", (int)Variant.Type.String },
                { "usage", (int)PropertyUsageFlags.Default },
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", ActorDataDB.GetKeys().Join(",") }
            },
            new()
            {
                { "name", nameof(CreateUnique) },
                { "type", (int)Variant.Type.Bool },
                { "usage", (int)PropertyUsageFlags.Default },
            }
        };
        if (ActorData != null)
        {
            props.Add(new()
            {
                { "name", nameof(ActorData) },
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Default },
            });
        }
        return props;
    }

    public override void _Ready()
    {
        _ready = true;
        if (Engine.IsEditorHint())
        {
            ChildEnteredTree += OnChildEnteredTree;
            return;
        }

        VisibleOnScreenNotifier2D = GetNode<VisibleOnScreenNotifier2D>(nameof(VisibleOnScreenNotifier2D));
        VisibleOnScreenNotifier2D.ScreenExited += OnScreenExited;
        ActorBody = this.GetChildren<AActorBody>().FirstOrDefault();
        if (ActorBody != null)
            RemoveChild(ActorBody);
        RaiseSpawnRequested();
    }

    public override void _ExitTree()
    {
        if (SpawnedActorBody?.Actor != null)
            SpawnedActorBody.Actor.Defeated -= OnActorDefeated;
        ActorBody?.QueueFree();
    }

    public virtual AActorBody? Spawn()
    {
        if (ActorData == null && ActorDataId != string.Empty)
            ActorData = ActorDataDB.GetData<AActorData>(ActorDataId)?.Clone();
        if (ActorData == null || ActorBody == null)
            return null;

        AActor actor = ((AActorData)ActorData).CreateActor();
        AActorBody actorBody = (AActorBody)ActorBody.Duplicate();
        actorBody.SetRole(DefaultActorRole);
        actor.SetActorBody(actorBody);
        actorBody.SetActor(actor);
        actorBody.GlobalPosition = GlobalPosition;

        actor.Defeated += OnActorDefeated;
        SpawnedActorBody = actorBody;
        SpawnPending = false;
        return actorBody;
    }

    public void OnCreateUnique()
    {
        if (!Engine.IsEditorHint() || !_ready)
            return;
        ActorData = ActorDataDB.GetData<AActorData>(ActorDataId)?.Clone();
        NotifyPropertyListChanged();
    }

    protected void OnChildEnteredTree(Node node)
    {
        if (GetChildCount() > 1)
            GetChildren().First(x => x != node).QueueFree();
    }

    protected void RaiseSpawnRequested()
    {
        SpawnPending = true;
        SpawnRequested?.Invoke(this);
    }

    private void OnActorDefeated(AActor actor)
    {
        actor.Defeated -= OnActorDefeated;
        if (!Respawn)
            return;
        if (!OffScreen)
        {
            RaiseSpawnRequested();
            return;
        }
        if (!VisibleOnScreenNotifier2D.IsOnScreen())
            RaiseSpawnRequested();
    }

    private void OnScreenExited()
    {
        if (Respawn && OffScreen && !IsInstanceValid(SpawnedActorBody))
            RaiseSpawnRequested();
    }
}
