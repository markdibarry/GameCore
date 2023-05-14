using GameCore.Actors;

namespace GameCore.Items;

public class Reservation
{
    public Reservation(BaseActor actor, EquipmentSlot equipmentSlot)
    {
        EquipmentSlot = equipmentSlot;
        Actor = actor;
    }

    public EquipmentSlot EquipmentSlot { get; }
    public BaseActor Actor { get; }
}
