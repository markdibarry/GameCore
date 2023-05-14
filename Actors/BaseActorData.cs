using GameCore.Items;
using Godot;

namespace GameCore.Actors;

public abstract partial class BaseActorData : Resource
{
    public string ActorId { get; set; } = string.Empty;
    [Export] public string ActorBodyId { get; set; } = string.Empty;
    [Export] public string EquipmentSlotPresetId { get; set; } = string.Empty;

    public abstract BaseActorData Clone();
    public abstract BaseActor CreateActor(BaseInventory? externalInventory = null);
}
