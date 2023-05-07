using GameCore.Actors;

namespace GameCore.Items;

public class Reservation
{
    public Reservation(AActor actor, EquipmentSlot equipmentSlot)
    {
        EquipmentSlot = equipmentSlot;
        Actor = actor;
    }

    public EquipmentSlot EquipmentSlot { get; }
    public AActor Actor { get; }
}
